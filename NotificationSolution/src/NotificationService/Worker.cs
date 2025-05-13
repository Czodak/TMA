using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.EventHandling;
using NotificationService.Events;
using NotificationService.Events.Base;
using NotificationService.Events.Common;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace NotificationService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;        
        private JsonSerializerOptions _jsonOptions;
        private IConnection _connection;
        private IChannel _channel;
        private readonly string _queueName;
        private readonly IServiceScopeFactory _scopeFactory;


        private static readonly Dictionary<TaskEventType, Type> _eventTypeMap = new()
        {
            { TaskEventType.TaskAssignementUpdated, typeof(TaskAssignementUpdatedEvent) },
            { TaskEventType.TaskRemoved, typeof(TaskRemovedEvent) },
            { TaskEventType.TaskStatusUpdated, typeof(TaskStatusUpdatedEvent) }
        };

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IOptions<JsonSerializerOptions> jsonOptions, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _jsonOptions = jsonOptions.Value;
            _queueName = "messages";
            _scopeFactory = serviceScopeFactory;
        }

        private async Task<IConnection> GetConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMq:Host"],
                Port = int.Parse(_configuration["RabbitMq:Port"]),
            };

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<ConnectFailureException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(
                    retryCount: 10,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), //exponential waiting
                    onRetry: (ex, time) =>
                    {
                        Console.WriteLine($"RabbitMQ not reachable yet, Retrying in {time.TotalSeconds} seconds");
                    }
                );

            return await policy.ExecuteAsync(async() => await factory.CreateConnectionAsync());
        }

        private async Task InitializeRabbitMq()
        {
            _connection = await GetConnection();

            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            Console.WriteLine($"--> Listening on message bus");

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeRabbitMq();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (ModuleHandle, ea) =>
            {
                try
                {
                    Console.WriteLine("--> event recievied!");

                    var body = ea.Body;
                    var notificationMessage = Encoding.UTF8.GetString(body.ToArray());
                    var deserializedMessage = DeserializeEvent(notificationMessage);

                    //it can't be created at  contructor level, becasue the Worker class is created only once. So injecting scoped service into singletion
                    //wont work correctly, after some time, the smtp service will throw connection issues.
                    using var scope = _scopeFactory.CreateScope();
                    var messageEventHandler = scope.ServiceProvider.GetRequiredService<IMessageEventHandler>();

                    await messageEventHandler.HandleEvent(deserializedMessage);
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fail, {ex.Message}");
                }
            };

            await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);

            // Keep the service running until cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private ITaskEvent DeserializeEvent(string message)
        {
            using var document = JsonDocument.Parse(message);

            if (!document.RootElement.TryGetProperty("EventType", out var eventTypeElement))
                throw new Exception("Missing eventType");

            var eventType = Enum.Parse<TaskEventType>(eventTypeElement.GetString());

            if (!_eventTypeMap.TryGetValue(eventType, out var concreteType))
                throw new Exception($"Unknown eventType: {eventType}");

            return (ITaskEvent)JsonSerializer.Deserialize(message, concreteType);
        }

        private async Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> rabbitMq connection shutdown");
            await Task.CompletedTask;
        }


        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel.IsOpen)
            {
                await _channel.CloseAsync();
                await _connection.CloseAsync();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}

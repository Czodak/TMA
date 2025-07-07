using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.EventHandling;
using NotificationService.Events;
using NotificationService.Events.Base;
using NotificationService.Events.Common;
using NotificationService.MessageClient;
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
        private readonly IMessageClient _messageClient;
        private readonly string _queueName;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string ERROR_QUEUE_MESSAGE = "messages_error";


        private static readonly Dictionary<TaskEventType, Type> _eventTypeMap = new()
        {
            { TaskEventType.TaskAssignementUpdated, typeof(TaskAssignementUpdatedEvent) },
            { TaskEventType.TaskRemoved, typeof(TaskRemovedEvent) },
            { TaskEventType.TaskStatusUpdated, typeof(TaskStatusUpdatedEvent) }
        };

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IOptions<JsonSerializerOptions> jsonOptions, IServiceScopeFactory serviceScopeFactory, IMessageClient messageClient)
        {
            _logger = logger;
            _configuration = configuration;
            _jsonOptions = jsonOptions.Value;
            _queueName = "messages";
            _scopeFactory = serviceScopeFactory;
            _messageClient = messageClient;
        }

        private async Task InitializeRabbitMq()
        {
            await _messageClient.CreateConnectionAsync();
            await _messageClient.DeclareQueueAsync(_queueName);
            await _messageClient.DeclareQueueAsync( ERROR_QUEUE_MESSAGE);

            Console.WriteLine($"--> Listening on message bus");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeRabbitMq();

            await _messageClient.RegisterConsumer(_queueName, async (channel, body, deliveryTag) =>
            {
                try
                {
                    Console.WriteLine("--> event recievied!");
                    
                    var notificationMessage = Encoding.UTF8.GetString(body);
                    var deserializedMessage = DeserializeEvent(notificationMessage);

                    using var scope = _scopeFactory.CreateScope();
                    var messageEventHandler = scope.ServiceProvider.GetRequiredService<IMessageEventHandler>();

                    try
                    {
                        await messageEventHandler.HandleEvent(deserializedMessage);
                    }
                    catch
                    {
                        var errorBody = Encoding.UTF8.GetBytes(notificationMessage);
                        await _messageClient.PublishAsync(ERROR_QUEUE_MESSAGE, errorBody);
                    }
                    await channel.BasicAckAsync(deliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fail, {ex.Message}");
                }
            });

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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
           await _messageClient.StopAsync(cancellationToken);
           await base.StopAsync(cancellationToken);
        }
    }
}

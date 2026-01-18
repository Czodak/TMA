using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.EventHandling;
using NotificationService.Events;
using NotificationService.Events.Base;
using NotificationService.Events.Common;
using NotificationService.MessageClient;

namespace NotificationService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IMessageClient _messageClient;
        private readonly string _queueName;
        private readonly string _errorQueueName = "messages_error";
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly Dictionary<TaskEventType, Type> _eventTypeMap = new()
        {
            { TaskEventType.TaskAssignementUpdated, typeof(TaskAssignementUpdatedEvent) },
            { TaskEventType.TaskRemoved, typeof(TaskRemovedEvent) },
            { TaskEventType.TaskStatusUpdated, typeof(TaskStatusUpdatedEvent) }
        };

        public Worker(ILogger<Worker> logger, IOptions<JsonSerializerOptions> jsonOptions, IServiceScopeFactory serviceScopeFactory, IMessageClient messageClient)
        {
            _logger = logger;
            _jsonOptions = jsonOptions.Value;
            _queueName = "messages";
            _scopeFactory = serviceScopeFactory;
            _messageClient = messageClient;
        }

        private async Task InitializeRabbitMqAsync()
        {
            await _messageClient.CreateConnectionAsync();
            await _messageClient.DeclareQueueAsync(_queueName);
            await _messageClient.DeclareQueueAsync(_errorQueueName);
            _logger.LogInformation("Listening on message bus");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeRabbitMqAsync();

            await _messageClient.RegisterConsumer(_queueName, async (channel, body, deliveryTag) =>
            {
                try
                {
                    var notificationMessage = Encoding.UTF8.GetString(body);
                    var deserializedMessage = DeserializeEvent(notificationMessage);

                    using var scope = _scopeFactory.CreateScope();
                    var messageEventHandler = scope.ServiceProvider.GetRequiredService<IMessageEventHandler>();

                    try
                    {
                        await messageEventHandler.HandleEvent(deserializedMessage);
                        await channel.BasicAckAsync(deliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to handle event, sending to error queue");
                        var errorBody = Encoding.UTF8.GetBytes(notificationMessage);
                        await _messageClient.PublishAsync(_errorQueueName, errorBody);
                        await channel.BasicAckAsync(deliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue");
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
            try
            {
                using var document = JsonDocument.Parse(message);

                if (!document.RootElement.TryGetProperty("eventType", out var eventTypeElement))
                    throw new InvalidOperationException("Missing 'EventType' property in message");

                var eventTypeString = eventTypeElement.GetString();
                if (string.IsNullOrEmpty(eventTypeString))
                    throw new InvalidOperationException("EventType cannot be null or empty");

                if (!Enum.TryParse<TaskEventType>(eventTypeString, out var eventType))
                    throw new InvalidOperationException($"Invalid EventType value: {eventTypeString}");

                if (!_eventTypeMap.TryGetValue(eventType, out var concreteType))
                    throw new InvalidOperationException($"Unknown EventType: {eventType}");

                var result = JsonSerializer.Deserialize(message, concreteType, _jsonOptions);
                if (result == null)
                    throw new InvalidOperationException($"Failed to deserialize message to type {concreteType.Name}");

                return (ITaskEvent)result;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse message as JSON", ex);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _messageClient.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
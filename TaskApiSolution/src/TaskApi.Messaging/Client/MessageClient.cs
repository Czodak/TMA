using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TaskApi.Common.Config;
using TaskApi.Common.Events.Base;

namespace TaskApi.Messaging.Client
{
    public class MessageClient : IMessageClient
    {
        private readonly RabbitMqConfig _rabbitMqConfig;

        public MessageClient(IOptions<RabbitMqConfig> rabbitMqConfig)
        {
            var config = rabbitMqConfig.Value;
            _rabbitMqConfig = config;
        }

        public async Task SendMessage(ITaskEvent taskEvent)
        {
            var factory = new ConnectionFactory 
            { 
                HostName = _rabbitMqConfig.Host,
                UserName = _rabbitMqConfig.User,
                Password = _rabbitMqConfig.Password
            };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "messages", durable: true, exclusive: false, autoDelete: false, arguments: null);
            // casting to object to serialize the actual record, not the interface

            var messageAsJson = JsonSerializer.Serialize((object)taskEvent);
            var body = Encoding.UTF8.GetBytes(messageAsJson);
            var properties = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "messages", mandatory:true, body: body, basicProperties: properties);
        }
    }
}

using System.Net.Sockets;
using Polly;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.MessageClient
{
    public class RabbitMqMessageClient : IMessageClient
    {
        private IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMqMessageClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task CreateConnectionAsync()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMq:Host"],
                Port = int.Parse(_configuration["RabbitMq:Port"]),
                UserName = _configuration["RabbitMq:User"],
                Password = _configuration["RabbitMq:Password"]
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

            _connection = await policy.ExecuteAsync(async () => await factory.CreateConnectionAsync());
            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
            _channel = await _connection.CreateChannelAsync();
        }

        public async Task DeclareQueueAsync(string queueName)
        {
            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public async Task PublishAsync(string queueName, byte[] message)
        {
            using var channel = await _connection.CreateChannelAsync();

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                body: message);
        }

        public async Task RegisterConsumer(string queueName, Func<IChannel, byte[], ulong, Task> onMessage)
        {
            if (!_connection.IsOpen)
            {
                await CreateConnectionAsync();
            }

            var channel = await _connection.CreateChannelAsync();
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                await onMessage(channel, ea.Body.ToArray(), ea.DeliveryTag);
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel.IsOpen)
            {
                await _channel.CloseAsync();
                await _connection.CloseAsync();
            }
        }

        private async Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> rabbitMq connection shutdown");
            await Task.CompletedTask;
        }
    }
}

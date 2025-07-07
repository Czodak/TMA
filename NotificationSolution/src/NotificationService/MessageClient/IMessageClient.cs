using RabbitMQ.Client;

namespace NotificationService.MessageClient
{
    public interface IMessageClient
    {
        Task CreateConnectionAsync();
        Task DeclareQueueAsync(string queueName);
        Task PublishAsync(string queueName, byte[] message);
        //channel is passed to ensure multi thread safety. IConnection is thread safety so there can be one, but channel isnt.
        Task RegisterConsumer(string queueName, Func<IChannel, byte[], ulong, Task> onMessage);
        Task StopAsync(CancellationToken cancellationToken);
    }
}

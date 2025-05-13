using TaskApi.Common.Events.Base;

namespace TaskApi.Messaging.Client
{
    public interface IMessageClient
    {
        Task SendMessage(ITaskEvent taskEvent);
    }
}

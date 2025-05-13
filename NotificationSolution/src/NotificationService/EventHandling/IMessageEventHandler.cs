using NotificationService.Events.Base;

namespace NotificationService.EventHandling
{
    public interface IMessageEventHandler
    {
        Task HandleEvent(ITaskEvent taskEvent);
    }
}

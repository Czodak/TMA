using NotificationService.Events.Common;

namespace NotificationService.Events.Base
{
    public interface ITaskEvent
    {
        TaskEventType EventType { get; }   
    }
}

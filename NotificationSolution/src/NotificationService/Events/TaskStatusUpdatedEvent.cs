using NotificationService.Events.Base;
using NotificationService.Events.Common;

namespace NotificationService.Events
{
    public record TaskStatusUpdatedEvent(string EmailDestination, string NewStatus) : ITaskEvent
    {
        public TaskEventType EventType => TaskEventType.TaskStatusUpdated;
    }
}

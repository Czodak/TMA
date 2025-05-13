using NotificationService.Events.Base;
using NotificationService.Events.Common;

namespace NotificationService.Events
{
    public record TaskAssignementUpdatedEvent(string EmailDestination, string TaskTitle) : ITaskEvent
    {
        public TaskEventType EventType => TaskEventType.TaskAssignementUpdated;
    }
}

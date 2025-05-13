using TaskApi.Common.Events.Base;
using TaskApi.Common.Events.Common;

namespace TaskApi.Common.Events
{
    public record TaskAssignementUpdatedEvent(string EmailDestination, string TaskTitle) : ITaskEvent
    {
        public TaskEventType EventType => TaskEventType.TaskAssignementUpdated;
    }
}

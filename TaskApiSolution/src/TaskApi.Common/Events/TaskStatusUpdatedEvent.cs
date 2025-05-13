using TaskApi.Common.Events.Base;
using TaskApi.Common.Events.Common;

namespace TaskApi.Common.Events
{
    public record TaskStatusUpdatedEvent(string EmailDestination, string NewStatus) : ITaskEvent
    {
        public TaskEventType EventType => TaskEventType.TaskStatusUpdated;
    }
}

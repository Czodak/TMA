using TaskApi.Common.Events.Common;

namespace TaskApi.Common.Events.Base
{
    public interface ITaskEvent
    {
        TaskEventType EventType { get; }   
    }
}

using TaskApi.Common.Events;
using TaskApi.Common.HttpClients.Auth;
using TaskApi.Data.Entities;

namespace TaskApi.BusinessLogic.MessagesCreator
{
    public interface IMessageFactory
    {
        Task<TaskStatusUpdatedEvent> GetTaskStatusUpdatedEvent(Tasks task);
        TaskAssignementUpdatedEvent GetTaskAssignementUpdatedEvent(Tasks task, UserInfo user);
        Task<TaskRemovedEvent> GetTaskRemovedEvent(Tasks task);
    }
}

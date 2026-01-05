using TaskApi.BusinessLogic.AuthApiService;
using TaskApi.BusinessLogic.MessagesCreator;
using TaskApi.Common.Events;
using TaskApi.Common.HttpClients.Auth;
using TaskApi.Data.Entities;

namespace TaskApi.BusinessLogic.MessagesFactory
{
    public class MessageFactory : IMessageFactory
    {
        private readonly IAuthApiService _authApiService;

        public MessageFactory(IAuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        public TaskAssignementUpdatedEvent GetTaskAssignementUpdatedEvent(Tasks task, UserInfo user)
        {
            if(string.IsNullOrEmpty(user?.Email))
            {
                return null;
            }

            return new TaskAssignementUpdatedEvent(user.Email, task.Title);
        }

        public async Task<TaskRemovedEvent> GetTaskRemovedEvent(Tasks task)
        {
            var user = await GetUserById(task);
            if (user == null)
            {
                return null;
            }

            var taskRemovedEvent = new TaskRemovedEvent(user.Email, task.Title);
            return taskRemovedEvent;
        }

        public async Task<TaskStatusUpdatedEvent> GetTaskStatusUpdatedEvent(Tasks task)
        {
            var user = await GetUserById(task);
            if (user == null)
            {
                return null;
            }

            var taskStatusUpdatedEvent = new TaskStatusUpdatedEvent(user.Email, task.Status.ToString());
            return taskStatusUpdatedEvent;
        }

        private async Task<UserInfo> GetUserById(Tasks task)
        {
            if (task.CurrentlyAssignedUserId == null)
            {
                // no one to recieve an email
                return null;
            }

            var user = await _authApiService.GetUserById(task.CurrentlyAssignedUserId.Value);

            return user;
        }
    }
}

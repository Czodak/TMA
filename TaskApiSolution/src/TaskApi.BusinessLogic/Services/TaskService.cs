using TaskApi.BusinessLogic.AuthApiService;
using TaskApi.BusinessLogic.Extensions;
using TaskApi.BusinessLogic.MessagesCreator;
using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Contracts.Response;
using TaskApi.Common.Exceptions;
using TaskApi.Common.HttpClients.Auth;
using TaskApi.Contracts.Request;
using TaskApi.Data.Entities;
using TaskApi.Data.Repositories;
using TaskApi.Messaging.Client;

namespace TaskApi.BusinessLogic.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMessageClient _messageClient;
        private readonly IMessageFactory _messageFactory;
        private readonly IAuthApiService _authApiService;

        public TaskService(ITaskRepository taskRepository, IAuthApiService authService, IMessageClient messageClient, IMessageFactory messageFactory)
        {
            _taskRepository = taskRepository;
            _authApiService = authService;
            _messageClient = messageClient;
            _messageFactory = messageFactory;
        }

        public async Task CreateTaskAsync(CreateTaskRequest createTaskRequest)
        {
            var currentlyLoggenInUser = await _authApiService.MeAsync();

            if (currentlyLoggenInUser == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }
            createTaskRequest.CreatorId = currentlyLoggenInUser.Id;
            await _taskRepository.SaveTaskAsync(createTaskRequest);
        }

        public async Task DeleteTask(int taskId)
        {
            var existingTask = await GetTaskById(taskId);

            await _taskRepository.DeleteTask(existingTask);

            var message = await _messageFactory.GetTaskRemovedEvent(existingTask);
            await _messageClient.SendMessage(message);
        }

        public async Task<IEnumerable<ReadTaskDto>> GetAllTasksAsync()
        {
            return await _taskRepository.GetAllTasksAsync();
        }

        public async Task<ReadTaskDto> GetTaskByIdAsync(int taskId)
        {
            if (taskId < 0)
            {
                throw new ArgumentException("TaskId cant be less than 0");
            }
            return await _taskRepository.GetTaskDtoByIdAsync(taskId);
        }

        public async Task UpdateTaskAsync(UpdateTaskDto updateTaskDto)
        {
            var existingTask = await GetTaskById(updateTaskDto.Id);

            if(UpdateTaskExtension.ApplyUpdate(existingTask, updateTaskDto))
            {
                await _taskRepository.UpdateTaskAsync(existingTask);                
            }
        }

        public async Task UpdateTaskStatusAsync(UpdateTaskStatusDto updateTaskStatusDto)
        {
            var existingTask = await GetTaskById(updateTaskStatusDto.Id);

            if (existingTask.Status == updateTaskStatusDto.Status) return;

            await _taskRepository.UpdateTaskStatusAsync(existingTask, updateTaskStatusDto.Status);
            var taskStatusUpdatedEvent = await _messageFactory.GetTaskStatusUpdatedEvent(existingTask);
            await _messageClient.SendMessage(taskStatusUpdatedEvent);
        }

        private async Task<Tasks> GetTaskById(int taskId)
        {
            var existingTask = await _taskRepository.GetTaskByIdAsync(taskId);
            if (existingTask == null)
            {
                throw new NotFoundException("Task with given id was not found");
            }
            return existingTask;
        }
        public async Task ChangeTaskAssignment(UpdateTaskAssigmentDto updateTaskAssigmentDto)
        {
            var existingTask = await GetTaskById(updateTaskAssigmentDto.Id);

            if (updateTaskAssigmentDto.NewAssignedUser == existingTask.CurrentlyAssignedUserId)
            {
                return;
            }

            UserInfo user = null;
            if (updateTaskAssigmentDto.NewAssignedUser != null)
            {
                user = await _authApiService.GetUserById(updateTaskAssigmentDto.NewAssignedUser.Value);
                if(user == null)
                {
                    throw new NotFoundException("User with given id was not found");
                }
            }
            
            await _taskRepository.UpdateAssignmentAsync(existingTask, updateTaskAssigmentDto);
            
            if(user == null)
            {
                return;
            }

            var message = _messageFactory.GetTaskAssignementUpdatedEvent(existingTask, user);
            await _messageClient.SendMessage(message);
        }
    }
}

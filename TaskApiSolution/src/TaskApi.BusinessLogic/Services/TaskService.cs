using TaskApi.BusinessLogic.AuthApiService;
using TaskApi.BusinessLogic.Extensions;
using TaskApi.BusinessLogic.MessagesCreator;
using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Contracts.Response;
using TaskApi.Common.Exceptions;
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

        public async Task<ReadTaskDto> CreateTaskAsync(CreateTaskRequest createTaskRequest, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(createTaskRequest);

            var currentlyLoggedInUser = await _authApiService.MeAsync();

            createTaskRequest.CreatorId = currentlyLoggedInUser.Id;
            var taskId = await _taskRepository.SaveTaskAsync(createTaskRequest, cancellationToken);

            var createdTask = await _taskRepository.GetTaskDtoByIdAsync(taskId, cancellationToken);
            return createdTask ?? throw new InvalidOperationException($"Failed to retrieve created task with id {taskId}");
        }

        public async Task DeleteTaskAsync(int taskId, CancellationToken cancellationToken)
        {
            var existingTask = await GetTaskById(taskId, cancellationToken);

            await _taskRepository.DeleteTask(existingTask, cancellationToken);

            var message = await _messageFactory.GetTaskRemovedEvent(existingTask);
            await _messageClient.SendMessage(message);
        }

        public async Task<IEnumerable<ReadTaskDto>> GetAllTasksAsync(CancellationToken cancellationToken)
        {
            return await _taskRepository.GetAllTasksAsync(cancellationToken);
        }

        public async Task<ReadTaskDto> GetTaskByIdAsync(int taskId, CancellationToken cancellationToken)
        {
            var dto = await _taskRepository.GetTaskDtoByIdAsync(taskId, cancellationToken);
            return dto ?? throw new NotFoundException($"Task with id {taskId} was not found");
        }

        public async Task UpdateTaskAsync(UpdateTaskDto updateTaskDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(updateTaskDto);

            var existingTask = await GetTaskById(updateTaskDto.Id, cancellationToken);

            if (UpdateTaskExtension.ApplyUpdate(existingTask, updateTaskDto))
            {
                await _taskRepository.UpdateTaskAsync(existingTask, cancellationToken);
            }
        }

        public async Task UpdateTaskStatusAsync(UpdateTaskStatusDto updateTaskStatusDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(updateTaskStatusDto);

            var existingTask = await GetTaskById(updateTaskStatusDto.Id, cancellationToken);

            if (existingTask.Status == updateTaskStatusDto.Status) return;

            await _taskRepository.UpdateTaskStatusAsync(existingTask, updateTaskStatusDto.Status, cancellationToken);
            var taskStatusUpdatedEvent = await _messageFactory.GetTaskStatusUpdatedEvent(existingTask);
            await _messageClient.SendMessage(taskStatusUpdatedEvent);
        }

        public async Task ChangeTaskAssignmentAsync(UpdateTaskAssigmentDto updateTaskAssigmentDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(updateTaskAssigmentDto);

            var existingTask = await GetTaskById(updateTaskAssigmentDto.Id, cancellationToken);

            if (updateTaskAssigmentDto.NewAssignedUser == existingTask.CurrentlyAssignedUserId)
            {
                return;
            }

            var user = await _authApiService.GetUserById(updateTaskAssigmentDto.NewAssignedUser);
            await _taskRepository.UpdateAssignmentAsync(existingTask, updateTaskAssigmentDto, cancellationToken);

            var message = _messageFactory.GetTaskAssignementUpdatedEvent(existingTask, user);
            await _messageClient.SendMessage(message);
        }

        public async Task ClearTaskAssignmentAsync(int taskId, CancellationToken cancellationToken)
        {
            var existingTask = await GetTaskById(taskId, cancellationToken);
            if (existingTask.CurrentlyAssignedUserId == null)
            {
                return;
            }

            existingTask.CurrentlyAssignedUserId = null;
            await _taskRepository.UpdateTaskAsync(existingTask, cancellationToken);
        }

        private async Task<Tasks> GetTaskById(int taskId, CancellationToken cancellationToken)
        {
            var existingTask = await _taskRepository.GetTaskByIdAsync(taskId, cancellationToken);
            return existingTask ?? throw new NotFoundException($"Task with id {taskId} was not found");
        }
    }
}
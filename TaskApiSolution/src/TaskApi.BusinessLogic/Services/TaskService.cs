using TaskApi.BusinessLogic.Extensions;
using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Contracts.Response;
using TaskApi.Common.Exceptions;
using TaskApi.Common.HttpClients.Auth;
using TaskApi.Contracts.Request;
using TaskApi.Data.Repositories;

namespace TaskApi.BusinessLogic.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly AuthApiClient _authClient;

        public TaskService(ITaskRepository taskRepository, AuthApiClient authClient)
        {
            _taskRepository = taskRepository;
            _authClient = authClient;
        }

        public async Task CreateTaskAsync(CreateTaskRequest createTaskRequest)
        {
            var currentlyLoggenInUser = await _authClient.MeAsync();

            if (currentlyLoggenInUser == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }
            createTaskRequest.CreatorId = currentlyLoggenInUser.Id;
            await _taskRepository.SaveTaskAsync(createTaskRequest);
        }

        public async Task DeleteTask(int taskId)
        {
            await _taskRepository.DeleteTask(taskId);
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
            var existingTask = await _taskRepository.GetTaskByIdAsync(updateTaskDto.Id);
            if(existingTask == null)
            {
                throw new NotFoundException("Task with given id was not found");
            }

            if(UpdateTaskExtension.ApplyUpdate(existingTask, updateTaskDto))
            {
                await _taskRepository.UpdateTaskAsync(existingTask);
            }
        }
    }
}

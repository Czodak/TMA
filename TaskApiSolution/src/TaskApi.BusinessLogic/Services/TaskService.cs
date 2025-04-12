using TaskApi.Common.Contracts.Response;
using TaskApi.Contracts.Request;
using TaskApi.Data.Repositories;

namespace TaskApi.BusinessLogic.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task CreateTaskAsync(CreateTaskRequest createTaskRequest)
        {
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
            return await _taskRepository.GetTaskByIdAsync(taskId);
        }
    }
}

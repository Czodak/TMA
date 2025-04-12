using TaskApi.Common.Contracts.Response;
using TaskApi.Contracts.Request;

namespace TaskApi.BusinessLogic.Services
{
    public interface ITaskService
    {
        Task CreateTaskAsync(CreateTaskRequest createTaskRequest);
        Task<IEnumerable<ReadTaskDto>> GetAllTasksAsync();
        Task<ReadTaskDto> GetTaskByIdAsync(int taskId);
        Task DeleteTask(int taskId);
    }
}

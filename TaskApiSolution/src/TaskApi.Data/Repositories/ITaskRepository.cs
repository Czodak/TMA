using TaskApi.Common.Contracts.Response;
using TaskApi.Contracts.Request;

namespace TaskApi.Data.Repositories
{
    public interface ITaskRepository
    {
        Task<IEnumerable<ReadTaskDto>> GetAllTasksAsync();
        Task SaveTaskAsync(CreateTaskRequest task);
        Task<ReadTaskDto> GetTaskByIdAsync(int id);
        Task DeleteTask(int taskId);
    }
}

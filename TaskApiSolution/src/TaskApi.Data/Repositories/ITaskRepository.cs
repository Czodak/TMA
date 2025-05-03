using TaskApi.Common.Contracts.Response;
using TaskApi.Contracts.Request;
using TaskApi.Data.Entities;

namespace TaskApi.Data.Repositories
{
    public interface ITaskRepository
    {
        Task<IEnumerable<ReadTaskDto>> GetAllTasksAsync();
        Task SaveTaskAsync(CreateTaskRequest task);
        Task<ReadTaskDto> GetTaskDtoByIdAsync(int id);
        Task DeleteTask(int taskId);
        Task<Tasks> GetTaskByIdAsync(int id);

        Task UpdateTaskAsync(Tasks task);
    }
}

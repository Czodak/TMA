using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Contracts.Response;
using TaskApi.Common.Enums;
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
        Task DeleteTask(Tasks task);
        Task<Tasks> GetTaskByIdAsync(int id);

        Task UpdateTaskAsync(Tasks task);
        Task UpdateTaskStatusAsync(Tasks task, TaskStatuses newStatus);
        Task UpdateAssignmentAsync(Tasks existingTask, UpdateTaskAssigmentDto updateTaskAssigmentDto);
    }
}

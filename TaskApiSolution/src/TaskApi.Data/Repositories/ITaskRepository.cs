using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Contracts.Response;
using TaskApi.Common.Enums;
using TaskApi.Contracts.Request;
using TaskApi.Data.Entities;

namespace TaskApi.Data.Repositories
{
    public interface ITaskRepository
    {
        Task<IEnumerable<ReadTaskDto>> GetAllTasksAsync(CancellationToken cancellationToken);
        Task<int> SaveTaskAsync(CreateTaskRequest task, CancellationToken cancellationToken);
        Task<ReadTaskDto> GetTaskDtoByIdAsync(int id, CancellationToken cancellationToken);
        Task DeleteTask(int taskId, CancellationToken cancellationToken);
        Task DeleteTask(Tasks task, CancellationToken cancellationToken);
        Task<Tasks> GetTaskByIdAsync(int id, CancellationToken cancellationToken);

        Task UpdateTaskAsync(Tasks task, CancellationToken cancellationToken);
        Task UpdateTaskStatusAsync(Tasks task, TaskStatuses newStatus, CancellationToken cancellationToken);
        Task UpdateAssignmentAsync(Tasks existingTask, UpdateTaskAssigmentDto updateTaskAssigmentDto, CancellationToken cancellationToken);
    }
}

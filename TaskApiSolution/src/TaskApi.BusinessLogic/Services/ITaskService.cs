using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Contracts.Response;
using TaskApi.Contracts.Request;

namespace TaskApi.BusinessLogic.Services
{
    public interface ITaskService
    {
        Task<ReadTaskDto> CreateTaskAsync(CreateTaskRequest createTaskRequest, CancellationToken cancellationToken);
        Task<IEnumerable<ReadTaskDto>> GetAllTasksAsync(CancellationToken cancellationToken);
        Task<ReadTaskDto> GetTaskByIdAsync(int taskId, CancellationToken cancellationToken);
        Task DeleteTaskAsync(int taskId, CancellationToken cancellationToken);

        Task UpdateTaskAsync(UpdateTaskDto updateTaskDto, CancellationToken cancellationToken);
        Task UpdateTaskStatusAsync(UpdateTaskStatusDto updateTaskStatusDto, CancellationToken cancellationToken);
        Task ChangeTaskAssignmentAsync(UpdateTaskAssigmentDto updateTaskAssigmentDto, CancellationToken cancellationToken);
        Task ClearTaskAssignmentAsync(int taskId, CancellationToken cancellationToken);
    }
}

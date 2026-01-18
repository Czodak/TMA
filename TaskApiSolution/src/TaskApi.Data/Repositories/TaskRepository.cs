using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Contracts.Response;
using TaskApi.Common.Enums;
using TaskApi.Common.Exceptions;
using TaskApi.Contracts.Request;
using TaskApi.Data.DatabaseContext;
using TaskApi.Data.Entities;

namespace TaskApi.Data.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public TaskRepository(ApplicationDbContext dbContext, IMapper mapper) : this(dbContext)
        {
            _mapper = mapper;
        }

        public TaskRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ReadTaskDto>> GetAllTasksAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Tasks
                .ProjectTo<ReadTaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<ReadTaskDto> GetTaskDtoByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbContext.Tasks
                .Where(task => task.Id == id)
                .ProjectTo<ReadTaskDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Tasks> GetTaskByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
        }

        public async Task<int> SaveTaskAsync(CreateTaskRequest task, CancellationToken cancellationToken)
        {
            var taskEntity = _mapper.Map<Tasks>(task);
            await _dbContext.Tasks.AddAsync(taskEntity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return taskEntity.Id;
        }

        public async Task DeleteTask(int taskId, CancellationToken cancellationToken)
        {
            var task = await _dbContext.Tasks.FindAsync(taskId, cancellationToken);
            if(task == null)
            {
                throw new NotFoundException($"Task with id {taskId} not found");
            }
            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteTask(Tasks task, CancellationToken cancellationToken)
        {
            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateTaskAsync(Tasks task, CancellationToken cancellationToken)
        {
            task.LastUpdatedAt = DateTime.UtcNow;
            _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateTaskStatusAsync(Tasks task, TaskStatuses newStatus, CancellationToken cancellationToken)
        {
            _dbContext.Tasks.Attach(task);
            task.Status = newStatus;
            task.LastUpdatedAt = DateTime.UtcNow;
            _dbContext.Entry(task).Property(p => p.Status).IsModified = true;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAssignmentAsync(Tasks existingTask, UpdateTaskAssigmentDto updateTaskAssigmentDto, CancellationToken cancellationToken)
        {
            _dbContext.Tasks.Attach(existingTask);
            existingTask.CurrentlyAssignedUserId = updateTaskAssigmentDto.NewAssignedUser;
            _dbContext.Entry(existingTask).Property(p => p.CurrentlyAssignedUserId).IsModified = true;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TaskApi.Common.Contracts.Response;
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

        public async Task<IEnumerable<ReadTaskDto>> GetAllTasksAsync()
        {
            return await _dbContext.Tasks
                .ProjectTo<ReadTaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ReadTaskDto> GetTaskByIdAsync(int id)
        {
            var t = await _dbContext.Tasks.Where(task => task.Id == id).FirstOrDefaultAsync();
            return await _dbContext.Tasks
                .Where(task => task.Id == id)
                .ProjectTo<ReadTaskDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task SaveTaskAsync(CreateTaskRequest task)
        {
            var taskEntity = _mapper.Map<Tasks>(task);
            await _dbContext.Tasks.AddAsync(taskEntity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTask(int taskId)
        {
            var task = await _dbContext.Tasks.FindAsync(taskId);
            if(task == null)
            {
                throw new NotFoundException($"Task with id {taskId} not found");
            }
            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();
        }
    }
}

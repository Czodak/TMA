using TaskApi.Common.Contracts.Request;
using TaskApi.Data.Entities;

namespace TaskApi.BusinessLogic.Extensions
{
    public static class UpdateTaskExtension
    {
        public static bool ApplyUpdate(this Tasks task, UpdateTaskDto updateTaskDto)
        {
            var isChanged = false;

            if(updateTaskDto.Title is not null)
            {
                task.Title = updateTaskDto.Title;
                isChanged = true;
            }

            if (updateTaskDto.Priority is int priority)
            {
                task.Priority = priority;
                isChanged = true;
            }

            if (updateTaskDto.Description is not null)
            {
                task.Description = updateTaskDto.Description;
                isChanged = true;
            }

            return isChanged;
        }
    }
}

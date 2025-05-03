using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskApi.BusinessLogic.Services;
using TaskApi.Common.Contracts.Request;
using TaskApi.Contracts.Request;

namespace TaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTask()
        {
            return Ok(await _taskService.GetAllTasksAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById([FromRoute] int id)
        {
            return Ok(await _taskService.GetTaskByIdAsync(id));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest createRequest)
        {
            await _taskService.CreateTaskAsync(createRequest);
            return Ok();
        }

        [HttpDelete("{taskId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTask([FromRoute] int taskId)
        {
            await _taskService.DeleteTask(taskId);
            return Ok();
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDto updateTaskDto)
        {
            await _taskService.UpdateTaskAsync(updateTaskDto);
            return Ok();
        }
    }
}

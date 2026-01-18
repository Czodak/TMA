using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskApi.BusinessLogic.Services;
using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Contracts.Response;
using TaskApi.Contracts.Request;

namespace TaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Retrieves all tasks
        /// </summary>
        /// <returns>List of all tasks</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReadTaskDto>>> GetAllTasks(CancellationToken cancellationToken)
        {
            var tasks = await _taskService.GetAllTasksAsync(cancellationToken);
            return Ok(tasks);
        }

        /// <summary>
        /// Retrieves a task by ID
        /// </summary>
        /// <param name="id">The task ID</param>
        /// <returns>The task details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReadTaskDto>> GetTaskById([FromRoute] int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
                return BadRequest("Task ID must be greater than 0");

            var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);
            if (task == null)
                return NotFound($"Task with ID {id} not found");

            return Ok(task);
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        /// <param name="createRequest">The task creation request</param>
        /// <returns>The created task</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReadTaskDto>> CreateTask([FromBody] CreateTaskRequest createRequest, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTask = await _taskService.CreateTaskAsync(createRequest, cancellationToken);
            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }

        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="taskId">The task ID to delete</param>
        [HttpDelete("{taskId:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteTask([FromRoute] int taskId, CancellationToken cancellationToken)
        {
            if (taskId <= 0)
                return BadRequest("Task ID must be greater than 0");

            await _taskService.DeleteTaskAsync(taskId, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Updates task details
        /// </summary>
        /// <param name="updateTaskDto">The task update request</param>
        [HttpPatch]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDto updateTaskDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _taskService.UpdateTaskAsync(updateTaskDto, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Updates task status
        /// </summary>
        /// <param name="updateTaskDto">The status update request</param>
        [HttpPatch("status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] UpdateTaskStatusDto updateTaskDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _taskService.UpdateTaskStatusAsync(updateTaskDto, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Assigns a task to a user
        /// </summary>
        /// <param name="updateTaskAssignmentDto">The assignment update request</param>
        [HttpPatch("assign")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangeTaskAssignment([FromBody] UpdateTaskAssigmentDto updateTaskAssignmentDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _taskService.ChangeTaskAssignmentAsync(updateTaskAssignmentDto, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Clears task assignment
        /// </summary>
        /// <param name="taskId">The task ID</param>
        [HttpPatch("clearAssignment")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClearTaskAssignment([FromQuery] int taskId, CancellationToken cancellationToken)
        {
            if (taskId <= 0)
                return BadRequest("Task ID must be greater than 0");

            await _taskService.ClearTaskAssignmentAsync(taskId, cancellationToken);
            return NoContent();
        }
    }
}
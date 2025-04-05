using Microsoft.AspNetCore.Mvc;
using TaskApi.Data.DatabaseContext;

namespace TaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheck : ControllerBase
    {

        private readonly ApplicationDbContext _dbContext;

        public HealthCheck(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var taskCount = _dbContext.Tasks.First().Title;
            return Ok($"Greetings from task api, taskCount : {taskCount}");
        }
    }
}

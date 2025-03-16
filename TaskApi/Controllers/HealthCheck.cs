using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheck : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheck : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Greetings from auth api :)))");
        }
    }
}

using AuthApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheck : ControllerBase
    {
        private readonly AuthDbContext _context;

        public HealthCheck(AuthDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var usersCount = _context.Users.Count();
            return Ok($"Greetings from auth api :))), usersCount : {usersCount}");
        }
    }
}

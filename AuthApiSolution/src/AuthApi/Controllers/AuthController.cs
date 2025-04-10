using System.Security.Claims;
using AuthApi.BusinessLogic.Services;
using AuthApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest registerUserRequest)
        {
            var jwt = await _authService.RegisterAsync(registerUserRequest);
            return Ok(new { Token = jwt });            
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest loginRequest)
        {
            var jwt = await _authService.LoginAsync(loginRequest);
            return Ok(new { Token = jwt });    
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userInfo = await _authService.GetCurrentlyLoggedInUser(userId);
            return Ok(userInfo);
        }
    }
}

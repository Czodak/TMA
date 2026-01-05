using System.Net;
using System.Security.Claims;
using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Contracts.Requests;
using AuthApi.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers;

[Route("api/user")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRegistrationService _registrationService;
    private readonly ILoginService _loginService;

    public AuthController(IAuthService authService, IRegistrationService registrationService, ILoginService loginService)
    {
        _authService = authService;
        _registrationService = registrationService;
        _loginService = loginService;
    }

    [HttpPost("register")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest registerUserRequest)
    {
        var jwt = await _registrationService.RegisterAsync(registerUserRequest);
        return Ok(new { Token = jwt });            
    }

    [HttpPost("login")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest loginRequest)
    {
        var jwt = await _loginService.LoginAsync(loginRequest);
        return Ok(new { Token = jwt });    
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<UserInfo>> Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userInfo = await _authService.GetCurrentlyLoggedInUser(userId);
        return Ok(userInfo);
    }

    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<UserInfo>>> GetAllUsers()
    {
        var usersInfo = await _authService.GetAllUserInfo();
        return Ok(usersInfo);
    }
    
    
    [HttpGet("userExists")]
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<UserInfo>> UserExists([FromQuery] string email)
    {
        var usersInfo = await _authService.UserExists(email);
        return Ok(usersInfo);
    }

    [HttpGet("get", Name ="GetUserById")]
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<UserInfo>> GetUserById([FromQuery] Guid id)
    {
        var userInfo = await _authService.GetUserById(id);
        if(userInfo == null)
        {
            return NotFound();
        }
        return Ok(userInfo);
    }
}

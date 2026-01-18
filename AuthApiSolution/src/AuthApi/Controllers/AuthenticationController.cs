using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Contracts.Requests;
using AuthApi.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AuthApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly ILoginService _loginService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IRegistrationService registrationService,
        ILoginService loginService,
        ILogger<AuthenticationController> logger)
    {
        _registrationService = registrationService;
        _loginService = loginService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest registerUserRequest, CancellationToken cancellationToken)
    {
        var jwt = await _registrationService.RegisterAsync(registerUserRequest, cancellationToken);
        return Ok(new TokenResponse(jwt));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest loginRequest, CancellationToken cancellationToken)
    {
        var jwt = await _loginService.LoginAsync(loginRequest, cancellationToken);
        if (string.IsNullOrWhiteSpace(jwt))
        {
            _logger.LogWarning("Login failed for {Email}", loginRequest?.Email);
            return Unauthorized();
        }

        return Ok(new TokenResponse(jwt));
    }
}
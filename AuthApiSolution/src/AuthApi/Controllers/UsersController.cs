using System.Net;
using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Contracts.Responses;
using AuthApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers;

[Route("api/users")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IAuthService authService, ILogger<UsersController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfo), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<UserInfo>> Me(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Authenticated request missing NameIdentifier claim.");
            return Unauthorized();
        }

        var userInfo = await _authService.GetCurrentlyLoggedInUser(userId, cancellationToken);
        return Ok(userInfo);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UserInfo>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<UserInfo>>> GetAllUsers(CancellationToken cancellationToken)
    {
        var usersInfo = await _authService.GetAllUserInfo(cancellationToken);
        return Ok(usersInfo);
    }

    [HttpGet("exists")]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<bool>> UserExists([FromQuery] string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("email is required");
        }

        var exists = await _authService.UserExists(email, cancellationToken);
        return Ok(exists);
    }

    [HttpGet("{id:guid}", Name = "GetUserById")]
    [ProducesResponseType(typeof(UserInfo), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<UserInfo>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var userInfo = await _authService.GetUserById(id, cancellationToken);
        if (userInfo == null)
        {
            return NotFound();
        }
        return Ok(userInfo);
    }
}
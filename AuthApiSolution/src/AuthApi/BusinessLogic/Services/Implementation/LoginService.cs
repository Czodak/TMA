using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Contracts.Requests;
using AuthApi.Data.Entities;
using AuthApi.Data.Repositories;
using AuthApi.Exceptions;

namespace AuthApi.BusinessLogic.Services.Implementation;

public class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginService> _logger;

    public LoginService(IUserRepository userRepository, IJwtService jwtService, IPasswordHasher passwordHasher, ILogger<LoginService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<string> LoginAsync(LoginUserRequest loginUserDto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(loginUserDto.Email, cancellationToken);
        if (user == null || !IsPasswordValid(loginUserDto, user))
        {
            _logger.LogWarning("Login failed for email {Email}", loginUserDto?.Email);
            throw new AuthenticationFailedException("Invalid email or password");
        }

        return _jwtService.GenerateJwt(user);
    }

    private bool IsPasswordValid(LoginUserRequest loginUserDto, UserEntity user)
    {
        return _passwordHasher.Verify(loginUserDto.Password, user.PasswordHash);
    }
}

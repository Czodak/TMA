using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Contracts.Requests;
using AuthApi.Data.Entities;
using AuthApi.Data.Repositories;
using System.Text.RegularExpressions;

namespace AuthApi.BusinessLogic.Services.Implementation;

public class RegistrationService : IRegistrationService
{
    private static readonly Regex EmailValidationRegex = new(
        "^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(IUserRepository userRepository, IJwtService jwtService, IPasswordHasher passwordHasher, ILogger<RegistrationService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<string> RegisterAsync(RegisterUserRequest registerUserDto, CancellationToken cancellationToken)
    {
        if (registerUserDto == null) throw new ArgumentNullException(nameof(registerUserDto));
        if (!IsEmailValid(registerUserDto.Email))
        {
            throw new ArgumentException("Invalid email address");
        }

        var userExists = await _userRepository.CheckExistenceByEmail(registerUserDto.Email, cancellationToken);
        if (userExists)
        {
            throw new ArgumentException("Email is already taken");
        }

        var user = new UserEntity
        {
            Email = registerUserDto.Email,
            PasswordHash = _passwordHasher.Hash(registerUserDto.Password),
            Name = registerUserDto.Name,
            LastName = registerUserDto.LastName
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var persisted = await _userRepository.GetByEmailAsync(user.Email, cancellationToken);
        if (persisted == null)
        {
            _logger.LogError("User persisted but cannot be retrieved by email {Email}", user.Email);
            throw new InvalidOperationException("Failed to create user");
        }

        return _jwtService.GenerateJwt(persisted);
    }

    private static bool IsEmailValid(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        return EmailValidationRegex.IsMatch(email);
    }
}
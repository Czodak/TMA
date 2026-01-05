using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Contracts.Requests;
using AuthApi.Data.Entities;
using AuthApi.Data.Repositories;
using System.Text.RegularExpressions;

namespace AuthApi.BusinessLogic.Services.Implementation;

public class RegistrationService : IRegistrationService
{
    private readonly Regex EmailValidatonRegex;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RegistrationService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        EmailValidatonRegex = new Regex("^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$");
        _jwtService = jwtService;
    }

    public async Task<string> RegisterAsync(RegisterUserRequest registerUserDto)
    {
        if (!IsEmailValid(registerUserDto.Email))
        {
            throw new ArgumentException("Invalid email address");
        }

        var userExists = await _userRepository.CheckExistenceByEmail(registerUserDto.Email);

        if (userExists)
        {
            throw new ArgumentException("Email is already taken");
        }


        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password);

        var user = new UserEntity
        {
            Email = registerUserDto.Email,
            PasswordHash = hashedPassword,
            Name = registerUserDto.Name,
            LastName = registerUserDto.LastName
        };

        await _userRepository.AddAsync(user);

        return _jwtService.GenerateJwt(user);
    }

    private bool IsEmailValid(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;

        if (EmailValidatonRegex.IsMatch(email)) return true;
        return false;
    }
}

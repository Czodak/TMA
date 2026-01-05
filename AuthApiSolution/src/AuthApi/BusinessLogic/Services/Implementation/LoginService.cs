using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Contracts.Requests;
using AuthApi.Data.Repositories;

namespace AuthApi.BusinessLogic.Services.Implementation;

public class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LoginService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<string> LoginAsync(LoginUserRequest loginUserDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginUserDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginUserDto.Password, user.PasswordHash))
        {
            throw new Exception("Invalid email or password");
        }

        return _jwtService.GenerateJwt(user);
    }
}

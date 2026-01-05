using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Contracts.Responses;
using AuthApi.Data.Repositories;
using AuthApi.Exceptions;

namespace AuthApi.BusinessLogic.Services.Implementation;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserInfo> GetCurrentlyLoggedInUser(string userId)
    {
        if(!Guid.TryParse(userId, out Guid userIdGuid))
        {
            throw new ArgumentException("Incorrect argument");
        }
        var user = await _userRepository.GetByIdAsync(userIdGuid) ?? throw new NotFoundException("User not found");
        return user;
    }

    public async Task<List<UserInfo>> GetAllUserInfo()
    {
        return await _userRepository.GetAllUsers();
    }

    public async Task<bool> UserExists(string email)
    {
        return await _userRepository.CheckExistenceByEmail(email);
    }

    public async Task<UserInfo> GetUserById(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }
}

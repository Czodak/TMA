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

    public async Task<UserInfo> GetCurrentlyLoggedInUser(string userId, CancellationToken cancellationToken)
    {
        if(!Guid.TryParse(userId, out Guid userIdGuid))
        {
            throw new ArgumentException("Incorrect argument");
        }
        var user = await _userRepository.GetByIdAsync(userIdGuid, cancellationToken) ?? throw new NotFoundException("User not found");
        return user;
    }

    public async Task<List<UserInfo>> GetAllUserInfo(CancellationToken cancellationToken)
    {
        return await _userRepository.GetAllUsers(cancellationToken);
    }

    public async Task<bool> UserExists(string email, CancellationToken cancellationToken)
    {
        return await _userRepository.CheckExistenceByEmail(email, cancellationToken);
    }

    public async Task<UserInfo> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }
}

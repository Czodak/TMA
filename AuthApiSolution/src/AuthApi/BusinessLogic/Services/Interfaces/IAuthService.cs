using AuthApi.Contracts.Responses;

namespace AuthApi.BusinessLogic.Services.Interfaces;

public interface IAuthService
{
    Task<UserInfo> GetCurrentlyLoggedInUser(string userId, CancellationToken cancellationToken);
    Task<List<UserInfo>> GetAllUserInfo(CancellationToken cancellationToken);

    Task<bool> UserExists(string email, CancellationToken cancellationToken);
    Task<UserInfo> GetUserById(Guid id, CancellationToken cancellationToken);
}

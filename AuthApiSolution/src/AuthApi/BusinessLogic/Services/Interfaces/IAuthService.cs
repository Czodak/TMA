using AuthApi.Contracts.Responses;

namespace AuthApi.BusinessLogic.Services.Interfaces;

public interface IAuthService
{
    Task<UserInfo> GetCurrentlyLoggedInUser(string userId);
    Task<List<UserInfo>> GetAllUserInfo();

    Task<bool> UserExists(string email);
    Task<UserInfo> GetUserById(Guid id);
}

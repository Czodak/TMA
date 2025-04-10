using AuthApi.Contracts.Requests;
using AuthApi.Contracts.Responses;

namespace AuthApi.BusinessLogic.Services
{
    public interface IAuthService
    {

        Task<string> RegisterAsync(RegisterUserRequest registerUserDto);
        Task<string> LoginAsync(LoginUserRequest loginUserDto);
        Task<UserInfo> GetCurrentlyLoggedInUser(string userId);
    }
}

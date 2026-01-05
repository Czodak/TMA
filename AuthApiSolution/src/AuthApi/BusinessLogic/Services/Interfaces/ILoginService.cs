using AuthApi.Contracts.Requests;

namespace AuthApi.BusinessLogic.Services.Interfaces;

public interface ILoginService
{
    Task<string> LoginAsync(LoginUserRequest loginUserDto);
}

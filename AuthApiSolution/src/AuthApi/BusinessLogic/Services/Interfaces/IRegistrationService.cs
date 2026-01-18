using AuthApi.Contracts.Requests;

namespace AuthApi.BusinessLogic.Services.Interfaces;

public interface IRegistrationService
{
    Task<string> RegisterAsync(RegisterUserRequest registerUserDto, CancellationToken cancellationToken);
}

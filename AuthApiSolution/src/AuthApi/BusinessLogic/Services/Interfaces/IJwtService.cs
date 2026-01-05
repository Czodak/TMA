using AuthApi.Data.Entities;

namespace AuthApi.BusinessLogic.Services.Interfaces;

public interface IJwtService
{
    string GenerateJwt(UserEntity user);
}

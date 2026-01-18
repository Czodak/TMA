using AuthApi.BusinessLogic.Services.Interfaces;

namespace AuthApi.BusinessLogic.Services.Implementation;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plain) => BCrypt.Net.BCrypt.HashPassword(plain);

    public bool Verify(string plain, string hash) => BCrypt.Net.BCrypt.Verify(plain, hash);
}
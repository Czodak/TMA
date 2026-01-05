using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Data.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.BusinessLogic.Services.Implementation;

public class JwtService : IJwtService
{
    private readonly string _jwtSecret;

    public JwtService(IConfiguration configuration)
    {
        _jwtSecret = configuration["JwtSettings:Secret"] ?? string.Empty;
    }

    public string GenerateJwt(UserEntity user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "AuthApi",
            audience: "TaskApi",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

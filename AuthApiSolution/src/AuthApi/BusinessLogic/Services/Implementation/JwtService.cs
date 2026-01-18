using AuthApi.BusinessLogic.Services.Interfaces;
using AuthApi.Data.Entities;
using AuthApi.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.BusinessLogic.Services.Implementation;

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly byte[] _secretBytes;

    public JwtService(IOptions<JwtSettings> options)
    {
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _secretBytes = Encoding.UTF8.GetBytes(_settings.Secret);
    }

    public string GenerateJwt(UserEntity user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (user.Id == Guid.Empty) throw new ArgumentException("User must have an Id to generate token", nameof(user));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
        };

        var key = new SymmetricSecurityKey(_secretBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(_settings.ExpiresInHours);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

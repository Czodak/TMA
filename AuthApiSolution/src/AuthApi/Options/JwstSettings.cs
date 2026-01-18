namespace AuthApi.Options;

public sealed record JwtSettings
{
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = "AuthApi";
    public string Audience { get; init; } = "TaskApi";
    public int ExpiresInHours { get; init; } = 1;
}

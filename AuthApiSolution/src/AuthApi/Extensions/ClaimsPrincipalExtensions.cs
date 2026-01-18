using System.Security.Claims;

namespace AuthApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
        => user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}

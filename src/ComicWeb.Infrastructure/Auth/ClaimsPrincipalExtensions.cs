using System.Security.Claims;
using ComicWeb.Domain.Enums;

namespace ComicWeb.Infrastructure.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
        return value != null ? Guid.Parse(value) : Guid.Empty;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(AppRoles.Admin);
    }
}

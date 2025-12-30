using System.Security.Claims;
using ComicWeb.Domain.Enums;

namespace ComicWeb.Infrastructure.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the current user id from the claims principal.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
        return value != null ? Guid.Parse(value) : Guid.Empty;
    }

    /// <summary>
    /// Determines whether the current user has the admin role.
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(AppRoles.Admin);
    }
}

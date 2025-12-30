using ComicWeb.Domain.Enums;

namespace ComicWeb.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = AppRoles.User;
    public int Status { get; set; } = 1;
    public bool EmailVerified { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<AuthRefreshToken> RefreshTokens { get; set; } = new List<AuthRefreshToken>();
    public ICollection<Comic> Comics { get; set; } = new List<Comic>();
}

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

public sealed class AuthRefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public bool Revoked { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User? User { get; set; }
}

public sealed class PasswordResetCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User? User { get; set; }
}

public sealed class EmailVerificationCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User? User { get; set; }
}

public sealed class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string[]? Tags { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Comic> Comics { get; set; } = new List<Comic>();
}

public sealed class Comic
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Author { get; set; }
    public int UnitPrice { get; set; }
    public int? SalaryType { get; set; }
    public int Status { get; set; }
    public string? AdminNote { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User? Owner { get; set; }
    public Category? Category { get; set; }
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}

public sealed class Chapter
{
    public Guid Id { get; set; }
    public Guid ComicId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int UnitPrice { get; set; }
    public int PageCount { get; set; }
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Comic? Comic { get; set; }
    public ICollection<ChapterPage> Pages { get; set; } = new List<ChapterPage>();
}

public sealed class ChapterPage
{
    public Guid Id { get; set; }
    public Guid ChapterId { get; set; }
    public int PageOrder { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public Chapter? Chapter { get; set; }
}

public sealed class Audio
{
    public Guid Id { get; set; }
    public Guid? ComicId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
    public int UnitPrice { get; set; }
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class Notification
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class Comment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ComicId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class Favorite
{
    public Guid UserId { get; set; }
    public Guid ComicId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class Follow
{
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class Mission
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Reward { get; set; }
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class UserMission
{
    public Guid UserId { get; set; }
    public Guid MissionId { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class CurrencyLedger
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? ComicId { get; set; }
    public Guid? ChapterId { get; set; }
    public Guid? AudioId { get; set; }
    public int Amount { get; set; }
    public int CurrencyType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public string? ProviderRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class UserPurchase
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid RefId { get; set; }
    public DateTimeOffset PurchasedAt { get; set; }
}

public sealed class WithdrawRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Amount { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankAccount { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public string? AdminNote { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class Outstanding
{
    public Guid Id { get; set; }
    public Guid ComicId { get; set; }
    public int Amount { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class Upload
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

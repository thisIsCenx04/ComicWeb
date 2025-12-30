namespace ComicWeb.Application.DTOs;

public sealed class UploadResponse
{
    public required string Link { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
}

public sealed class PurchaseChapterRequest
{
    public Guid ChapterId { get; set; }
}

public sealed class CurrencyEntryRequest
{
    public Guid UserId { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string? Description { get; set; }
}

public sealed class WithdrawCreateRequest
{
    public int Amount { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankAccount { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
}

public sealed class WithdrawStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
}

public sealed class CommentCreateRequest
{
    public Guid ComicId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public sealed class NotificationCreateRequest
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Status { get; set; }
}

public sealed class MissionCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Reward { get; set; }
    public int Status { get; set; }
}

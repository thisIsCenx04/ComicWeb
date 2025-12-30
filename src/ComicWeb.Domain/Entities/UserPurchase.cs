namespace ComicWeb.Domain.Entities;

public sealed class UserPurchase
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid RefId { get; set; }
    public DateTimeOffset PurchasedAt { get; set; }
}

namespace ComicWeb.Domain.Entities;

public sealed class CurrencyLedger
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

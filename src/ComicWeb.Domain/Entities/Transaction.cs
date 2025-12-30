namespace ComicWeb.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? ComicId { get; set; }
    public Guid? ChapterId { get; set; }
    public int Amount { get; set; }
    public int CurrencyType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public string? ProviderRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

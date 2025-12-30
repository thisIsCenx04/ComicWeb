namespace ComicWeb.Domain.Entities;

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

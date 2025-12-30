namespace ComicWeb.Domain.Entities;

public sealed class Outstanding
{
    public Guid Id { get; set; }
    public Guid ComicId { get; set; }
    public int Amount { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

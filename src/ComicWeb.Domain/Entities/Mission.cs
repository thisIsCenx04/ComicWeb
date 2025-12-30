namespace ComicWeb.Domain.Entities;

public sealed class Mission
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Reward { get; set; }
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

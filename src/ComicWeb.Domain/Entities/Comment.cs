namespace ComicWeb.Domain.Entities;

public sealed class Comment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ComicId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

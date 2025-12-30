namespace ComicWeb.Domain.Entities;

public sealed class Favorite
{
    public Guid UserId { get; set; }
    public Guid ComicId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

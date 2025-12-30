namespace ComicWeb.Domain.Entities;

public sealed class Follow
{
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

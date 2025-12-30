namespace ComicWeb.Domain.Entities;

public sealed class UserMission
{
    public Guid UserId { get; set; }
    public Guid MissionId { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

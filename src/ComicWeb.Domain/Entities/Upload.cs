namespace ComicWeb.Domain.Entities;

public sealed class Upload
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

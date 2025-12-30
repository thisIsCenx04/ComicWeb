namespace ComicWeb.Domain.Entities;

public sealed class ChapterPage
{
    public Guid Id { get; set; }
    public Guid ChapterId { get; set; }
    public int PageOrder { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public Chapter? Chapter { get; set; }
}

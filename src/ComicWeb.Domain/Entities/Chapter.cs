namespace ComicWeb.Domain.Entities;

public sealed class Chapter
{
    public Guid Id { get; set; }
    public Guid ComicId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int UnitPrice { get; set; }
    public int PageCount { get; set; }
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Comic? Comic { get; set; }
    public ICollection<ChapterPage> Pages { get; set; } = new List<ChapterPage>();
}

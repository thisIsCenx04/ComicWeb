namespace ComicWeb.Domain.Entities;

public sealed class Comic
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Author { get; set; }
    public int UnitPrice { get; set; }
    public int? SalaryType { get; set; }
    public int Status { get; set; }
    public string? AdminNote { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User? Owner { get; set; }
    public Category? Category { get; set; }
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}

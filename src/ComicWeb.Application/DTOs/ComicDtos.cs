namespace ComicWeb.Application.DTOs;

public class ComicDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Author { get; set; }
    public int UnitPrice { get; set; }
    public int? SalaryType { get; set; }
    public int Status { get; set; }
    public string? AdminNote { get; set; }
    public Guid? CategoryId { get; set; }
}

public sealed class ComicCreateRequest
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Author { get; set; }
    public int UnitPrice { get; set; }
    public int? SalaryType { get; set; }
    public Guid? CategoryId { get; set; }
}

public sealed class ComicUpdateRequest
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Author { get; set; }
    public int UnitPrice { get; set; }
    public int? SalaryType { get; set; }
    public Guid? CategoryId { get; set; }
    public int Status { get; set; }
    public string? AdminNote { get; set; }
}

public sealed class ComicDetailDto : ComicDto
{
    public IReadOnlyList<ChapterDto> Chapters { get; set; } = Array.Empty<ChapterDto>();
}

public sealed class ComicStatusUpdateRequest
{
    public int Status { get; set; }
    public string? AdminNote { get; set; }
}

public sealed class OutstandingCreateRequest
{
    public Guid ComicId { get; set; }
    public int Amount { get; set; }
}

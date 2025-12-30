namespace ComicWeb.Application.DTOs;

public sealed class ChapterDto
{
    public Guid Id { get; set; }
    public Guid ComicId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int UnitPrice { get; set; }
    public int PageCount { get; set; }
    public int Status { get; set; }
}

public sealed class ChapterCreateRequest
{
    public Guid ComicId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int UnitPrice { get; set; }
    public int Status { get; set; }
}

public sealed class ChapterUpdateRequest
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int UnitPrice { get; set; }
    public int Status { get; set; }
}

public sealed class ChapterPageDto
{
    public Guid Id { get; set; }
    public int PageOrder { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

public sealed class ChapterPagesBulkRequest
{
    public List<ChapterPageCreateRequest> Pages { get; set; } = new();
}

public sealed class ChapterPageCreateRequest
{
    public int Order { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

public sealed class ChapterPageReorderRequest
{
    public List<ChapterPageReorderItem> Pages { get; set; } = new();
}

public sealed class ChapterPageReorderItem
{
    public Guid PageId { get; set; }
    public int Order { get; set; }
}

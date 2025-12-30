namespace ComicWeb.Application.DTOs;

public sealed class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string[]? Tags { get; set; }
}

public sealed class CategoryCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string[]? Tags { get; set; }
}

public sealed class CategoryUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string[]? Tags { get; set; }
}

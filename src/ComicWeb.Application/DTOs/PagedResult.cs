namespace ComicWeb.Application.DTOs;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Total { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
}

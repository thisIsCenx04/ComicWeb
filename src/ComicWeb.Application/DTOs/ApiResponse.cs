namespace ComicWeb.Application.DTOs;

public interface IApiResponse
{
    bool Success { get; }
    int StatusCode { get; }
    string? Message { get; }
}

public sealed class ApiResponse<T> : IApiResponse
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }

    public static ApiResponse<T> From(T? data, int statusCode, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = statusCode is >= 200 and < 400,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }
}

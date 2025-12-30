namespace ComicWeb.Infrastructure.Storage.Cloudinary;

public sealed class CloudinaryUploadResult
{
    public required string Link { get; init; }
    public string? PublicId { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
}

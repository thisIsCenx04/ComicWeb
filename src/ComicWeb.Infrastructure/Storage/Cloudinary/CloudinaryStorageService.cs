using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using CloudinaryAccount = global::CloudinaryDotNet.Account;
using CloudinaryClient = global::CloudinaryDotNet.Cloudinary;
using CloudinaryFileDescription = global::CloudinaryDotNet.FileDescription;

namespace ComicWeb.Infrastructure.Storage.Cloudinary;

public sealed class CloudinaryStorageService
{
    private readonly CloudinaryClient _cloudinary;
    private readonly CloudinarySettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudinaryStorageService"/> class.
    /// </summary>
    public CloudinaryStorageService(IOptions<CloudinarySettings> settings)
    {
        _settings = settings.Value;
        _cloudinary = new CloudinaryClient(new CloudinaryAccount(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret))
        {
            Api = { Secure = true }
        };
    }

    /// <summary>
    /// Uploads an image to Cloudinary and returns its metadata.
    /// </summary>
    public async Task<CloudinaryUploadResult?> UploadImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return null;
        }

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new CloudinaryFileDescription(file.FileName, stream),
            Folder = _settings.Folder
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (result.StatusCode != System.Net.HttpStatusCode.OK && result.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return null;
        }

        return new CloudinaryUploadResult
        {
            Link = result.SecureUrl?.ToString() ?? string.Empty,
            PublicId = result.PublicId,
            Width = result.Width,
            Height = result.Height
        };
    }
}

using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using ComicWeb.Infrastructure.Storage;
using ComicWeb.Infrastructure.Storage.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("uploads")]
public sealed class UploadsController : ControllerBase
{
    private readonly ComicDbContext _dbContext;
    private readonly StorageSettings _settings;
    private readonly CloudinaryStorageService _cloudinaryStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadsController"/> class.
    /// </summary>
    public UploadsController(ComicDbContext dbContext, IOptions<StorageSettings> settings, CloudinaryStorageService cloudinaryStorage)
    {
        _dbContext = dbContext;
        _settings = settings.Value;
        _cloudinaryStorage = cloudinaryStorage;
    }

    /// <summary>
    /// Uploads an image and returns its public link.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadImage(IFormFile file)
    {
        if (file.Length == 0 || file.Length > _settings.MaxFileSizeMb * 1024L * 1024L)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid file"));
        }

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType))
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid file"));
        }

        var result = await _cloudinaryStorage.UploadImageAsync(file, HttpContext.RequestAborted);
        if (result == null || string.IsNullOrWhiteSpace(result.Link))
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid file"));
        }

        await SaveAuditAsync(result.Link, "IMAGE");
        return Ok(ApiResponse<UploadResponse>.From(new UploadResponse
        {
            Link = result.Link,
            Width = result.Width,
            Height = result.Height
        }, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Writes an upload audit record for the current user.
    /// </summary>
    private async Task SaveAuditAsync(string link, string fileType)
    {
        var userId = User.GetUserId();
        _dbContext.Uploads.Add(new Upload
        {
            UserId = userId == Guid.Empty ? null : userId,
            FileType = fileType,
            Url = link
        });
        await _dbContext.SaveChangesAsync();
    }
}

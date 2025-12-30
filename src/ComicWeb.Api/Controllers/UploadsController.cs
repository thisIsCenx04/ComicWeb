using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using ComicWeb.Infrastructure.Storage;
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

    public UploadsController(ComicDbContext dbContext, IOptions<StorageSettings> settings)
    {
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadImage(IFormFile file)
    {
        var link = await SaveFileAsync(file, _settings.ImagePath, new[] { "image/jpeg", "image/png", "image/webp" });
        if (link == null)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid file"));
        }

        await SaveAuditAsync(link, "IMAGE");
        return Ok(ApiResponse<UploadResponse>.From(new UploadResponse { Link = link }, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPost("audio")]
    public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadAudio(IFormFile file)
    {
        var link = await SaveFileAsync(file, _settings.AudioPath, new[] { "audio/mpeg", "audio/mp3", "audio/wav", "audio/ogg" });
        if (link == null)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid file"));
        }

        await SaveAuditAsync(link, "AUDIO");
        return Ok(ApiResponse<UploadResponse>.From(new UploadResponse { Link = link }, StatusCodes.Status200OK));
    }

    private async Task<string?> SaveFileAsync(IFormFile file, string relativePath, string[] allowedContentTypes)
    {
        if (file.Length == 0 || file.Length > _settings.MaxFileSizeMb * 1024L * 1024L)
        {
            return null;
        }

        if (!allowedContentTypes.Contains(file.ContentType))
        {
            return null;
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        Directory.CreateDirectory(rootPath);
        var filePath = Path.Combine(rootPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        var urlPath = relativePath.Replace("wwwroot", string.Empty).Replace("\\", "/");
        return $"{baseUrl}{urlPath}/{fileName}";
    }

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

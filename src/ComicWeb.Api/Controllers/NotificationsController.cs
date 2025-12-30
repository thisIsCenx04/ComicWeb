using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    public NotificationsController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Notification>>>> GetAll()
    {
        var items = await _dbContext.Notifications.OrderByDescending(n => n.CreatedAt).ToListAsync();
        return Ok(ApiResponse<IReadOnlyList<Notification>>.From(items, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Notification>>> Create(NotificationCreateRequest request)
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var item = new Notification
        {
            Slug = request.Slug.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            Status = request.Status
        };

        _dbContext.Notifications.Add(item);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<Notification>.From(item, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Notification>>> Update(Guid id, NotificationCreateRequest request)
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var item = await _dbContext.Notifications.FindAsync(id);
        if (item == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Notification not found"));
        }

        item.Slug = request.Slug.Trim();
        item.Title = request.Title.Trim();
        item.Description = request.Description;
        item.ThumbnailUrl = request.ThumbnailUrl;
        item.Status = request.Status;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<Notification>.From(item, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var item = await _dbContext.Notifications.FindAsync(id);
        if (item == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Notification not found"));
        }

        _dbContext.Notifications.Remove(item);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }
}

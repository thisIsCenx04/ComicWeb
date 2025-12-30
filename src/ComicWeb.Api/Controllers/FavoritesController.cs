using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("favorites")]
public sealed class FavoritesController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    public FavoritesController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpPost("{comicId:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Add(Guid comicId)
    {
        var userId = User.GetUserId();
        var exists = await _dbContext.Favorites.AnyAsync(f => f.UserId == userId && f.ComicId == comicId);
        if (!exists)
        {
            _dbContext.Favorites.Add(new Favorite { UserId = userId, ComicId = comicId });
            await _dbContext.SaveChangesAsync();
        }

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpDelete("{comicId:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Remove(Guid comicId)
    {
        var userId = User.GetUserId();
        var favorite = await _dbContext.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ComicId == comicId);
        if (favorite != null)
        {
            _dbContext.Favorites.Remove(favorite);
            await _dbContext.SaveChangesAsync();
        }

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ComicDto>>>> GetMine()
    {
        var userId = User.GetUserId();
        var items = await _dbContext.Favorites
            .Where(f => f.UserId == userId)
            .Join(_dbContext.Comics, f => f.ComicId, c => c.Id, (_, c) => c)
            .Select(c => new ComicDto
            {
                Id = c.Id,
                Slug = c.Slug,
                Title = c.Title,
                Description = c.Description,
                ThumbnailUrl = c.ThumbnailUrl,
                Author = c.Author,
                UnitPrice = c.UnitPrice,
                SalaryType = c.SalaryType,
                Status = c.Status,
                AdminNote = c.AdminNote,
                CategoryId = c.CategoryId
            })
            .ToListAsync();

        return Ok(ApiResponse<IReadOnlyList<ComicDto>>.From(items, StatusCodes.Status200OK));
    }
}

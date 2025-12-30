using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("comments")]
public sealed class CommentsController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    public CommentsController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Comment>>>> GetByComic([FromQuery] Guid comicId)
    {
        var items = await _dbContext.Comments
            .Where(c => c.ComicId == comicId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<IReadOnlyList<Comment>>.From(items, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Comment>>> Create(CommentCreateRequest request)
    {
        var userId = User.GetUserId();
        var comment = new Comment
        {
            UserId = userId,
            ComicId = request.ComicId,
            ParentId = request.ParentId,
            Content = request.Content.Trim()
        };

        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<Comment>.From(comment, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        var comment = await _dbContext.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Comment not found"));
        }

        if (comment.UserId != User.GetUserId() && !User.IsAdmin())
        {
            return Forbid();
        }

        _dbContext.Comments.Remove(comment);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }
}

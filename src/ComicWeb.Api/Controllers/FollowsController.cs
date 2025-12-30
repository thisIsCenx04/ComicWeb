using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("follows")]
public sealed class FollowsController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FollowsController"/> class.
    /// </summary>
    public FollowsController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Follows a user.
    /// </summary>
    [Authorize]
    [HttpPost("{userId:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Follow(Guid userId)
    {
        var followerId = User.GetUserId();
        if (followerId == userId)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid target"));
        }

        var exists = await _dbContext.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowedId == userId);
        if (!exists)
        {
            _dbContext.Follows.Add(new Follow { FollowerId = followerId, FollowedId = userId });
            await _dbContext.SaveChangesAsync();
        }

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Unfollows a user.
    /// </summary>
    [Authorize]
    [HttpDelete("{userId:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Unfollow(Guid userId)
    {
        var followerId = User.GetUserId();
        var follow = await _dbContext.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == userId);
        if (follow != null)
        {
            _dbContext.Follows.Remove(follow);
            await _dbContext.SaveChangesAsync();
        }

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Gets follow relationships for the current user.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Follow>>>> GetMine()
    {
        var followerId = User.GetUserId();
        var items = await _dbContext.Follows.Where(f => f.FollowerId == followerId).ToListAsync();
        return Ok(ApiResponse<IReadOnlyList<Follow>>.From(items, StatusCodes.Status200OK));
    }
}

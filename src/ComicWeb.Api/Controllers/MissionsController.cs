using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("missions")]
public sealed class MissionsController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    public MissionsController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Mission>>>> GetAll()
    {
        var items = await _dbContext.Missions.OrderBy(m => m.CreatedAt).ToListAsync();
        return Ok(ApiResponse<IReadOnlyList<Mission>>.From(items, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Mission>>> Create(MissionCreateRequest request)
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var mission = new Mission
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Reward = request.Reward,
            Status = request.Status
        };

        _dbContext.Missions.Add(mission);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<Mission>.From(mission, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<object?>>> Complete(Guid id)
    {
        var userId = User.GetUserId();
        var existing = await _dbContext.UserMissions.FindAsync(userId, id);
        if (existing == null)
        {
            _dbContext.UserMissions.Add(new UserMission { UserId = userId, MissionId = id, CompletedAt = DateTimeOffset.UtcNow });
            await _dbContext.SaveChangesAsync();
        }

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }
}

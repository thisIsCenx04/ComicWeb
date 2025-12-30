using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("currency")]
public sealed class CurrencyController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyController"/> class.
    /// </summary>
    public CurrencyController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the current user's currency history.
    /// </summary>
    [Authorize]
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<PagedResult<CurrencyLedger>>>> GetHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        var query = _dbContext.CurrencyLedgers.Where(c => c.UserId == userId);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<CurrencyLedger>
        {
            Items = items,
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PagedResult<CurrencyLedger>>.From(result, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Creates a currency ledger entry.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CurrencyLedger>>> CreateEntry(CurrencyEntryRequest request)
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var entry = new CurrencyLedger
        {
            UserId = request.UserId,
            EntryType = request.EntryType,
            Amount = request.Amount,
            Description = request.Description
        };

        _dbContext.CurrencyLedgers.Add(entry);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<CurrencyLedger>.From(entry, StatusCodes.Status200OK));
    }
}

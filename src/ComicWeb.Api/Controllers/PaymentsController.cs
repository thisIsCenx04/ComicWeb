using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    public PaymentsController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpPost("purchased-chapter")]
    public async Task<ActionResult<ApiResponse<object?>>> PurchaseChapter(PurchaseChapterRequest request)
    {
        var userId = User.GetUserId();
        var chapter = await _dbContext.Chapters.FindAsync(request.ChapterId);
        if (chapter == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Chapter not found"));
        }

        var already = await _dbContext.UserPurchases.AnyAsync(p => p.UserId == userId && p.Type == "CHAPTER" && p.RefId == chapter.Id);
        if (already)
        {
            return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
        }

        var tx = new Transaction
        {
            UserId = userId,
            Type = "CHAPTER",
            ChapterId = chapter.Id,
            Amount = chapter.UnitPrice,
            CurrencyType = 0,
            Status = "SUCCESS",
            Provider = "MANUAL"
        };

        var purchase = new UserPurchase
        {
            UserId = userId,
            Type = "CHAPTER",
            RefId = chapter.Id
        };

        _dbContext.Transactions.Add(tx);
        _dbContext.UserPurchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpGet("transactions")]
    public async Task<ActionResult<ApiResponse<PagedResult<Transaction>>>> GetTransactions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var query = _dbContext.Transactions.AsQueryable();
        if (!User.IsAdmin())
        {
            var userId = User.GetUserId();
            query = query.Where(t => t.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<Transaction>
        {
            Items = items,
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PagedResult<Transaction>>.From(result, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpGet("transactions/check/{id:guid}")]
    public async Task<ActionResult<ApiResponse<Transaction>>> CheckTransaction(Guid id)
    {
        var tx = await _dbContext.Transactions.FindAsync(id);
        if (tx == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Transaction not found"));
        }

        if (!User.IsAdmin() && tx.UserId != User.GetUserId())
        {
            return Forbid();
        }

        return Ok(ApiResponse<Transaction>.From(tx, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPut("accept-manual/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> AcceptManual(Guid id)
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var tx = await _dbContext.Transactions.FindAsync(id);
        if (tx == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Transaction not found"));
        }

        tx.Status = "SUCCESS";
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }
}

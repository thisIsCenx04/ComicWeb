using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("withdraws")]
public sealed class WithdrawsController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    public WithdrawsController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<WithdrawRequest>>> Create(WithdrawCreateRequest request)
    {
        var userId = User.GetUserId();
        var balance = await GetBalanceAsync(userId);
        if (request.Amount > balance)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Insufficient balance"));
        }

        var withdraw = new WithdrawRequest
        {
            UserId = userId,
            Amount = request.Amount,
            BankName = request.BankName.Trim(),
            BankAccount = request.BankAccount.Trim(),
            BankAccountName = request.BankAccountName.Trim(),
            Status = "PENDING"
        };

        _dbContext.WithdrawRequests.Add(withdraw);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<WithdrawRequest>.From(withdraw, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WithdrawRequest>>>> GetMine()
    {
        var userId = User.GetUserId();
        var items = await _dbContext.WithdrawRequests
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<IReadOnlyList<WithdrawRequest>>.From(items, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpGet("admin")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WithdrawRequest>>>> GetAll()
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var items = await _dbContext.WithdrawRequests
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<IReadOnlyList<WithdrawRequest>>.From(items, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WithdrawRequest>>> UpdateStatus(Guid id, WithdrawStatusRequest request)
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var withdraw = await _dbContext.WithdrawRequests.FindAsync(id);
        if (withdraw == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Withdraw not found"));
        }

        withdraw.Status = request.Status;
        withdraw.AdminNote = request.AdminNote;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<WithdrawRequest>.From(withdraw, StatusCodes.Status200OK));
    }

    private async Task<int> GetBalanceAsync(Guid userId)
    {
        var credits = await _dbContext.CurrencyLedgers
            .Where(c => c.UserId == userId && c.EntryType == "CREDIT")
            .SumAsync(c => (int?)c.Amount) ?? 0;

        var debits = await _dbContext.CurrencyLedgers
            .Where(c => c.UserId == userId && c.EntryType == "DEBIT")
            .SumAsync(c => (int?)c.Amount) ?? 0;

        return credits - debits;
    }
}

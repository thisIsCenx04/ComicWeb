using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("comics")]
public sealed class ComicsController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComicsController"/> class.
    /// </summary>
    public ComicsController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets a paged list of comics with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ComicDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] Guid? categoryId = null, [FromQuery] int? status = null)
    {
        var query = _dbContext.Comics.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Title.ToLower().Contains(search.ToLower()));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == categoryId);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        var result = new PagedResult<ComicDto>
        {
            Items = items,
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PagedResult<ComicDto>>.From(result, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Gets a list of hot comics.
    /// </summary>
    [HttpGet("hot")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ComicDto>>>> GetHot([FromQuery] int limit = 10)
    {
        var comics = await _dbContext.Comics
            .OrderByDescending(c => _dbContext.Favorites.Count(f => f.ComicId == c.Id))
            .Take(limit)
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

        return Ok(ApiResponse<IReadOnlyList<ComicDto>>.From(comics, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Gets a list of outstanding comics.
    /// </summary>
    [HttpGet("outstandings")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ComicDto>>>> GetOutstandings([FromQuery] int limit = 10)
    {
        var comics = await _dbContext.Outstandings
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .Join(_dbContext.Comics, o => o.ComicId, c => c.Id, (_, c) => c)
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

        return Ok(ApiResponse<IReadOnlyList<ComicDto>>.From(comics, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Gets a list of recently completed comics.
    /// </summary>
    [HttpGet("last-completed")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ComicDto>>>> GetLastCompleted([FromQuery] int limit = 10)
    {
        var comics = await _dbContext.Comics
            .OrderByDescending(c => c.UpdatedAt)
            .Take(limit)
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

        return Ok(ApiResponse<IReadOnlyList<ComicDto>>.From(comics, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Gets a comic by slug with chapter details.
    /// </summary>
    [Authorize]
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ApiResponse<ComicDetailDto>>> GetBySlug(string slug)
    {
        var comic = await _dbContext.Comics
            .Include(c => c.Chapters)
            .FirstOrDefaultAsync(c => c.Slug == slug);
        if (comic == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Comic not found"));
        }

        var dto = new ComicDetailDto
        {
            Id = comic.Id,
            Slug = comic.Slug,
            Title = comic.Title,
            Description = comic.Description,
            ThumbnailUrl = comic.ThumbnailUrl,
            Author = comic.Author,
            UnitPrice = comic.UnitPrice,
            SalaryType = comic.SalaryType,
            Status = comic.Status,
            AdminNote = comic.AdminNote,
            CategoryId = comic.CategoryId,
            Chapters = comic.Chapters.OrderBy(c => c.CreatedAt).Select(ch => new ChapterDto
            {
                Id = ch.Id,
                ComicId = ch.ComicId,
                Slug = ch.Slug,
                Title = ch.Title,
                ThumbnailUrl = ch.ThumbnailUrl,
                UnitPrice = ch.UnitPrice,
                PageCount = ch.PageCount,
                Status = ch.Status
            }).ToList()
        };

        return Ok(ApiResponse<ComicDetailDto>.From(dto, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Creates a new comic.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ComicDto>>> Create(ComicCreateRequest request)
    {
        var slugExists = await _dbContext.Comics.AnyAsync(c => c.Slug == request.Slug);
        if (slugExists)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Slug already exists"));
        }

        var userId = User.GetUserId();
        var comic = new Comic
        {
            OwnerId = userId,
            CategoryId = request.CategoryId,
            Slug = request.Slug.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            Author = request.Author,
            UnitPrice = request.UnitPrice,
            SalaryType = request.SalaryType,
            Status = 0
        };

        _dbContext.Comics.Add(comic);
        await _dbContext.SaveChangesAsync();

        var dto = new ComicDto
        {
            Id = comic.Id,
            Slug = comic.Slug,
            Title = comic.Title,
            Description = comic.Description,
            ThumbnailUrl = comic.ThumbnailUrl,
            Author = comic.Author,
            UnitPrice = comic.UnitPrice,
            SalaryType = comic.SalaryType,
            Status = comic.Status,
            AdminNote = comic.AdminNote,
            CategoryId = comic.CategoryId
        };

        return Ok(ApiResponse<ComicDto>.From(dto, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Updates an existing comic.
    /// </summary>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ComicDto>>> Update(Guid id, ComicUpdateRequest request)
    {
        var comic = await _dbContext.Comics.FindAsync(id);
        if (comic == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Comic not found"));
        }

        var userId = User.GetUserId();
        if (comic.OwnerId != userId && !User.IsAdmin())
        {
            return Forbid();
        }

        var slugExists = await _dbContext.Comics.AnyAsync(c => c.Slug == request.Slug && c.Id != id);
        if (slugExists)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Slug already exists"));
        }

        comic.Slug = request.Slug.Trim();
        comic.Title = request.Title.Trim();
        comic.Description = request.Description;
        comic.ThumbnailUrl = request.ThumbnailUrl;
        comic.Author = request.Author;
        comic.UnitPrice = request.UnitPrice;
        comic.SalaryType = request.SalaryType;
        comic.CategoryId = request.CategoryId;
        comic.Status = request.Status;
        comic.AdminNote = request.AdminNote;

        await _dbContext.SaveChangesAsync();

        var dto = new ComicDto
        {
            Id = comic.Id,
            Slug = comic.Slug,
            Title = comic.Title,
            Description = comic.Description,
            ThumbnailUrl = comic.ThumbnailUrl,
            Author = comic.Author,
            UnitPrice = comic.UnitPrice,
            SalaryType = comic.SalaryType,
            Status = comic.Status,
            AdminNote = comic.AdminNote,
            CategoryId = comic.CategoryId
        };

        return Ok(ApiResponse<ComicDto>.From(dto, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Updates a comic's status and admin note.
    /// </summary>
    [Authorize]
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<object?>>> UpdateStatus(Guid id, ComicStatusUpdateRequest request)
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var comic = await _dbContext.Comics.FindAsync(id);
        if (comic == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Comic not found"));
        }

        comic.Status = request.Status;
        comic.AdminNote = request.AdminNote;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Gets comics created by the current user.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ComicDto>>>> GetMine()
    {
        var userId = User.GetUserId();
        var items = await _dbContext.Comics
            .Where(c => c.OwnerId == userId)
            .OrderByDescending(c => c.CreatedAt)
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

    /// <summary>
    /// Gets comics purchased by the current user.
    /// </summary>
    [Authorize]
    [HttpGet("purchased")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ComicDto>>>> GetPurchased()
    {
        var userId = User.GetUserId();
        var items = await _dbContext.UserPurchases
            .Where(p => p.UserId == userId && p.Type == "COMIC")
            .Join(_dbContext.Comics, p => p.RefId, c => c.Id, (_, c) => c)
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

    /// <summary>
    /// Creates an outstanding entry for a comic.
    /// </summary>
    [Authorize]
    [HttpPost("outstandings")]
    public async Task<ActionResult<ApiResponse<object?>>> CreateOutstanding(OutstandingCreateRequest request)
    {
        if (!User.IsAdmin())
        {
            return Forbid();
        }

        var exists = await _dbContext.Comics.AnyAsync(c => c.Id == request.ComicId);
        if (!exists)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Comic not found"));
        }

        var outstanding = new Outstanding
        {
            ComicId = request.ComicId,
            Amount = request.Amount,
            CreatedBy = User.GetUserId()
        };

        _dbContext.Outstandings.Add(outstanding);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }
}

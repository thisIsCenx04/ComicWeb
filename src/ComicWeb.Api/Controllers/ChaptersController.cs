using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("chapters")]
public sealed class ChaptersController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChaptersController"/> class.
    /// </summary>
    public ChaptersController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets a chapter by id, optionally including pages.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ChapterDto>>> GetById(Guid id, [FromQuery] bool includePages = false)
    {
        var query = _dbContext.Chapters.AsQueryable();
        if (includePages)
        {
            query = query.Include(c => c.Pages);
        }

        var chapter = await query.FirstOrDefaultAsync(c => c.Id == id);
        if (chapter == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Chapter not found"));
        }

        var dto = new ChapterDto
        {
            Id = chapter.Id,
            ComicId = chapter.ComicId,
            Slug = chapter.Slug,
            Title = chapter.Title,
            ThumbnailUrl = chapter.ThumbnailUrl,
            UnitPrice = chapter.UnitPrice,
            PageCount = chapter.PageCount,
            Status = chapter.Status
        };

        return Ok(ApiResponse<ChapterDto>.From(dto, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Gets a paged list of chapters with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ChapterDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] Guid? comicId = null, [FromQuery] int? status = null)
    {
        var query = _dbContext.Chapters.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Title.ToLower().Contains(search.ToLower()));
        }

        if (comicId.HasValue)
        {
            query = query.Where(c => c.ComicId == comicId);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ChapterDto
            {
                Id = c.Id,
                ComicId = c.ComicId,
                Slug = c.Slug,
                Title = c.Title,
                ThumbnailUrl = c.ThumbnailUrl,
                UnitPrice = c.UnitPrice,
                PageCount = c.PageCount,
                Status = c.Status
            })
            .ToListAsync();

        var result = new PagedResult<ChapterDto>
        {
            Items = items,
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PagedResult<ChapterDto>>.From(result, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Creates a new chapter.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ChapterDto>>> Create(ChapterCreateRequest request)
    {
        var comicExists = await _dbContext.Comics.AnyAsync(c => c.Id == request.ComicId);
        if (!comicExists)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Comic not found"));
        }

        var slugExists = await _dbContext.Chapters.AnyAsync(c => c.Slug == request.Slug);
        if (slugExists)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Slug already exists"));
        }

        var chapter = new Chapter
        {
            ComicId = request.ComicId,
            Slug = request.Slug.Trim(),
            Title = request.Title.Trim(),
            ThumbnailUrl = request.ThumbnailUrl,
            UnitPrice = request.UnitPrice,
            Status = request.Status
        };

        _dbContext.Chapters.Add(chapter);
        await _dbContext.SaveChangesAsync();

        var dto = new ChapterDto
        {
            Id = chapter.Id,
            ComicId = chapter.ComicId,
            Slug = chapter.Slug,
            Title = chapter.Title,
            ThumbnailUrl = chapter.ThumbnailUrl,
            UnitPrice = chapter.UnitPrice,
            PageCount = chapter.PageCount,
            Status = chapter.Status
        };

        return Ok(ApiResponse<ChapterDto>.From(dto, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Updates an existing chapter.
    /// </summary>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ChapterDto>>> Update(Guid id, ChapterUpdateRequest request)
    {
        var chapter = await _dbContext.Chapters.FindAsync(id);
        if (chapter == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Chapter not found"));
        }

        var slugExists = await _dbContext.Chapters.AnyAsync(c => c.Slug == request.Slug && c.Id != id);
        if (slugExists)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Slug already exists"));
        }

        chapter.Slug = request.Slug.Trim();
        chapter.Title = request.Title.Trim();
        chapter.ThumbnailUrl = request.ThumbnailUrl;
        chapter.UnitPrice = request.UnitPrice;
        chapter.Status = request.Status;
        await _dbContext.SaveChangesAsync();

        var dto = new ChapterDto
        {
            Id = chapter.Id,
            ComicId = chapter.ComicId,
            Slug = chapter.Slug,
            Title = chapter.Title,
            ThumbnailUrl = chapter.ThumbnailUrl,
            UnitPrice = chapter.UnitPrice,
            PageCount = chapter.PageCount,
            Status = chapter.Status
        };

        return Ok(ApiResponse<ChapterDto>.From(dto, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Deletes a chapter by id.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        var chapter = await _dbContext.Chapters.FindAsync(id);
        if (chapter == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Chapter not found"));
        }

        _dbContext.Chapters.Remove(chapter);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Adds a batch of pages to a chapter.
    /// </summary>
    [Authorize]
    [HttpPost("{id:guid}/pages")]
    public async Task<ActionResult<ApiResponse<object?>>> AddPages(Guid id, ChapterPagesBulkRequest request)
    {
        var chapter = await _dbContext.Chapters.FindAsync(id);
        if (chapter == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Chapter not found"));
        }

        if (request.Pages.Count == 0)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "No pages"));
        }

        var orderSet = new HashSet<int>(request.Pages.Select(p => p.Order));
        if (orderSet.Count != request.Pages.Count)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Duplicate orders"));
        }

        var pages = request.Pages.Select(p => new ChapterPage
        {
            ChapterId = id,
            PageOrder = p.Order,
            ImageUrl = p.ImageUrl.Trim()
        }).ToList();

        _dbContext.ChapterPages.AddRange(pages);
        chapter.PageCount += pages.Count;
        await _dbContext.SaveChangesAsync();

        var response = new
        {
            ChapterId = chapter.Id,
            PageCount = chapter.PageCount,
            Pages = pages.Select(p => new ChapterPageDto
            {
                Id = p.Id,
                PageOrder = p.PageOrder,
                ImageUrl = p.ImageUrl
            }).ToList()
        };

        return Ok(ApiResponse<object?>.From(response, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Gets pages for a chapter, enforcing purchase rules.
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}/pages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChapterPageDto>>>> GetPages(Guid id)
    {
        var chapter = await _dbContext.Chapters.Include(c => c.Comic).FirstOrDefaultAsync(c => c.Id == id);
        if (chapter == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Chapter not found"));
        }

        if (chapter.UnitPrice > 0)
        {
            var userId = User.GetUserId();
            var isOwner = chapter.Comic?.OwnerId == userId;
            var purchased = await _dbContext.UserPurchases.AnyAsync(p => p.UserId == userId && p.Type == "CHAPTER" && p.RefId == chapter.Id);
            if (!isOwner && !purchased && !User.IsAdmin())
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object?>.From(null, StatusCodes.Status403Forbidden, "Purchase required"));
            }
        }

        var pages = await _dbContext.ChapterPages
            .Where(p => p.ChapterId == id)
            .OrderBy(p => p.PageOrder)
            .Select(p => new ChapterPageDto
            {
                Id = p.Id,
                PageOrder = p.PageOrder,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync();

        return Ok(ApiResponse<IReadOnlyList<ChapterPageDto>>.From(pages, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Gets pages for a chapter by slug, enforcing purchase rules.
    /// </summary>
    [Authorize]
    [HttpGet("slug/{slug}/pages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChapterPageDto>>>> GetPagesBySlug(string slug)
    {
        var chapter = await _dbContext.Chapters.Include(c => c.Comic).FirstOrDefaultAsync(c => c.Slug == slug);
        if (chapter == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Chapter not found"));
        }

        if (chapter.UnitPrice > 0)
        {
            var userId = User.GetUserId();
            var isOwner = chapter.Comic?.OwnerId == userId;
            var purchased = await _dbContext.UserPurchases.AnyAsync(p => p.UserId == userId && p.Type == "CHAPTER" && p.RefId == chapter.Id);
            if (!isOwner && !purchased && !User.IsAdmin())
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object?>.From(null, StatusCodes.Status403Forbidden, "Purchase required"));
            }
        }

        var pages = await _dbContext.ChapterPages
            .Where(p => p.ChapterId == chapter.Id)
            .OrderBy(p => p.PageOrder)
            .Select(p => new ChapterPageDto
            {
                Id = p.Id,
                PageOrder = p.PageOrder,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync();

        return Ok(ApiResponse<IReadOnlyList<ChapterPageDto>>.From(pages, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Reorders pages within a chapter.
    /// </summary>
    [Authorize]
    [HttpPut("{id:guid}/pages/reorder")]
    public async Task<ActionResult<ApiResponse<object?>>> ReorderPages(Guid id, ChapterPageReorderRequest request)
    {
        var chapter = await _dbContext.Chapters.Include(c => c.Pages).FirstOrDefaultAsync(c => c.Id == id);
        if (chapter == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Chapter not found"));
        }

        if (request.Pages.Count == 0)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "No pages"));
        }

        var pageMap = request.Pages.ToDictionary(p => p.PageId, p => p.Order);
        foreach (var page in chapter.Pages)
        {
            if (pageMap.TryGetValue(page.Id, out var order))
            {
                page.PageOrder = -100000 - order;
            }
        }

        await _dbContext.SaveChangesAsync();

        foreach (var page in chapter.Pages)
        {
            if (pageMap.TryGetValue(page.Id, out var order))
            {
                page.PageOrder = order;
            }
        }

        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Deletes a page from a chapter.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:guid}/pages/{pageId:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> DeletePage(Guid id, Guid pageId)
    {
        var page = await _dbContext.ChapterPages.FirstOrDefaultAsync(p => p.Id == pageId && p.ChapterId == id);
        if (page == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Page not found"));
        }

        _dbContext.ChapterPages.Remove(page);

        var chapter = await _dbContext.Chapters.FindAsync(id);
        if (chapter != null && chapter.PageCount > 0)
        {
            chapter.PageCount -= 1;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }
}

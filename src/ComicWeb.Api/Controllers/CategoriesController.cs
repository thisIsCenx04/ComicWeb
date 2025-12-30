using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ComicDbContext _dbContext;

    public CategoriesController(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CategoryDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var query = _dbContext.Categories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Tags = c.Tags
            })
            .ToListAsync();

        var result = new PagedResult<CategoryDto>
        {
            Items = items,
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PagedResult<CategoryDto>>.From(result, StatusCodes.Status200OK));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Category not found"));
        }

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Tags = category.Tags
        };

        return Ok(ApiResponse<CategoryDto>.From(dto, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(CategoryCreateRequest request)
    {
        var slugExists = await _dbContext.Categories.AnyAsync(c => c.Slug == request.Slug);
        if (slugExists)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Slug already exists"));
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim(),
            Tags = request.Tags
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Tags = category.Tags
        };

        return Ok(ApiResponse<CategoryDto>.From(dto, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(Guid id, CategoryUpdateRequest request)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Category not found"));
        }

        var slugExists = await _dbContext.Categories.AnyAsync(c => c.Slug == request.Slug && c.Id != id);
        if (slugExists)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Slug already exists"));
        }

        category.Name = request.Name.Trim();
        category.Slug = request.Slug.Trim();
        category.Tags = request.Tags;
        await _dbContext.SaveChangesAsync();

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Tags = category.Tags
        };

        return Ok(ApiResponse<CategoryDto>.From(dto, StatusCodes.Status200OK));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "Category not found"));
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }
}

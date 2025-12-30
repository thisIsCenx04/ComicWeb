using BCrypt.Net;
using ComicWeb.Domain.Entities;
using ComicWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Infrastructure.Data;

public sealed class DbSeeder
{
    private readonly ComicDbContext _dbContext;

    public DbSeeder(ComicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync()
    {
        if (await _dbContext.Users.AnyAsync())
        {
            return;
        }

        var admin = new User
        {
            FullName = "Admin",
            Email = "admin@comicweb.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = AppRoles.Admin,
            Status = 1,
            EmailVerified = true
        };

        var user = new User
        {
            FullName = "User",
            Email = "user@comicweb.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            Role = AppRoles.User,
            Status = 1,
            EmailVerified = true
        };

        var category = new Category
        {
            Name = "Action",
            Slug = "action",
            Tags = new[] { "hero", "battle" }
        };

        var comic = new Comic
        {
            Title = "Sample Comic",
            Slug = "sample-comic",
            Description = "Sample description",
            Owner = user,
            Category = category,
            Status = 1
        };

        var chapter = new Chapter
        {
            Title = "Chapter 1",
            Slug = "chapter-1",
            Comic = comic,
            UnitPrice = 0,
            Status = 1
        };

        _dbContext.Users.AddRange(admin, user);
        _dbContext.Categories.Add(category);
        _dbContext.Comics.Add(comic);
        _dbContext.Chapters.Add(chapter);

        await _dbContext.SaveChangesAsync();

        var pages = new List<ChapterPage>
        {
            new() { ChapterId = chapter.Id, PageOrder = 1, ImageUrl = "https://placehold.co/800x1200" },
            new() { ChapterId = chapter.Id, PageOrder = 2, ImageUrl = "https://placehold.co/800x1201" }
        };

        _dbContext.ChapterPages.AddRange(pages);
        chapter.PageCount = pages.Count;

        await _dbContext.SaveChangesAsync();
    }
}

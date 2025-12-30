using ComicWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComicWeb.Infrastructure.Data;

public sealed class ComicDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComicDbContext"/> class.
    /// </summary>
    public ComicDbContext(DbContextOptions<ComicDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuthRefreshToken> AuthRefreshTokens => Set<AuthRefreshToken>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();
    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Comic> Comics => Set<Comic>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<ChapterPage> ChapterPages => Set<ChapterPage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<UserMission> UserMissions => Set<UserMission>();
    public DbSet<CurrencyLedger> CurrencyLedgers => Set<CurrencyLedger>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<UserPurchase> UserPurchases => Set<UserPurchase>();
    public DbSet<WithdrawRequest> WithdrawRequests => Set<WithdrawRequest>();
    public DbSet<Outstanding> Outstandings => Set<Outstanding>();
    public DbSet<Upload> Uploads => Set<Upload>();

    /// <summary>
    /// Configures entity mappings and database metadata.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.HasPostgresExtension("citext");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Email).HasColumnName("email").HasColumnType("citext");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<AuthRefreshToken>(entity =>
        {
            entity.ToTable("auth_refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TokenHash).HasColumnName("token_hash");
            entity.Property(e => e.Revoked).HasColumnName("revoked");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_refresh_user");
            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("idx_refresh_expires");
        });

        modelBuilder.Entity<PasswordResetCode>(entity =>
        {
            entity.ToTable("password_reset_codes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CodeHash).HasColumnName("code_hash");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.UsedAt).HasColumnName("used_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_prc_user");
            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("idx_prc_expires");
        });

        modelBuilder.Entity<EmailVerificationCode>(entity =>
        {
            entity.ToTable("email_verification_codes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CodeHash).HasColumnName("code_hash");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_evc_user");
            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("idx_evc_expires");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Slug).HasColumnName("slug");
            entity.Property(e => e.Tags).HasColumnName("tags").HasColumnType("text[]");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Comic>(entity =>
        {
            entity.ToTable("comics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Slug).HasColumnName("slug");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
            entity.Property(e => e.SalaryType).HasColumnName("salary_type");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.AdminNote).HasColumnName("admin_note");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.CategoryId).HasDatabaseName("idx_comics_category");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_comics_status");
        });

        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.ToTable("chapters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.Slug).HasColumnName("slug");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
            entity.Property(e => e.PageCount).HasColumnName("page_count");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            entity.HasIndex(e => e.ComicId).HasDatabaseName("idx_chapters_comic");
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<ChapterPage>(entity =>
        {
            entity.ToTable("chapter_pages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ChapterId).HasColumnName("chapter_id");
            entity.Property(e => e.PageOrder).HasColumnName("page_order");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.HasIndex(e => new { e.ChapterId, e.PageOrder }).IsUnique();
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Slug).HasColumnName("slug");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.ToTable("favorites");
            entity.HasKey(e => new { e.UserId, e.ComicId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.ToTable("follows");
            entity.HasKey(e => new { e.FollowerId, e.FollowedId });
            entity.Property(e => e.FollowerId).HasColumnName("follower_id");
            entity.Property(e => e.FollowedId).HasColumnName("followed_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Mission>(entity =>
        {
            entity.ToTable("missions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Reward).HasColumnName("reward");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<UserMission>(entity =>
        {
            entity.ToTable("user_missions");
            entity.HasKey(e => new { e.UserId, e.MissionId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.MissionId).HasColumnName("mission_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
        });

        modelBuilder.Entity<CurrencyLedger>(entity =>
        {
            entity.ToTable("currency_ledger");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EntryType).HasColumnName("entry_type");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.ChapterId).HasColumnName("chapter_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CurrencyType).HasColumnName("currency_type");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Provider).HasColumnName("provider");
            entity.Property(e => e.ProviderRef).HasColumnName("provider_ref");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<UserPurchase>(entity =>
        {
            entity.ToTable("user_purchases");
            entity.HasKey(e => new { e.UserId, e.Type, e.RefId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.RefId).HasColumnName("ref_id");
            entity.Property(e => e.PurchasedAt).HasColumnName("purchased_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<WithdrawRequest>(entity =>
        {
            entity.ToTable("withdraw_requests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.BankName).HasColumnName("bank_name");
            entity.Property(e => e.BankAccount).HasColumnName("bank_account");
            entity.Property(e => e.BankAccountName).HasColumnName("bank_account_name");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.AdminNote).HasColumnName("admin_note");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Outstanding>(entity =>
        {
            entity.ToTable("outstandings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Upload>(entity =>
        {
            entity.ToTable("uploads");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FileType).HasColumnName("file_type");
            entity.Property(e => e.Url).HasColumnName("url");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        });
    }
}

using System.Security.Cryptography;
using BCrypt.Net;
using ComicWeb.Application.DTOs;
using ComicWeb.Domain.Entities;
using ComicWeb.Infrastructure.Auth;
using ComicWeb.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ComicWeb.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ComicDbContext _dbContext;
    private readonly JwtService _jwtService;
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    public AuthController(ComicDbContext dbContext, JwtService jwtService, IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Registers a new user and issues tokens.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthTokensResponse>>> Register(AuthRegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await _dbContext.Users.AnyAsync(u => u.Email == email);
        if (exists)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Email already exists"));
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EmailVerified = false
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var verifyCode = GenerateCode();
        _dbContext.EmailVerificationCodes.Add(new EmailVerificationCode
        {
            UserId = user.Id,
            CodeHash = JwtService.HashToken(verifyCode),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        });
        await _dbContext.SaveChangesAsync();

        var tokens = await IssueTokensAsync(user);
        return Ok(ApiResponse<AuthTokensResponse>.From(tokens, StatusCodes.Status200OK, $"Verify code: {verifyCode}"));
    }

    /// <summary>
    /// Authenticates a user and issues tokens.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthTokensResponse>>> Login(AuthLoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return Unauthorized(ApiResponse<object?>.From(null, StatusCodes.Status401Unauthorized, "Invalid credentials"));
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Use Google login"));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse<object?>.From(null, StatusCodes.Status401Unauthorized, "Invalid credentials"));
        }

        var tokens = await IssueTokensAsync(user);
        return Ok(ApiResponse<AuthTokensResponse>.From(tokens, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Returns the current user's profile.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> Me()
    {
        var userId = User.GetUserId();
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse<object?>.From(null, StatusCodes.Status404NotFound, "User not found"));
        }

        var dto = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            EmailVerified = user.EmailVerified
        };

        return Ok(ApiResponse<UserProfileDto>.From(dto, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Revokes a refresh token for the current user.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object?>>> Logout(AuthRefreshRequest request)
    {
        var tokenHash = JwtService.HashToken(request.RefreshToken);
        var token = await _dbContext.AuthRefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (token != null)
        {
            token.Revoked = true;
            await _dbContext.SaveChangesAsync();
        }

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Exchanges a refresh token for new tokens.
    /// </summary>
    [HttpPost("refetchToken")]
    public async Task<ActionResult<ApiResponse<AuthTokensResponse>>> Refresh(AuthRefreshRequest request)
    {
        var tokenHash = JwtService.HashToken(request.RefreshToken);
        var token = await _dbContext.AuthRefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (token == null || token.Revoked || token.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Unauthorized(ApiResponse<object?>.From(null, StatusCodes.Status401Unauthorized, "Invalid refresh token"));
        }

        var user = await _dbContext.Users.FindAsync(token.UserId);
        if (user == null)
        {
            return Unauthorized(ApiResponse<object?>.From(null, StatusCodes.Status401Unauthorized, "Invalid refresh token"));
        }

        token.Revoked = true;
        await _dbContext.SaveChangesAsync();

        var tokens = await IssueTokensAsync(user);
        return Ok(ApiResponse<AuthTokensResponse>.From(tokens, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Creates a password reset code for a user.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ForgotPassword(AuthForgotPasswordRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
        }

        var code = GenerateCode();
        _dbContext.PasswordResetCodes.Add(new PasswordResetCode
        {
            UserId = user.Id,
            CodeHash = JwtService.HashToken(code),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2)
        });
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(new { code }, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Resets a user's password using a reset code.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ResetPassword(AuthResetPasswordRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid request"));
        }

        var codeHash = JwtService.HashToken(request.Code);
        var code = await _dbContext.PasswordResetCodes.FirstOrDefaultAsync(c => c.UserId == user.Id && c.CodeHash == codeHash);
        if (code == null || code.ExpiresAt <= DateTimeOffset.UtcNow || code.UsedAt != null)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid code"));
        }

        code.UsedAt = DateTimeOffset.UtcNow;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Updates the current user's password.
    /// </summary>
    [Authorize]
    [HttpPost("update-password")]
    public async Task<ActionResult<ApiResponse<object?>>> UpdatePassword(AuthUpdatePasswordRequest request)
    {
        var userId = User.GetUserId();
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid request"));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid password"));
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Verifies a user's email address using a code.
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<object?>>> VerifyEmail(AuthVerifyEmailRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid request"));
        }

        var codeHash = JwtService.HashToken(request.Code);
        var code = await _dbContext.EmailVerificationCodes.FirstOrDefaultAsync(c => c.UserId == user.Id && c.CodeHash == codeHash);
        if (code == null || code.ExpiresAt <= DateTimeOffset.UtcNow || code.VerifiedAt != null)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid code"));
        }

        code.VerifiedAt = DateTimeOffset.UtcNow;
        user.EmailVerified = true;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(null, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Resends an email verification code.
    /// </summary>
    [HttpPost("resend-confirm")]
    public async Task<ActionResult<ApiResponse<object?>>> ResendConfirm(AuthResendVerifyRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return BadRequest(ApiResponse<object?>.From(null, StatusCodes.Status400BadRequest, "Invalid request"));
        }

        var code = GenerateCode();
        _dbContext.EmailVerificationCodes.Add(new EmailVerificationCode
        {
            UserId = user.Id,
            CodeHash = JwtService.HashToken(code),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        });
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object?>.From(new { code }, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Signs in a user using Google profile data.
    /// </summary>
    [HttpPost("google")]
    public async Task<ActionResult<ApiResponse<AuthTokensResponse>>> GoogleLogin(AuthGoogleLoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = null,
                EmailVerified = true
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        var tokens = await IssueTokensAsync(user);
        return Ok(ApiResponse<AuthTokensResponse>.From(tokens, StatusCodes.Status200OK));
    }

    /// <summary>
    /// Issues and persists access and refresh tokens for a user.
    /// </summary>
    private async Task<AuthTokensResponse> IssueTokensAsync(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        _dbContext.AuthRefreshTokens.Add(new AuthRefreshToken
        {
            UserId = user.Id,
            TokenHash = JwtService.HashToken(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenDays)
        });
        await _dbContext.SaveChangesAsync();

        return new AuthTokensResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    /// <summary>
    /// Generates a six-digit verification code.
    /// </summary>
    private static string GenerateCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }
}

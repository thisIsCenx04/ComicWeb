namespace ComicWeb.Application.DTOs;

public sealed class AuthLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class AuthRegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class AuthTokensResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}

public sealed class AuthRefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class AuthForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public sealed class AuthResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class AuthVerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public sealed class AuthResendVerifyRequest
{
    public string Email { get; set; } = string.Empty;
}

public sealed class AuthUpdatePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class AuthGoogleLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public sealed class UserProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool EmailVerified { get; set; }
}

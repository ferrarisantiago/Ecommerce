using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Common;

namespace Ecommerce.Application.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string? FullName { get; set; }
    public string Role { get; set; } = "User";
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public string Email { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
}

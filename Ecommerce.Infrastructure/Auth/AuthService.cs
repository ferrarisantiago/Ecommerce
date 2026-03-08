using Ecommerce.Application.Auth;
using Ecommerce.Application.Common;
using Ecommerce.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        IdentityUser? existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return ServiceResult<AuthResponse>.Fail("Email already registered.", 409);
        }

        string role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim();
        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        IdentityUser user = new()
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            string errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return ServiceResult<AuthResponse>.Fail(errors, 400);
        }

        await _userManager.AddToRoleAsync(user, role);
        IReadOnlyCollection<string> roles = (await _userManager.GetRolesAsync(user)).ToList();
        (string token, DateTime expiresAtUtc) = _jwtTokenGenerator.CreateToken(user, roles);

        _logger.LogInformation("API register success for {Email} with role {Role}.", request.Email, role);

        AuthResponse response = new()
        {
            Email = user.Email ?? request.Email,
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Roles = roles
        };

        return ServiceResult<AuthResponse>.Ok(response, 201);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        IdentityUser? user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return ServiceResult<AuthResponse>.Fail("Invalid credentials.", 401);
        }

        SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("API login failed for {Email}.", request.Email);
            return ServiceResult<AuthResponse>.Fail("Invalid credentials.", 401);
        }

        IReadOnlyCollection<string> roles = (await _userManager.GetRolesAsync(user)).ToList();
        (string token, DateTime expiresAtUtc) = _jwtTokenGenerator.CreateToken(user, roles);

        _logger.LogInformation("API login success for {Email}.", request.Email);

        return ServiceResult<AuthResponse>.Ok(new AuthResponse
        {
            Email = user.Email ?? request.Email,
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Roles = roles
        });
    }
}

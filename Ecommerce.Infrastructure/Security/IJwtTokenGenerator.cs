using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Infrastructure.Security;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(IdentityUser user, IReadOnlyCollection<string> roles);
}

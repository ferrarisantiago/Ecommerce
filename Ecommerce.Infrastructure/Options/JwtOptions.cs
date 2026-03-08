namespace Ecommerce.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Ecommerce.Api";
    public string Audience { get; set; } = "Ecommerce.Client";
    public string Key { get; set; } = "change-this-key-for-production-minimum-32chars";
    public int ExpirationMinutes { get; set; } = 120;
}

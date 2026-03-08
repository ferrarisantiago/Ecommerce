using System.Text;
using Ecommerce.Application.Auth;
using Ecommerce.Application.Products;
using Ecommerce_Datos.Datos;
using Ecommerce.Infrastructure.Auth;
using Ecommerce.Infrastructure.Options;
using Ecommerce.Infrastructure.Products;
using Ecommerce.Infrastructure.Seeding;
using Ecommerce.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaulConnection")
                                  ?? configuration.GetConnectionString("DefaultConnection")
                                  ?? "Server=sqlserver;Database=Ecommerce;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True;Encrypt=False";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductQueryService, ProductQueryService>();
        services.AddScoped<IDataSeeder, DataSeeder>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        JwtOptions jwt = new();
        configuration.GetSection(JwtOptions.SectionName).Bind(jwt);

        byte[] keyBytes = Encoding.UTF8.GetBytes(jwt.Key);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

        services.AddAuthorization();
        return services;
    }
}

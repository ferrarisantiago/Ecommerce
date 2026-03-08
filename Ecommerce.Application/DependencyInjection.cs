using Ecommerce.Application.Cart;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICartCalculatorService, CartCalculatorService>();
        return services;
    }
}

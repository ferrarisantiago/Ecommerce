using Ecommerce_Modelos;
using Ecommerce_Datos.Datos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Seeding;

public class DataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        ApplicationDbContext db,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        ILogger<DataSeeder> logger)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (_db.Database.IsRelational())
        {
            await _db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await _db.Database.EnsureCreatedAsync(cancellationToken);
        }

        await EnsureRoleAsync("Admin");
        await EnsureRoleAsync("User");

        await SeedAdminUserAsync();
        await SeedCatalogAsync(cancellationToken);
    }

    private async Task EnsureRoleAsync(string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@ecommerce.local";
        const string adminPassword = "Admin123!";

        IdentityUser? user = await _userManager.FindByEmailAsync(adminEmail);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            IdentityResult result = await _userManager.CreateAsync(user, adminPassword);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Unable to create default admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }
    }

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        if (!await _db.Categorias.AnyAsync(cancellationToken))
        {
            _db.Categorias.AddRange(
                new Categoria { NombreCategoria = "Electronics", MostrarOrden = 1 },
                new Categoria { NombreCategoria = "Home", MostrarOrden = 2 },
                new Categoria { NombreCategoria = "Gaming", MostrarOrden = 3 });
            await _db.SaveChangesAsync(cancellationToken);
        }

        if (!await _db.TiposAplicaciones.AnyAsync(cancellationToken))
        {
            _db.TiposAplicaciones.AddRange(
                new TipoAplicacion { Nombre = "General" },
                new TipoAplicacion { Nombre = "Premium" });
            await _db.SaveChangesAsync(cancellationToken);
        }

        if (!await _db.Productos.AnyAsync(cancellationToken))
        {
            int electronicsId = await _db.Categorias.Where(c => c.NombreCategoria == "Electronics").Select(c => c.Id).FirstAsync(cancellationToken);
            int homeId = await _db.Categorias.Where(c => c.NombreCategoria == "Home").Select(c => c.Id).FirstAsync(cancellationToken);
            int gamingId = await _db.Categorias.Where(c => c.NombreCategoria == "Gaming").Select(c => c.Id).FirstAsync(cancellationToken);
            int typeId = await _db.TiposAplicaciones.Select(t => t.Id).FirstAsync(cancellationToken);

            _db.Productos.AddRange(
                new Producto
                {
                    NombreProducto = "Wireless Headphones",
                    DescripcionCorta = "Noise-cancelling over-ear headphones.",
                    DescripcionLarga = "Comfortable wireless headphones with ANC and 30h battery.",
                    Precio = 199.99,
                    CategoriaId = electronicsId,
                    TipoAplicacionId = typeId,
                    Stock = 35,
                    MinimumStockLevel = 5,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                },
                new Producto
                {
                    NombreProducto = "Smart Lamp",
                    DescripcionCorta = "Wi-Fi smart lamp with warm/cool modes.",
                    DescripcionLarga = "Smart lamp compatible with voice assistants and schedules.",
                    Precio = 59.90,
                    CategoriaId = homeId,
                    TipoAplicacionId = typeId,
                    Stock = 50,
                    MinimumStockLevel = 8,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                },
                new Producto
                {
                    NombreProducto = "Mechanical Keyboard",
                    DescripcionCorta = "RGB mechanical keyboard for gaming.",
                    DescripcionLarga = "Tactile switches, RGB lighting, detachable USB-C cable.",
                    Precio = 129.50,
                    CategoriaId = gamingId,
                    TipoAplicacionId = typeId,
                    Stock = 20,
                    MinimumStockLevel = 3,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

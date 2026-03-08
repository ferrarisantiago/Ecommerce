using Ecommerce.Application.Products;
using Ecommerce_Datos.Datos;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Products;

public class ProductQueryService : IProductQueryService
{
    private readonly ApplicationDbContext _db;

    public ProductQueryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<ProductSummaryDto>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var products = await _db.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.NombreProducto,
                Description = p.DescripcionCorta,
                Price = Convert.ToDecimal(p.Precio),
                Stock = p.Stock,
                Category = p.Categoria != null ? p.Categoria.NombreCategoria : string.Empty
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}

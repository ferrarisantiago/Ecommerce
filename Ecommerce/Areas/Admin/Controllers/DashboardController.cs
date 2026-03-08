using Ecommerce_Datos.Datos;
using Ecommerce_Modelos.ViewModels;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WC.AdminRole)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IQueryable<LowStockAlertItemVM> lowStockQuery = BuildLowStockQuery();

            AdminDashboardVM vm = new()
            {
                ActiveProducts = await _db.Productos.CountAsync(p => !p.IsDeleted && p.IsActive),
                InactiveProducts = await _db.Productos.CountAsync(p => p.IsDeleted || !p.IsActive),
                LowStockCount = await lowStockQuery.CountAsync(),
                TotalOrders = await _db.Ordenes.CountAsync(),
                TotalRevenue = await _db.Ordenes
                    .Where(o => o.Estado != WC.EstadoCancelado)
                    .SumAsync(o => (double?)o.Total) ?? 0,
                LowStockItems = await lowStockQuery
                    .OrderBy(p => p.CurrentStock)
                    .ThenBy(p => p.ProductName)
                    .Take(8)
                    .ToListAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> StockAlerts()
        {
            List<LowStockAlertItemVM> lowStockProducts = await BuildLowStockQuery()
                .OrderBy(p => p.CurrentStock)
                .ThenBy(p => p.ProductName)
                .ToListAsync();

            return View(lowStockProducts);
        }

        public async Task<IActionResult> SalesReport(string range = SalesReportRanges.Last7Days)
        {
            (string normalizedRange, string label, DateTime? startUtc, DateTime? endUtc) = ResolveRange(range);

            IQueryable<SalesLine> detailsQuery =
                from detail in _db.OrdenDetalles.AsNoTracking()
                join order in _db.Ordenes.AsNoTracking() on detail.OrdenId equals order.Id
                join product in _db.Productos.AsNoTracking() on detail.ProductoId equals product.Id
                where order.Estado != WC.EstadoCancelado
                select new SalesLine
                {
                    OrderId = order.Id,
                    OrderedAt = order.FechaOrden,
                    ProductId = detail.ProductoId,
                    ProductName = product.NombreProducto,
                    Quantity = detail.Cantidad,
                    UnitPrice = detail.UnitPrice
                };

            if (startUtc.HasValue)
            {
                detailsQuery = detailsQuery.Where(x => x.OrderedAt >= startUtc.Value);
            }

            if (endUtc.HasValue)
            {
                detailsQuery = detailsQuery.Where(x => x.OrderedAt < endUtc.Value);
            }

            var rawLines = await detailsQuery.ToListAsync();

            SalesReportVM vm = new()
            {
                Range = normalizedRange,
                RangeLabel = label,
                StartUtc = startUtc,
                EndUtc = endUtc,
                TotalOrders = rawLines.Select(x => x.OrderId).Distinct().Count(),
                TotalUnitsSold = rawLines.Sum(x => x.Quantity),
                TotalRevenue = rawLines.Sum(x => x.Quantity * x.UnitPrice),
                TopProducts = rawLines
                    .GroupBy(x => new { x.ProductId, x.ProductName })
                    .Select(g => new SalesReportItemVM
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        TotalQuantitySold = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                    })
                    .OrderByDescending(x => x.TotalQuantitySold)
                    .ThenByDescending(x => x.TotalRevenue)
                    .ToList()
            };

            return View(vm);
        }

        private IQueryable<LowStockAlertItemVM> BuildLowStockQuery()
        {
            return _db.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => !p.IsDeleted && p.IsActive && p.Stock <= p.MinimumStockLevel)
                .Select(p => new LowStockAlertItemVM
                {
                    ProductId = p.Id,
                    ProductName = p.NombreProducto,
                    CurrentStock = p.Stock,
                    MinimumStockLevel = p.MinimumStockLevel,
                    CategoryName = p.Categoria != null ? p.Categoria.NombreCategoria : "Sin categoria",
                    IsActive = p.IsActive
                });
        }

        private static (string NormalizedRange, string Label, DateTime? StartUtc, DateTime? EndUtc) ResolveRange(string? range)
        {
            DateTime todayUtc = DateTime.UtcNow.Date;

            return range?.ToLowerInvariant() switch
            {
                SalesReportRanges.Today => (SalesReportRanges.Today, "Hoy", todayUtc, todayUtc.AddDays(1)),
                SalesReportRanges.Last30Days => (SalesReportRanges.Last30Days, "Ultimos 30 dias", todayUtc.AddDays(-29), todayUtc.AddDays(1)),
                SalesReportRanges.AllTime => (SalesReportRanges.AllTime, "Todo el tiempo", null, null),
                _ => (SalesReportRanges.Last7Days, "Ultimos 7 dias", todayUtc.AddDays(-6), todayUtc.AddDays(1))
            };
        }

        private sealed class SalesLine
        {
            public int OrderId { get; set; }
            public DateTime OrderedAt { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public double UnitPrice { get; set; }
        }
    }
}

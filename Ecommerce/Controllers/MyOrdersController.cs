using Ecommerce_Datos.Datos;
using Ecommerce_Modelos.ViewModels;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class MyOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MyOrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var orders = await _db.Ordenes
                .AsNoTracking()
                .Where(o => o.usuarioId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var order = await _db.Ordenes
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            bool canAccess = User.IsInRole(WC.AdminRole) || order.usuarioId == userId;
            if (!canAccess)
            {
                return Forbid();
            }

            var items = await _db.OrdenDetalles
                .AsNoTracking()
                .Include(d => d.Producto)
                .Where(d => d.OrdenId == id)
                .ToListAsync();

            OrderHistoryVM vm = new()
            {
                Orden = order,
                Items = items
            };

            return View(vm);
        }
    }
}

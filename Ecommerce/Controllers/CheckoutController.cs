using Ecommerce_Datos.Datos;
using Ecommerce_Modelos;
using Ecommerce_Modelos.ViewModels;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            ILogger<CheckoutController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            CheckoutVM vm = await BuildCheckoutVmAsync();
            if (!vm.Items.Any())
            {
                TempData[WC.Error] = "Tu carrito esta vacio.";
                return RedirectToAction("Index", "Carro");
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutVM model)
        {
            CheckoutVM vm = await BuildCheckoutVmAsync(model);
            if (!vm.Items.Any())
            {
                TempData[WC.Error] = "Tu carrito esta vacio.";
                return RedirectToAction("Index", "Carro");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            // Re-validate stock and commit order atomically.
            using var transaction = await _db.Database.BeginTransactionAsync();

            List<Producto> products = await _db.Productos
                .Where(p => vm.Items.Select(i => i.ProductoId).Contains(p.Id) && !p.IsDeleted && p.IsActive)
                .ToListAsync();

            foreach (CheckoutItemVM item in vm.Items)
            {
                Producto? product = products.FirstOrDefault(p => p.Id == item.ProductoId);
                if (product is null)
                {
                    ModelState.AddModelError(string.Empty, $"El producto '{item.NombreProducto}' ya no esta disponible.");
                    continue;
                }

                if (product.Stock < item.Quantity)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"El producto '{item.NombreProducto}' solo tiene {product.Stock} unidades disponibles.");
                }
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                vm = await BuildCheckoutVmAsync(model);
                return View(vm);
            }

            Orden orden = new()
            {
                usuarioId = userId,
                FechaOrden = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Estado = WC.EstadoPendiente,
                NombreCompleto = model.NombreCompleto,
                Email = model.Email,
                Telefono = model.Telefono,
                DireccionEnvio = model.DireccionEnvio,
                CiudadEnvio = model.CiudadEnvio,
                ProvinciaEnvio = model.ProvinciaEnvio,
                CodigoPostalEnvio = model.CodigoPostalEnvio,
                PaisEnvio = model.PaisEnvio,
                Total = vm.Total
            };

            _db.Ordenes.Add(orden);
            await _db.SaveChangesAsync();

            foreach (CheckoutItemVM item in vm.Items)
            {
                Producto product = products.First(p => p.Id == item.ProductoId);
                product.Stock -= item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;

                _db.OrdenDetalles.Add(new OrdenDetalle
                {
                    OrdenId = orden.Id,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            HttpContext.Session.Set(WC.SessionCarroCompras, new List<CarroCompras>());

            _logger.LogInformation(
                "Checkout completed. OrderId: {OrderId}, UserId: {UserId}, Total: {Total}",
                orden.Id,
                userId,
                orden.Total);

            return RedirectToAction(nameof(Confirmacion), new { id = orden.Id });
        }

        public async Task<IActionResult> Confirmacion(int id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            Orden? order = await _db.Ordenes
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id && o.usuarioId == userId);

            if (order is null)
            {
                return NotFound();
            }

            return View(order);
        }

        private async Task<CheckoutVM> BuildCheckoutVmAsync(CheckoutVM? source = null)
        {
            List<CarroCompras> cart = GetCart()
                .GroupBy(c => c.ProductoId)
                .Select(g => new CarroCompras
                {
                    ProductoId = g.Key,
                    Cantidad = g.Sum(x => x.Cantidad)
                })
                .ToList();
            CheckoutVM vm = source ?? new CheckoutVM();

            if (cart.Count == 0)
            {
                vm.Items = new List<CheckoutItemVM>();
                return vm;
            }

            List<int> productIds = cart.Select(c => c.ProductoId).Distinct().ToList();
            Dictionary<int, Producto> productLookup = await _db.Productos
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id) && !p.IsDeleted && p.IsActive)
                .ToDictionaryAsync(p => p.Id);

            List<CheckoutItemVM> items = new();
            bool cartWasUpdated = false;
            List<CarroCompras> normalizedCart = new();
            List<string> adjustedProducts = new();

            foreach (CarroCompras cartItem in cart)
            {
                if (!productLookup.TryGetValue(cartItem.ProductoId, out Producto? product))
                {
                    cartWasUpdated = true;
                    continue;
                }

                int quantity = cartItem.Cantidad < 1 ? 1 : cartItem.Cantidad;
                if (quantity > product.Stock)
                {
                    quantity = product.Stock;
                    cartWasUpdated = true;
                    adjustedProducts.Add(product.NombreProducto);
                }

                if (quantity <= 0)
                {
                    cartWasUpdated = true;
                    adjustedProducts.Add(product.NombreProducto);
                    continue;
                }

                normalizedCart.Add(new CarroCompras
                {
                    ProductoId = product.Id,
                    Cantidad = quantity
                });

                items.Add(new CheckoutItemVM
                {
                    ProductoId = product.Id,
                    NombreProducto = product.NombreProducto,
                    ImagenUrl = product.ImagenUrl,
                    UnitPrice = product.Precio,
                    Quantity = quantity,
                    AvailableStock = product.Stock
                });
            }

            vm.Items = items;

            if (cartWasUpdated)
            {
                HttpContext.Session.Set(WC.SessionCarroCompras, normalizedCart);
                if (adjustedProducts.Count > 0)
                {
                    TempData[WC.Error] = "Se ajustaron cantidades por stock disponible: " + string.Join(", ", adjustedProducts.Distinct());
                }
            }

            IdentityUser? user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                if (string.IsNullOrWhiteSpace(vm.Email))
                {
                    vm.Email = user.Email ?? string.Empty;
                }
                if (string.IsNullOrWhiteSpace(vm.Telefono))
                {
                    vm.Telefono = user.PhoneNumber ?? string.Empty;
                }
                if (string.IsNullOrWhiteSpace(vm.NombreCompleto))
                {
                    vm.NombreCompleto = user.UserName ?? string.Empty;
                }
            }

            return vm;
        }

        private List<CarroCompras> GetCart()
        {
            List<CarroCompras>? cart = HttpContext.Session.Get<List<CarroCompras>>(WC.SessionCarroCompras);
            return cart ?? new List<CarroCompras>();
        }
    }
}

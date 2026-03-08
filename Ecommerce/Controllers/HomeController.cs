using Ecommerce_Datos.Datos;
using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce_Modelos;
using Ecommerce_Modelos.ViewModels;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Ecommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly ICategoriaRepositorio _categoriaRepo;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext db,
            ICategoriaRepositorio categoriaRepo)
        {
            _logger = logger;
            _db = db;
            _categoriaRepo = categoriaRepo;
        }

        public async Task<IActionResult> Index(
            string? search,
            int? categoriaId,
            double? minPrice,
            double? maxPrice,
            string sort = "newest",
            int page = 1,
            int pageSize = 12)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is 8 or 12 or 16 ? pageSize : 12;
            sort = NormalizeSort(sort);

            IQueryable<Producto> query = _db.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.TipoAplicacion)
                .Where(p => !p.IsDeleted && p.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(p =>
                    p.NombreProducto.Contains(term) ||
                    p.DescripcionCorta.Contains(term) ||
                    p.DescripcionLarga.Contains(term));
            }

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Precio >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Precio <= maxPrice.Value);
            }

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Precio).ThenBy(p => p.NombreProducto),
                "price_desc" => query.OrderByDescending(p => p.Precio).ThenBy(p => p.NombreProducto),
                _ => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            };

            int totalItems = await query.CountAsync();
            int totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling((double)totalItems / pageSize);
            if (page > totalPages)
            {
                page = totalPages;
            }

            List<Producto> productos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            HomeVM homeVM = new()
            {
                Productos = productos,
                Categorias = _categoriaRepo.ObtenerTodos(orderBy: q => q.OrderBy(c => c.MostrarOrden).ThenBy(c => c.NombreCategoria)),
                Search = search,
                CategoriaId = categoriaId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(homeVM);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            Producto? producto = await _db.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.TipoAplicacion)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted && p.IsActive);

            if (producto is null)
            {
                return NotFound();
            }

            List<CarroCompras> carroComprasLista = GetCart();
            CarroCompras? currentCartItem = carroComprasLista.SingleOrDefault(x => x.ProductoId == id);

            if (currentCartItem is not null && currentCartItem.Cantidad > 0)
            {
                producto.TempCantidad = currentCartItem.Cantidad;
            }

            List<Producto> related = await _db.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.TipoAplicacion)
                .Where(p => !p.IsDeleted && p.IsActive && p.CategoriaId == producto.CategoriaId && p.Id != producto.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            DetalleVM detalleVM = new()
            {
                Producto = producto,
                ExisteEnCarro = currentCartItem is not null,
                RelatedProducts = related
            };

            return View(detalleVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Detalle")]
        public async Task<IActionResult> DetallePost(int id, DetalleVM detalleVM)
        {
            Producto? producto = await _db.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted && p.IsActive);

            if (producto is null)
            {
                return NotFound();
            }

            int requestedQty = detalleVM.Producto.TempCantidad;
            if (requestedQty < 1)
            {
                requestedQty = 1;
            }

            if (producto.Stock <= 0)
            {
                TempData[WC.Error] = "Este producto esta agotado.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            if (requestedQty > producto.Stock)
            {
                TempData[WC.Error] = "La cantidad solicitada excede el stock disponible.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            List<CarroCompras> carroComprasLista = GetCart();
            CarroCompras? existing = carroComprasLista.SingleOrDefault(x => x.ProductoId == id);
            if (existing is null)
            {
                carroComprasLista.Add(new CarroCompras { ProductoId = id, Cantidad = requestedQty });
            }
            else
            {
                int updatedQty = existing.Cantidad + requestedQty;
                if (updatedQty > producto.Stock)
                {
                    TempData[WC.Error] = $"No puedes agregar mas de {producto.Stock} unidades de este producto.";
                    return RedirectToAction(nameof(Detalle), new { id });
                }

                existing.Cantidad = updatedQty;
            }

            HttpContext.Session.Set(WC.SessionCarroCompras, carroComprasLista);
            TempData[WC.Exitosa] = "Producto agregado al carrito.";

            return RedirectToAction(nameof(Detalle), new { id });
        }

        public IActionResult RemoverDeCarro(int id)
        {
            List<CarroCompras> carroComprasLista = GetCart();
            CarroCompras? productoARemover = carroComprasLista.SingleOrDefault(x => x.ProductoId == id);
            if (productoARemover is not null)
            {
                carroComprasLista.Remove(productoARemover);
                HttpContext.Session.Set(WC.SessionCarroCompras, carroComprasLista);
            }

            return RedirectToAction(nameof(Detalle), new { id });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            ErrorViewModel model = new()
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode,
                FriendlyMessage = statusCode switch
                {
                    404 => "No encontramos la pagina que buscabas.",
                    403 => "No tienes permisos para acceder a este recurso.",
                    _ => "Ocurrio un error inesperado. Intenta nuevamente."
                }
            };

            return View(model);
        }

        private static string NormalizeSort(string? sort)
        {
            return sort switch
            {
                "price_asc" => "price_asc",
                "price_desc" => "price_desc",
                _ => "newest"
            };
        }

        private List<CarroCompras> GetCart()
        {
            List<CarroCompras>? cart = HttpContext.Session.Get<List<CarroCompras>>(WC.SessionCarroCompras);
            return cart ?? new List<CarroCompras>();
        }
    }
}

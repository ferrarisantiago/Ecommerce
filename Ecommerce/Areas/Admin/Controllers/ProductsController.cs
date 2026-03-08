using Ecommerce_Datos.Datos;
using Ecommerce_Modelos;
using Ecommerce_Modelos.ViewModels;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WC.AdminRole)]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            ApplicationDbContext db,
            IWebHostEnvironment environment,
            ILogger<ProductsController> logger)
        {
            _db = db;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index(bool includeDeleted = false, string? search = null, int? categoryId = null)
        {
            IQueryable<Producto> query = _db.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.TipoAplicacion);

            if (!includeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(p => p.NombreProducto.Contains(term));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoriaId == categoryId.Value);
            }

            List<Producto> products = await query
                .OrderBy(p => p.IsDeleted)
                .ThenByDescending(p => p.CreatedAt)
                .ThenBy(p => p.NombreProducto)
                .ToListAsync();

            ViewBag.IncludeDeleted = includeDeleted;
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = await _db.Categorias
                .AsNoTracking()
                .OrderBy(c => c.MostrarOrden)
                .ThenBy(c => c.NombreCategoria)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.NombreCategoria
                })
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            ProductoVM vm = await BuildProductVmAsync(id);
            if (id.HasValue && vm.Producto.Id == 0)
            {
                return NotFound();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductoVM vm)
        {
            IFormFile? file = HttpContext.Request.Form.Files.FirstOrDefault();
            bool isCreate = vm.Producto.Id == 0;

            if (isCreate && file is null)
            {
                ModelState.AddModelError("Producto.ImagenUrl", "La imagen es obligatoria al crear un producto.");
            }

            if (!ModelState.IsValid)
            {
                vm.ListaCategoria = await GetCategoriasAsync();
                vm.ListaTipoAplicacion = await GetTiposAplicacionAsync();
                return View(vm);
            }

            if (isCreate)
            {
                vm.Producto.CreatedAt = DateTime.UtcNow;
                vm.Producto.UpdatedAt = DateTime.UtcNow;
                vm.Producto.IsDeleted = false;
            }
            else
            {
                Producto? existing = await _db.Productos.FirstOrDefaultAsync(p => p.Id == vm.Producto.Id);
                if (existing is null)
                {
                    return NotFound();
                }

                vm.Producto.CreatedAt = existing.CreatedAt;
                vm.Producto.IsDeleted = existing.IsDeleted;
                vm.Producto.UpdatedAt = DateTime.UtcNow;
                if (vm.Producto.IsDeleted)
                {
                    vm.Producto.IsActive = false;
                }

                if (file is null)
                {
                    vm.Producto.ImagenUrl = existing.ImagenUrl;
                }
                else
                {
                    DeleteImage(existing.ImagenUrl);
                }
            }

            if (file is not null)
            {
                vm.Producto.ImagenUrl = await SaveImageAsync(file);
            }

            if (isCreate)
            {
                _db.Productos.Add(vm.Producto);
                _logger.LogInformation("Admin creo producto {ProductName}.", vm.Producto.NombreProducto);
                TempData[WC.Exitosa] = "Producto creado correctamente.";
            }
            else
            {
                _db.Productos.Update(vm.Producto);
                _logger.LogInformation("Admin actualizo producto {ProductId}.", vm.Producto.Id);
                TempData[WC.Exitosa] = "Producto actualizado correctamente.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int stock)
        {
            if (stock < 0)
            {
                TempData[WC.Error] = "El stock no puede ser negativo.";
                return RedirectToAction(nameof(Index));
            }

            Producto? product = await _db.Productos.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {
                return NotFound();
            }

            product.Stock = stock;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin actualizo stock de producto {ProductId} a {Stock}.", id, stock);

            TempData[WC.Exitosa] = "Stock actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            Producto? product = await _db.Productos.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {
                return NotFound();
            }

            product.IsDeleted = !product.IsDeleted;
            if (product.IsDeleted)
            {
                product.IsActive = false;
            }
            else
            {
                product.IsActive = true;
            }
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Admin {Action} producto {ProductId}.",
                product.IsDeleted ? "desactivo" : "reactivo",
                product.Id);

            TempData[WC.Exitosa] = product.IsDeleted
                ? "Producto desactivado (soft delete)."
                : "Producto restaurado.";

            return RedirectToAction(nameof(Index), new { includeDeleted = true });
        }

        private async Task<ProductoVM> BuildProductVmAsync(int? id)
        {
            ProductoVM vm = new()
            {
                Producto = new Producto(),
                ListaCategoria = await GetCategoriasAsync(),
                ListaTipoAplicacion = await GetTiposAplicacionAsync()
            };

            if (!id.HasValue)
            {
                return vm;
            }

            vm.Producto = await _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id.Value) ?? new Producto();
            return vm;
        }

        private async Task<IEnumerable<SelectListItem>> GetCategoriasAsync()
        {
            return await _db.Categorias
                .AsNoTracking()
                .OrderBy(c => c.MostrarOrden)
                .ThenBy(c => c.NombreCategoria)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.NombreCategoria
                })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetTiposAplicacionAsync()
        {
            return await _db.TiposAplicaciones
                .AsNoTracking()
                .OrderBy(t => t.Nombre)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Nombre
                })
                .ToListAsync();
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            string uploads = Path.Combine(_environment.WebRootPath, "imagenes", "productos");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploads, fileName);

            await using FileStream fileStream = new(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return fileName;
        }

        private void DeleteImage(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return;
            }

            string path = Path.Combine(_environment.WebRootPath, "imagenes", "productos", imageName);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}

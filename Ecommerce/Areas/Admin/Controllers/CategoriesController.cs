using Ecommerce_Datos.Datos;
using Ecommerce_Modelos;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WC.AdminRole)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext db, ILogger<CategoriesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categorias
                .AsNoTracking()
                .OrderBy(c => c.MostrarOrden)
                .ThenBy(c => c.NombreCategoria)
                .ToListAsync();

            return View(categories);
        }

        public IActionResult Create()
        {
            return View(new Categoria());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            _db.Categorias.Add(categoria);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin creo categoria {CategoryName}.", categoria.NombreCategoria);

            TempData[WC.Exitosa] = "Categoria creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            Categoria? categoria = await _db.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (categoria is null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            _db.Categorias.Update(categoria);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin actualizo categoria {CategoryId}.", categoria.Id);

            TempData[WC.Exitosa] = "Categoria actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Categoria? categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
            if (categoria is null)
            {
                return NotFound();
            }

            bool hasProducts = await _db.Productos.AnyAsync(p => p.CategoriaId == id);
            if (hasProducts)
            {
                TempData[WC.Error] = "No se puede eliminar una categoria con productos asociados.";
                return RedirectToAction(nameof(Index));
            }

            _db.Categorias.Remove(categoria);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Admin elimino categoria {CategoryId}.", id);

            TempData[WC.Exitosa] = "Categoria eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}

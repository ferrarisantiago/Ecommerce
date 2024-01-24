using Ecommerce_Datos.Datos;
using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce_Modelos;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class TipoAplicacionController : Controller
    {
        private readonly ITipoAplicacionRepositorio _tipoA;

        public TipoAplicacionController(ITipoAplicacionRepositorio tipoA)
        {
            _tipoA = tipoA;
        }
        public IActionResult Index()
        {
            IEnumerable<TipoAplicacion> ListaTipoAplicaciones = _tipoA.ObtenerTodos();

            return View(ListaTipoAplicaciones);
        }
        //Metodo get
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(TipoAplicacion tipoAplicacion)
        {
            if (ModelState.IsValid)
            {
                _tipoA.Agregar(tipoAplicacion);
                _tipoA.Grabar();
                return RedirectToAction("Index");
            }
            return View(tipoAplicacion);
        }

        //GET Editar
        public IActionResult Editar(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var obj = _tipoA.Obtener(Id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(TipoAplicacion tipoAplicacion)
        {
            if (ModelState.IsValid)
            {
                _tipoA.Actualizar(tipoAplicacion);
                _tipoA.Grabar();
                return RedirectToAction("Index");
            }
            return View(tipoAplicacion);
        }

        //GET Eliminar
        public IActionResult Eliminar(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var obj = _tipoA.Obtener(Id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(TipoAplicacion tipoAplicacion)
        {
            if (tipoAplicacion == null)
            {
                return NotFound();

            }
            _tipoA.Remover(tipoAplicacion);
            _tipoA.Grabar();
            return RedirectToAction("Index");
        }
    }
}

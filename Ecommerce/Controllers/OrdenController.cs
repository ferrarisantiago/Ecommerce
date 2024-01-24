using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce_Modelos;
using Ecommerce_Modelos.ViewModels;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers

{
    [Authorize(Roles =WC.AdminRole)]
    public class OrdenController : Controller
    {
        private readonly IOrdenRepositorio _ordenRepo;
        private readonly IOrdenDetalleRepositorio _ordenDetalleRepo;
        [BindProperty]
        public OrdenVM OrdenVM { get; set; }

        public OrdenController(IOrdenRepositorio ordenRepo, IOrdenDetalleRepositorio ordenDetalleRepo)
        {
            _ordenRepo = ordenRepo;
            _ordenDetalleRepo = ordenDetalleRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detalle(int id)
        {
            OrdenVM ordenVM = new OrdenVM()
            {
                Orden = _ordenRepo.ObtenerPrimero(o => o.Id == id),
                OrdenDetalle = _ordenDetalleRepo.ObtenerTodos(d => d.OrdenId == id, incluirPropiedades: "Producto")
            };
            return View(ordenVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Detalle()
        {
            List<CarroCompras> carroCompraLista = new List<CarroCompras>();
            OrdenVM.OrdenDetalle = _ordenDetalleRepo.ObtenerTodos(d=>d.OrdenId == OrdenVM.Orden.Id);

            foreach(var detalle in OrdenVM.OrdenDetalle)
            {
                CarroCompras carroCompras = new CarroCompras()
                {
                    ProductoId = detalle.ProductoId
                };
                carroCompraLista.Add(carroCompras);
            }
            HttpContext.Session.Clear();
            HttpContext.Session.Set(WC.SessionCarroCompras, carroCompraLista);
            HttpContext.Session.Set(WC.SessionOrdenId, OrdenVM.Orden.Id);
            return RedirectToAction("Index","Carro");
        }

        [HttpPost]
        public IActionResult Eliminar()
        {
            Orden orden = _ordenRepo.ObtenerPrimero(o => o.Id == OrdenVM.Orden.Id);
            IEnumerable<OrdenDetalle> ordenDetalle = _ordenDetalleRepo.ObtenerTodos(d => d.OrdenId == OrdenVM.Orden.Id);

            _ordenDetalleRepo.RemoverRango(ordenDetalle);
            _ordenRepo.Remover(orden);
            _ordenRepo.Grabar();

            return RedirectToAction("Index");
        }

        #region APIs

        [HttpGet]
        public IActionResult ObtenerListaOrdenes()
        {
            return Json(new { data = _ordenRepo.ObtenerTodos() });
        }
        #endregion
    }
}

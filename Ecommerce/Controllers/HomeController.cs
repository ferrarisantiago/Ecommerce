using Ecommerce_Datos.Datos;
using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce_Datos.Migrations;
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
        private readonly IProductoRepositorio _prodRepo;
        private readonly ICategoriaRepositorio _categoriaRepo;

        public HomeController(ILogger<HomeController> logger, IProductoRepositorio prodRepo, ICategoriaRepositorio categoRepo)
        {
            _logger = logger;
            _categoriaRepo = categoRepo;
            _prodRepo = prodRepo;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                //Productos = _db.Productos.Include(c => c.Categoria).Include(t => t.TipoAplicacion),
                //Categorias = _db.Categorias,
                Productos = _prodRepo.ObtenerTodos(incluirPropiedades: "Categoria,TipoAplicacion"),
                Categorias = _categoriaRepo.ObtenerTodos()
            };
            return View(homeVM);
        }

        public IActionResult Detalle(int Id)
        {
            List<CarroCompras> carroComprasLista = new List<CarroCompras>();
            if (HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras) != null
                && HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras).Count() > 0)
            {
                carroComprasLista = HttpContext.Session.Get<List<CarroCompras>>(WC.SessionCarroCompras);
            }

            DetalleVM detalleVM = new DetalleVM()
            {
                //Producto = _db.Productos.Include(c => c.Categoria).Include(t => t.TipoAplicacion)
                //                        .Where(p => p.Id == Id).FirstOrDefault(),

                Producto = _prodRepo.ObtenerPrimero(p => p.Id == Id,incluirPropiedades:"Categoria,TipoAplicacion"),
                ExisteEnCarro = false
            };

            foreach (CarroCompras item in carroComprasLista)
            {
                if (item.ProductoId == Id)
                {
                    detalleVM.ExisteEnCarro = true;
                }
            }

            return View(detalleVM);
        }

        [HttpPost,ActionName("Detalle")]
        public IActionResult DetallePost(int Id,DetalleVM detalleVM)
        {
            List<CarroCompras> carroComprasLista = new List<CarroCompras>();
            if(HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras) != null
                && HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras).Count()>0)
            {
                carroComprasLista = HttpContext.Session.Get<List<CarroCompras>>(WC.SessionCarroCompras);
            }
            carroComprasLista.Add(new CarroCompras { ProductoId = Id,Cantidad = detalleVM.Producto.TempCantidad });
            HttpContext.Session.Set(WC.SessionCarroCompras, carroComprasLista);

            return RedirectToAction("Index");
        }

        public IActionResult RemoverDeCarro(int Id)
        {
            List<CarroCompras> carroComprasLista = new List<CarroCompras>();
            if (HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras) != null
                && HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras).Count() > 0)
            {
                carroComprasLista = HttpContext.Session.Get<List<CarroCompras>>(WC.SessionCarroCompras);
            }
            var productoARemover = carroComprasLista.SingleOrDefault(x=> x.ProductoId == Id);
            if(productoARemover != null)
            {
                carroComprasLista.Remove(productoARemover);
            }
            HttpContext.Session.Set(WC.SessionCarroCompras, carroComprasLista);

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
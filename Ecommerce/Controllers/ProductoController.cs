using Ecommerce_Datos.Datos;
using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce_Modelos;
using Ecommerce_Modelos.ViewModels;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace Ecommerce.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductoController : Controller
    {
        private readonly IProductoRepositorio _repoProd;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController(IProductoRepositorio repoProd, IWebHostEnvironment webHostEnvironment)
        {
            _repoProd = repoProd;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            //IEnumerable<Producto> ListaProductos = _repoProd.Productos.Include(c=> c.Categoria)
            //                                        .Include(t=>t.TipoAplicacion);

            IEnumerable<Producto> ListaProductos = _repoProd.ObtenerTodos(incluirPropiedades: "Categoria,TipoAplicacion");
            return View(ListaProductos);
        }

        //GET Upsert
        public IActionResult Upsert(int? Id)
        {

            ProductoVM productoVM = new ProductoVM()
            {
                Producto = new Producto(),
                //ListaCategoria = _db.Categorias.Select(c=> new SelectListItem
                //{
                //    Text = c.NombreCategoria,
                //    Value = c.Id.ToString(),
                //}),
                //ListaTipoAplicacion = _db.TiposAplicaciones.Select(t=> new SelectListItem
                //{
                //    Text = t.Nombre,
                //    Value = t.Id.ToString(),
                //})

                ListaCategoria = _repoProd.ObtenerDrowdownList(WC.CategoriaNombre),
                ListaTipoAplicacion = _repoProd.ObtenerDrowdownList(WC.AplicacionNombre)

            };

            Producto producto = new Producto();
            if (Id == null)
            {
                //Crear el producto
                return View(productoVM);
            }
            else
            {
                productoVM.Producto = _repoProd.Obtener(Id.GetValueOrDefault());
                if (productoVM.Producto == null)
                {
                    return NotFound();
                }
                return View(productoVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductoVM productoVM)
        {
            if (ModelState.IsValid)
            {
                //Metodo que recibe la imagen
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                if(productoVM.Producto.Id == 0)
                {
                    //Crear nuevo producto
                    string upload = webRootPath + WC.ImagenRuta;
                    //Asignador de Id a la imagen que se va a grabar
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);
                    //se recorre mediante un using
                    using(var fileStream = new FileStream(Path.Combine(upload,fileName+extension),FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productoVM.Producto.ImagenUrl = fileName + extension;
                    _repoProd.Agregar(productoVM.Producto);
                }
                else
                {
                    //Actualizar
                    var objProducto = _repoProd.ObtenerPrimero(p => p.Id == productoVM.Producto.Id, isTraking: false);

                    if (files.Count>0)
                    {
                        string upload = webRootPath + WC.ImagenRuta;
                        //Asignador de Id a la imagen que se va a grabar
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        //borrar la imagen anterior
                        var anteriorFile = Path.Combine(upload,objProducto.ImagenUrl);
                        if (System.IO.File.Exists(anteriorFile))
                        {
                            System.IO.File.Delete(anteriorFile);
                        }

                        //se recorre mediante un using
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        productoVM.Producto.ImagenUrl = fileName + extension;
                    }
                    else
                    {
                        productoVM.Producto.ImagenUrl = objProducto.ImagenUrl;
                    }
                    _repoProd.Actualizar(productoVM.Producto);
                }
                _repoProd.Grabar();
                return RedirectToAction("Index");
            }
            //se llenan nuevamente las listas
            productoVM.ListaCategoria = _repoProd.ObtenerDrowdownList(WC.CategoriaNombre);
            productoVM.ListaTipoAplicacion = _repoProd.ObtenerDrowdownList(WC.AplicacionNombre);
            return View(productoVM);
        }

        //GET Eliminar
        public IActionResult Eliminar(int? Id)
        {
            if(Id == null || Id == 0)
            {
                return NotFound();
            }
            Producto producto = _repoProd.ObtenerPrimero(c => c.Id==Id, incluirPropiedades: "Categoria,TipoAplicacion");
            if(producto== null) { 
                return NotFound(); 
            }
            return View(producto);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(Producto producto)
        {
            if (producto == null)
            {
                return NotFound();
            }
            //Eliminar producto
            string upload = _webHostEnvironment.WebRootPath + WC.ImagenRuta;

            //borrar la imagen anterior
            var anteriorFile = Path.Combine(upload, producto.ImagenUrl);
            if (System.IO.File.Exists(anteriorFile))
            {
                System.IO.File.Delete(anteriorFile);
            }
            _repoProd.Remover(producto);
            _repoProd.Grabar();
            return RedirectToAction("Index");
        }
    }
}

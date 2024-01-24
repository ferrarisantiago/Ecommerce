using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce_Modelos.ViewModels;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class VentaController : Controller
    {
        private readonly IVentaRepositorio _ventaRepositorio;
        private readonly IVentaDetalleRepositorio _ventaDetalleRepositorio;

        public VentaController(IVentaRepositorio ventaRepositorio, IVentaDetalleRepositorio ventaDetalleRepositorio)
        {
            _ventaRepositorio = ventaRepositorio;
            _ventaDetalleRepositorio = ventaDetalleRepositorio;
        }

        public IActionResult Index(string buscarNombre=null,string buscarEmail=null,string buscarTelefono=null,string Estado=null)
        {
            VentaVM ventaVM = new VentaVM()
            {
                ListaVenta = _ventaRepositorio.ObtenerTodos(),
                ListaEstadoVenta = WC.ListaEstados.ToList().Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = e,
                    Value = e
                })
            };

            if (!string.IsNullOrEmpty(buscarNombre))
            {
                ventaVM.ListaVenta = ventaVM.ListaVenta.Where(i=>i.NombreCompleto.ToLower().Contains(buscarNombre.ToLower()));
            }

            if (!string.IsNullOrEmpty(buscarEmail))
            {
                ventaVM.ListaVenta = ventaVM.ListaVenta.Where(i => i.Email.ToLower().Contains(buscarEmail.ToLower()));
            }

            if (!string.IsNullOrEmpty(buscarTelefono))
            {
                ventaVM.ListaVenta = ventaVM.ListaVenta.Where(i => i.Telefono.ToLower().Contains(buscarTelefono.ToLower()));
            }

            if (!string.IsNullOrEmpty(Estado) && Estado != "--Estado--")
            {
                ventaVM.ListaVenta = ventaVM.ListaVenta.Where(i => i.EstadoVenta.ToLower().Contains(Estado.ToLower()));
            }

            return View(ventaVM);
        }
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Modelos.ViewModels
{
    public class VentaVM
    {
        public IEnumerable<Venta> ListaVenta { get; set; }

        public IEnumerable<SelectListItem> ListaEstadoVenta { get; set; }

        public string Estado { get; set; }
    }
}

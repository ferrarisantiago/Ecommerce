using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce_Modelos.ViewModels
{
    public class ProductoVM
    {
        public Producto Producto { get; set; }

        public IEnumerable<SelectListItem>?  ListaCategoria { get; set; }

        public IEnumerable<SelectListItem>? ListaTipoAplicacion { get; set; }
    }
}

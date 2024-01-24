namespace Ecommerce_Modelos.ViewModels
{
    public class UserProductosVM
    {
        public UserProductosVM()
        {
            ProductoLista = new List<Producto>();
        }
        public UsuarioAplicacion UsuarioAplicacion { get; set; }
        public IList<Producto> ProductoLista { get; set; }
    }
}

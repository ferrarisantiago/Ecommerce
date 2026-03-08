namespace Ecommerce_Modelos.ViewModels
{
    public class DetalleVM
    {
        public DetalleVM()
        {
            Producto = new Producto();
            RelatedProducts = new List<Producto>();
        }
        public Producto Producto { get; set; }
        public bool ExisteEnCarro { get; set; }
        public IEnumerable<Producto> RelatedProducts { get; set; }

    }
}

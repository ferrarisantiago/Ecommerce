namespace Ecommerce_Modelos.ViewModels
{
    public class OrderHistoryVM
    {
        public Orden Orden { get; set; } = new Orden();
        public IEnumerable<OrdenDetalle> Items { get; set; } = new List<OrdenDetalle>();
    }
}

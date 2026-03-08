namespace Ecommerce_Modelos.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Producto> Productos { get; set; } = new List<Producto>();
        public IEnumerable<Categoria> Categorias { get; set; } = new List<Categoria>();

        public string? Search { get; set; }
        public int? CategoriaId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public string Sort { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}

using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Modelos.ViewModels
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(120)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress]
        [StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El telefono es obligatorio")]
        [Phone]
        [StringLength(30)]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La direccion es obligatoria")]
        [StringLength(250)]
        public string DireccionEnvio { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [StringLength(100)]
        public string CiudadEnvio { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ProvinciaEnvio { get; set; }

        [StringLength(20)]
        public string? CodigoPostalEnvio { get; set; }

        [StringLength(100)]
        public string? PaisEnvio { get; set; }

        public IList<CheckoutItemVM> Items { get; set; } = new List<CheckoutItemVM>();

        public double Total => Items.Sum(item => item.LineTotal);
    }

    public class CheckoutItemVM
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int AvailableStock { get; set; }
        public double LineTotal => Quantity * UnitPrice;
    }
}

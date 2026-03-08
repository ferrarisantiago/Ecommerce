using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Modelos
{
    public class Orden
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string usuarioId { get; set; }

        [ForeignKey("usuarioId")]
        public UsuarioAplicacion UsuarioAplicacion { get; set; }

        [Required]
        public DateTime FechaOrden { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(40)]
        public string Estado { get; set; } = "Pendiente";

        [Required]
        public string Telefono { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(250)]
        public string DireccionEnvio { get; set; }

        [Required]
        [StringLength(100)]
        public string CiudadEnvio { get; set; }

        [StringLength(100)]
        public string? ProvinciaEnvio { get; set; }

        [StringLength(20)]
        public string? CodigoPostalEnvio { get; set; }

        [StringLength(100)]
        public string? PaisEnvio { get; set; }

        [Range(0, double.MaxValue)]
        public double Total { get; set; }
    }
}

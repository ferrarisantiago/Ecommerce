using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Modelos
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        public string CreadoPorUsusarioId { get; set; }

        [ForeignKey("CreadoPorUsusarioId")]
        public UsuarioAplicacion UsuarioAplicacion { get; set; }

        [Required]
        public DateTime FechaVenta { get; set; }

        public DateTime FechaEnvio { get; set; }

        [Required]
        public double FinalVentaTotal { get; set; }

        public string EstadoVenta { get; set; }

        public DateTime FechaPago { get; set; }

        public string TransaccionId { get; set; }  // BrainTree 

        [Required]
        public string Telefono { get; set; }

        [Required]
        public string Direccion { get; set; }

        [Required]
        public string Ciudad { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        public string Email { get; set; }
    }
}

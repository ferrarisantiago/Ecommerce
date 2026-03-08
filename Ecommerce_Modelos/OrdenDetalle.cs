using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Modelos
{
    public class OrdenDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdenId { get; set; }

        [ForeignKey("OrdenId")]
        [Required]
        public Orden Orden { get; set; }

        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; }

        [Range(1, 1000)]
        public int Cantidad { get; set; }

        [Range(0, double.MaxValue)]
        public double UnitPrice { get; set; }

    }
}

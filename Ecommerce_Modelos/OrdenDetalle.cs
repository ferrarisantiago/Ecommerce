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

    }
}

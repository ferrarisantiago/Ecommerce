
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_Modelos
{
    public class Producto
    {
        public Producto()
        {
            TempCantidad = 1;
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Nombre del producto es obligatorio")]
        public string NombreProducto { get; set; }

        [Required(ErrorMessage = "Descripcion corta del producto es obligatorio")]
        public string DescripcionCorta { get; set; }

        [Required(ErrorMessage = "Descripcion producto es obligatorio")]
        public string DescripcionLarga { get; set; }

        [Required(ErrorMessage = "El precio producto es obligatorio")]
        [Range(1,double.MaxValue,ErrorMessage ="El precio debe ser mayor a cero")]
        public double Precio { get; set; }


        public string? ImagenUrl { get; set; }

        //Foreign Key
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        public int TipoAplicacionId { get; set; }

        [ForeignKey("TipoAplicacionId")]
        public virtual TipoAplicacion? TipoAplicacion { get; set; }

        [NotMapped]   //Propiedad que no agrega el campo
        [Range(1,1000)]
        public int TempCantidad { get; set; }

    }
}

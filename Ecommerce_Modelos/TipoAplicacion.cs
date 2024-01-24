using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Modelos
{
    public class TipoAplicacion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="El nombre es obligatorio.")]
        public string Nombre { get; set; }
    }
}

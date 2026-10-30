using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinariaULP.Models
{
    [Table("Especies")]
    public class Especie
    {
        [Key]
        [Display(Name = "Código")]
        public int IdEspecie { get; set; }

        [Required(ErrorMessage = "El nombre de la especie es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras.")]
        [Display(Name = "Especie")]
        public string Nombre { get; set; } = string.Empty;

        public override string ToString() => Nombre;
    }
}
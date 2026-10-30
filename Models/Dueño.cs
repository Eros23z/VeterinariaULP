using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinariaULP.Models
{
    [Table("Dueños")]
    public class Dueño
    {
        [Key]
        [Display(Name = "Código")]
        public int IdDueño { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe contener 7 u 8 dígitos numéricos.")]
        [Display(Name = "DNI")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Phone(ErrorMessage = "Formato de teléfono inválido.")]
        [StringLength(30)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [StringLength(100)]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El domicilio es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Display(Name = "Activo")]
        public bool Estado { get; set; } = true;

        [NotMapped]
        [Display(Name = "Dueño")]
        public string NombreCompleto => $"{Apellido}, {Nombre}";

        public override string ToString() => $"{Apellido}, {Nombre} (DNI: {Dni})";
    }
}

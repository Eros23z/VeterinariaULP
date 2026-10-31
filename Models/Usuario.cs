using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinariaULP.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Display(Name = "Código")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Rol { get; set; } = "Veterinario"; // 'Administrador' o 'Veterinario'

        [StringLength(30)]
        [Display(Name = "Matrícula Profesional")]
        public string? Matricula { get; set; }

        [StringLength(255)]
        public string? Avatar { get; set; }

        public bool Estado { get; set; } = true;

        [NotMapped]
        [Display(Name = "Profesional")]
        public string NombreCompleto => $"Dr(a). {Apellido}, {Nombre} {(string.IsNullOrEmpty(Matricula) ? "" : $"({Matricula})")}";

        public override string ToString() => NombreCompleto;
    }
}
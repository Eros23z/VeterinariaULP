using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VeterinariaULP.Models;

namespace VeterinariaULP.Models
{
    [Table("Mascotas")]
    public class Mascota
    {
        [Key]
        [Display(Name = "Código")]
        public int IdMascota { get; set; }

        [Required(ErrorMessage = "El nombre de la mascota es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Nombre de la Mascota")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La raza es obligatoria.")]
        [StringLength(50)]
        [Display(Name = "Raza")]
        public string Raza { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sexo es obligatorio.")]
        [StringLength(15)]
        [Display(Name = "Sexo")]
        public string Sexo { get; set; } = "Macho"; // 'Macho' o 'Hembra'

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; } = DateTime.Today.AddYears(-1);

        [StringLength(255)]
        [Display(Name = "Foto")]
        public string? Foto { get; set; }

        [StringLength(255)]
        [Display(Name = "Observaciones Médicas")]
        public string? Observaciones { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Relación con Especie
        [Required(ErrorMessage = "Debe seleccionar una especie.")]
        [Display(Name = "Especie")]
        public int IdEspecie { get; set; }

        [ForeignKey(nameof(IdEspecie))]
        public Especie? Especie { get; set; }

        // Relación con Dueño
        [Required(ErrorMessage = "Debe asignar un dueño.")]
        [Display(Name = "Dueño")]
        public int IdDueño { get; set; }

        [ForeignKey(nameof(IdDueño))]
        public Dueño? Dueño { get; set; }

        public override string ToString() => $"{Nombre} ({Raza}) - Dueño: {Dueño?.NombreCompleto}";
    }
}
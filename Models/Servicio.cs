using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinariaULP.Models
{
    [Table("Servicios")]
    public class Servicio
    {
        [Key]
        [Display(Name = "Código")]
        public int IdServicio { get; set; }

        [Required(ErrorMessage = "La descripción del servicio es obligatoria.")]
        [StringLength(100, ErrorMessage = "No puede exceder los 100 caracteres.")]
        [Display(Name = "Descripción del Servicio")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El arancel base es obligatorio.")]
        [Range(0.01, 9999999.99, ErrorMessage = "El precio debe ser mayor a cero.")]
        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Precio Base ($)")]
        public decimal PrecioBase { get; set; }

        [Required(ErrorMessage = "La duración estimada es obligatoria.")]
        [Range(5, 480, ErrorMessage = "La duración debe oscilar entre 5 y 480 minutos.")]
        [Display(Name = "Duración (Minutos)")]
        public int DuracionMinutos { get; set; } = 30;

        [Display(Name = "Habilitado")]
        public bool Activo { get; set; } = true;

        public override string ToString() => $"{Descripcion} - ${PrecioBase:N2}";
    }
}
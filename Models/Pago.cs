using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VeterinariaULP.Models;

namespace VeterinariaULP.Models
{
    [Table("Pagos")]
    public class Pago
    {
        [Key]
        [Display(Name = "N° Transacción")]
        public int IdPago { get; set; }

        [Required]
        [Display(Name = "Fecha de Cobro")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El concepto de cobro es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El importe es obligatorio.")]
        [Range(0.01, 9999999.99, ErrorMessage = "El importe debe ser mayor a cero.")]
        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Importe ($)")]
        public decimal Importe { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Método de Pago")]
        public string MetodoPago { get; set; } = "Efectivo"; // 'Efectivo', 'Transferencia', 'Tarjeta'

        [Display(Name = "Anulado")]
        public bool Anulado { get; set; } = false;

        // Relación con AtencionClinica
        [Required]
        public int IdAtencion { get; set; }

        [ForeignKey(nameof(IdAtencion))]
        public AtencionClinica? Atencion { get; set; }

        // Auditoría
        [Required]
        [Display(Name = "Operador")]
        public int UsuarioCreaId { get; set; } = 1;

        [ForeignKey(nameof(UsuarioCreaId))]
        public Usuario? UsuarioCrea { get; set; }
    }
}
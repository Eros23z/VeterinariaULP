using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VeterinariaULP.Models;

namespace VeterinariaULP.Models
{
    [Table("AtencionesClinicas")]
    public class AtencionClinica
    {
        [Key]
        [Display(Name = "N° Ficha")]
        public int IdAtencion { get; set; }

        [Required]
        [Display(Name = "Fecha y Hora de Consulta")]
        public DateTime FechaAtencion { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Debe registrar el peso actual del paciente.")]
        [Range(0.05, 250.00, ErrorMessage = "El peso debe estar entre 0.05 kg y 250 kg.")]
        [Column(TypeName = "decimal(5, 2)")]
        [Display(Name = "Peso Actual (kg)")]
        public decimal PesoActual { get; set; }

        [Required(ErrorMessage = "Describa los síntomas o el cuadro clínico de ingreso.")]
        [StringLength(500, ErrorMessage = "No puede superar los 500 caracteres.")]
        [Display(Name = "Síntomas Observados")]
        public string Sintomas { get; set; } = string.Empty;

        [Required(ErrorMessage = "El diagnóstico profesional es obligatorio.")]
        [StringLength(500, ErrorMessage = "No puede superar los 500 caracteres.")]
        [Display(Name = "Diagnóstico")]
        public string Diagnostico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar el tratamiento o prescripción médica.")]
        [StringLength(500, ErrorMessage = "No puede superar los 500 caracteres.")]
        [Display(Name = "Tratamiento / Indicaciones")]
        public string TratamientoIndicado { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Estudio / Radiografía Adjunta")]
        public string? ArchivoEstudio { get; set; }

        // Relación 1 a 1 con Turno
        [Required]
        [Display(Name = "Turno Asociado")]
        public int IdTurno { get; set; }

        [ForeignKey(nameof(IdTurno))]
        public Turno? Turno { get; set; }

        // Relación 1 a 1 con Pago
        public Pago? Pago { get; set; }
    }
}
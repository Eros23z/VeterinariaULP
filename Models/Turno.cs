using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VeterinariaULP.Models;

namespace VeterinariaULP.Models
{
    [Table("Turnos")]
    public class Turno
    {
        [Key]
        [Display(Name = "N° Cita")]
        public int IdTurno { get; set; }

        [Required(ErrorMessage = "La fecha y hora del turno son obligatorias.")]
        [Display(Name = "Fecha y Hora")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaHora { get; set; } = DateTime.Today.AddDays(1).AddHours(9);

        [Required(ErrorMessage = "Debe especificar el motivo de consulta.")]
        [StringLength(150, ErrorMessage = "El motivo no puede superar los 150 caracteres.")]
        [Display(Name = "Motivo de la Cita")]
        public string Motivo { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // 'Pendiente', 'Atendido', 'Cancelado'

        // Relación con Mascota
        [Required(ErrorMessage = "Debe seleccionar la mascota a atender.")]
        [Display(Name = "Paciente")]
        public int IdMascota { get; set; }

        [ForeignKey(nameof(IdMascota))]
        public Mascota? Mascota { get; set; }

        // Relación con Servicio
        [Required(ErrorMessage = "Debe seleccionar el servicio médico.")]
        [Display(Name = "Prestación / Servicio")]
        public int IdServicio { get; set; }

        [ForeignKey(nameof(IdServicio))]
        public Servicio? Servicio { get; set; }

        // Relación con Veterinario
        [Required(ErrorMessage = "Debe asignar un profesional.")]
        [Display(Name = "Veterinario Asignado")]
        public int IdVeterinario { get; set; }

        [ForeignKey(nameof(IdVeterinario))]
        public Usuario? Veterinario { get; set; }

        [NotMapped]
        public DateTime FechaHoraFin => Servicio != null
            ? FechaHora.AddMinutes(Servicio.DuracionMinutos)
            : FechaHora.AddMinutes(30);

        public override string ToString() => $"Turno #{IdTurno} - {FechaHora:dd/MM/yyyy HH:mm} - {Mascota?.Nombre} ({Servicio?.Descripcion})";
    }
}
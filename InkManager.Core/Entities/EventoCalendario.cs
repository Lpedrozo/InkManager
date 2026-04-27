using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class EventoCalendario : BaseEntity
    {
        [Required]
        public int CalendarioId { get; set; }

        public int? CitaId { get; set; }

        [Required]
        [MaxLength(20)]
        public string TipoEvento { get; set; } = string.Empty; // 'cita', 'descanso', 'evento', 'bloqueo'

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        public DateTime FechaHoraFin { get; set; }

        [MaxLength(7)]
        public string? Color { get; set; }

        [MaxLength(200)]
        public string? GoogleEventId { get; set; }

        // Navigation properties
        public virtual Calendario Calendario { get; set; } = null!;
        public virtual Cita? Cita { get; set; }
    }
}
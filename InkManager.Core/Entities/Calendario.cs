using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Calendario : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } = string.Empty; // 'estudio', 'artista'

        [Required]
        public int EntidadId { get; set; } // EstudioId o UsuarioId

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public bool EsPrincipal { get; set; } = false;

        [MaxLength(200)]
        public string? GoogleCalendarId { get; set; }

        [MaxLength(7)]
        public string Color { get; set; } = "#3788d8";

        public bool Activo { get; set; } = true;

        // Navigation properties
        public virtual ICollection<EventoCalendario> Eventos { get; set; } = new List<EventoCalendario>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class HistorialEstadoCita : BaseEntity
    {
        [MaxLength(20)]
        public string? EstadoAnterior { get; set; }

        [Required]
        [MaxLength(20)]
        public string EstadoNuevo { get; set; } = string.Empty;

        public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string UsuarioTipo { get; set; } = string.Empty;

        [Required]
        public int UsuarioId { get; set; }

        [MaxLength(500)]
        public string? Comentario { get; set; }

        // Foreign keys
        public int CitaId { get; set; }

        // Navigation properties
        public virtual Cita Cita { get; set; } = null!;
    }
}
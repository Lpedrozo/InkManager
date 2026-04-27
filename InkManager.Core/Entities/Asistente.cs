using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Asistente : BaseEntity
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int ArtistaAsistidoId { get; set; }

        [Required]
        public int EstudioId { get; set; }

        [MaxLength(500)]
        public string? Permisos { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Usuario ArtistaAsistido { get; set; } = null!;
        public virtual Estudio Estudio { get; set; } = null!;
    }
}
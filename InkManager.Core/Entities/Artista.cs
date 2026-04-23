using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Artista : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Especialidad { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FotoPerfilUrl { get; set; }

        public decimal ComisionPorcentaje { get; set; } = 0;

        // Foreign keys
        public int? EstudioId { get; set; }

        // Navigation properties
        public virtual Estudio? Estudio { get; set; }
        public virtual ICollection<Asistente> Asistentes { get; set; } = new List<Asistente>();
        public virtual ICollection<Cita> Citas { get; set; } = new List<Cita>();
        public virtual ICollection<Cubiculo> CubiculosAsignados { get; set; } = new List<Cubiculo>();
    }
}
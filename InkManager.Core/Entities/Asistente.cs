using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Asistente : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PermisosJson { get; set; }

        // Foreign keys
        public int ArtistaId { get; set; }

        // Navigation properties
        public virtual Artista Artista { get; set; } = null!;
        public virtual ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Cliente : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        public string? AlergiasConocidas { get; set; }

        public string? NotasGenerales { get; set; }

        // Sobrescribir FechaRegistro para mantener compatibilidad
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
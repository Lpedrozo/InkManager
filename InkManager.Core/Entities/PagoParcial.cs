using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class PagoParcial : BaseEntity
    {
        [Required]
        [Range(0, 999999.99)]
        public decimal Monto { get; set; }

        [Required]
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(30)]
        public string MetodoPago { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReferenciaPago { get; set; }

        [MaxLength(500)]
        public string? Nota { get; set; }

        // Foreign keys
        public int CitaId { get; set; }

        // Navigation properties
        public virtual Cita Cita { get; set; } = null!;
    }
}
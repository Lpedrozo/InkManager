using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class MetricaDiaria : BaseEntity
    {
        [Required]
        public DateTime Fecha { get; set; }

        public int TotalCitasCompletadas { get; set; } = 0;

        public decimal TotalIngresos { get; set; } = 0;

        public decimal TotalHorasTrabajadas { get; set; } = 0;

        // Foreign keys
        public int EstudioId { get; set; }
        public int ArtistaId { get; set; }

        // Navigation properties
        public virtual Estudio Estudio { get; set; } = null!;
        public virtual Artista Artista { get; set; } = null!;
    }
}
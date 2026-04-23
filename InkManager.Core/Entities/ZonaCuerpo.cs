using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class ZonaCuerpo : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;

        public string? CoordenadasJson { get; set; }

        public int OrdenVisual { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
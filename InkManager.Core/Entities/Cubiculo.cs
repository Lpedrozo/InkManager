using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Cubiculo : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public decimal PosicionX { get; set; } = 0;
        public decimal PosicionY { get; set; } = 0;
        public decimal PosicionZ { get; set; } = 0;

        public decimal Ancho { get; set; } = 200;
        public decimal Largo { get; set; } = 200;
        public decimal Alto { get; set; } = 200;

        [MaxLength(7)]
        public string ColorHex { get; set; } = "#CCCCCC";

        // Foreign keys
        public int EstudioId { get; set; }
        public int? UsuarioAsignadoId { get; set; }

        // Navigation properties
        public virtual Estudio Estudio { get; set; } = null!;
        public virtual Usuario? UsuarioAsignado { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class ClienteArtista
    {
        public int ClienteId { get; set; }
        public int ArtistaId { get; set; }
        public int EstudioId { get; set; }
        public DateTime FechaAsociacion { get; set; } = DateTime.UtcNow;
        public string? Notas { get; set; }
        [MaxLength(30)]
        public string? EstadoCliente { get; set; } = "activo";
        public DateTime? UltimoContacto { get; set; }
        public DateTime? ProximoContacto { get; set; }
        public string? NotaSeguimiento { get; set; }
        public virtual Usuario Cliente { get; set; } = null!;
        public virtual Usuario Artista { get; set; } = null!;
        public virtual Estudio Estudio { get; set; } = null!;
    }
}
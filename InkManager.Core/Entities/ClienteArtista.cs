namespace InkManager.Core.Entities
{
    public class ClienteArtista
    {
        public int ClienteId { get; set; }
        public int ArtistaId { get; set; }
        public int EstudioId { get; set; }
        public DateTime FechaAsociacion { get; set; } = DateTime.UtcNow;
        public string? Notas { get; set; }

        // Navigation properties
        public virtual Usuario Cliente { get; set; } = null!;
        public virtual Usuario Artista { get; set; } = null!;
        public virtual Estudio Estudio { get; set; } = null!;
    }
}
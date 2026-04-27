namespace InkManager.Core.Entities
{
    public class EstudioUsuario
    {
        public int EstudioId { get; set; }
        public int UsuarioId { get; set; }

        // Propiedades adicionales
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
        public string? RolEnEstudio { get; set; }

        // Nuevas propiedades
        public string? HorarioLaboral { get; set; } // JSON con horarios por día
        public bool EsPrincipal { get; set; } = false;

        // Navigation properties
        public virtual Estudio Estudio { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
namespace InkManager.Core.Entities
{
    public class EstudioUsuario
    {
        public int EstudioId { get; set; }
        public int UsuarioId { get; set; }

        // Propiedades adicionales (opcional)
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
        public string? RolEnEstudio { get; set; } // Ej: "dueño", "empleado", "freelance"

        // Navigation properties
        public virtual Estudio Estudio { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
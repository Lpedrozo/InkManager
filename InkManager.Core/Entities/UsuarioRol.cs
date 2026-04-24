namespace InkManager.Core.Entities
{
    public class UsuarioRol
    {
        public int UsuarioId { get; set; }
        public int RolId { get; set; }

        // Navigation
        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Rol Rol { get; set; } = null!;
    }
}
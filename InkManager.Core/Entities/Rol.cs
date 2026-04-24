using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Rol : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        // Navigation
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }
}
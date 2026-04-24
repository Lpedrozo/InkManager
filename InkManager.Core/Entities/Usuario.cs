using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Usuario : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(500)]
        public string? FotoPerfilUrl { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime? UltimoAcceso { get; set; }

        // Navigation properties
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
        public virtual ICollection<EstudioUsuario> EstudioUsuarios { get; set; } = new List<EstudioUsuario>();

        // Relaciones con citas
        public virtual ICollection<Cita> CitasComoCliente { get; set; } = new List<Cita>();
        public virtual ICollection<Cita> CitasComoArtista { get; set; } = new List<Cita>();

        // Relación con Cubiculo
        public virtual ICollection<Cubiculo> CubiculosAsignados { get; set; } = new List<Cubiculo>();

        // Relación con MetricasDiarias
        public virtual ICollection<MetricaDiaria> MetricasDiarias { get; set; } = new List<MetricaDiaria>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Estudio : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        public string? ConfiguracionJson { get; set; }

        // Navigation properties
        public virtual ICollection<EstudioUsuario> EstudioUsuarios { get; set; } = new List<EstudioUsuario>();
        public virtual ICollection<Cubiculo> Cubiculos { get; set; } = new List<Cubiculo>();
        public virtual ICollection<ConfiguracionCorreo> ConfiguracionesCorreo { get; set; } = new List<ConfiguracionCorreo>();
    }
}
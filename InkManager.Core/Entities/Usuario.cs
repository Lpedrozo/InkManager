using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Usuario : BaseEntity
    {
        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;  // Ahora nullable

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;  // Nuevo campo requerido

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FotoPerfilUrl { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime? UltimoAcceso { get; set; }

        // Relación con Estudio
        public int? EstudioId { get; set; }
        public virtual Estudio? Estudio { get; set; }

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
        // Agrega estas propiedades dentro de la clase Usuario:

        // Relación con asistentes (como artista asistido)
        public virtual ICollection<Asistente> AsistentesQueAyudan { get; set; } = new List<Asistente>();

        // Relación con asistentes (como asistente)
        public virtual Asistente? AsistenteAsignado { get; set; }

        // Relación con clientes que ha atendido
        public virtual ICollection<ClienteArtista> ClientesAtendidos { get; set; } = new List<ClienteArtista>();

        // Relación con artistas que lo atienden (como cliente)
        public virtual ICollection<ClienteArtista> ArtistasQueAtienden { get; set; } = new List<ClienteArtista>();

        // Relación con calendarios personales
        public virtual ICollection<Calendario> CalendariosPersonales { get; set; } = new List<Calendario>();
    }
}
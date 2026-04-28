using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class Cita : BaseEntity
    {
        [Required]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        public DateTime FechaHoraFin { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "pendiente";

        [Required]
        [Range(0, 999999.99)]
        public decimal PrecioTotal { get; set; } = 0;

        [Required]
        [Range(0, 999999.99)]
        public decimal Adelanto { get; set; } = 0;

        public decimal? TamanioCm { get; set; }

        public string? NotasInternas { get; set; }

        public string? NotasPublicas { get; set; }

        [MaxLength(500)]
        public string? FotoReferenciaUrl { get; set; }

        public bool RequiereRecordatorio { get; set; } = true;

        public DateTime? FechaRecordatorioEnviado { get; set; }

        // Foreign keys
        public int UsuarioId { get; set; } // Cliente que agenda
        public int ArtistaReferenciaId { get; set; } // Artista que atiende
        public int? ZonaCuerpoId { get; set; }

        // Nuevo campo: Estudio donde se realiza la cita
        public int? EstudioId { get; set; }

        // Navigation properties
        public virtual Usuario Usuario { get; set; } = null!; // Cliente
        public virtual Usuario ArtistaReferencia { get; set; } = null!; // Artista
        public virtual ZonaCuerpo? ZonaCuerpo { get; set; }
        public virtual Estudio? Estudio { get; set; } // Estudio donde se realiza
        [MaxLength(200)]
        public string? GoogleEventIdArtista { get; set; }

        [MaxLength(200)]
        public string? GoogleEventIdEstudio { get; set; }
        // Colecciones
        public virtual ICollection<PagoParcial> PagosParciales { get; set; } = new List<PagoParcial>();
        public virtual ICollection<HistorialEstadoCita> HistorialEstados { get; set; } = new List<HistorialEstadoCita>();
    }
}
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
        public int ClienteId { get; set; }
        public int ArtistaId { get; set; }
        public int? AsistenteId { get; set; }
        public int? ZonaCuerpoId { get; set; }

        // Navigation properties
        public virtual Cliente Cliente { get; set; } = null!;
        public virtual Artista Artista { get; set; } = null!;
        public virtual Asistente? Asistente { get; set; }
        public virtual ZonaCuerpo? ZonaCuerpo { get; set; }
        public virtual ICollection<PagoParcial> PagosParciales { get; set; } = new List<PagoParcial>();
        public virtual ICollection<HistorialEstadoCita> HistorialEstados { get; set; } = new List<HistorialEstadoCita>();
    }
}
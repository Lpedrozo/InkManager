using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InkManager.Core.DTOs
{
    public class CrearCitaConImagenDto
    {
        public int UsuarioId { get; set; }
        public int ArtistaReferenciaId { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public decimal PrecioTotal { get; set; }
        public decimal Adelanto { get; set; }
        public int? ZonaCuerpoId { get; set; }
        public decimal? TamanioCm { get; set; }
        public string? NotasInternas { get; set; }
        public string? NotasPublicas { get; set; }
        public bool RequiereRecordatorio { get; set; }
        public IFormFile? FotoReferencia { get; set; }
    }
    public class CitaDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public int ArtistaReferenciaId { get; set; }
        public string ArtistaNombre { get; set; } = string.Empty;
        public int? AsistenteId { get; set; }
        public string? AsistenteNombre { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal PrecioTotal { get; set; }
        public decimal Adelanto { get; set; }
        public decimal SaldoPendiente => PrecioTotal - Adelanto - TotalPagado;
        public decimal TotalPagado { get; set; }
        public string? ZonaCuerpo { get; set; }
        public int? ZonaCuerpoId { get; set; }
        public decimal? TamanioCm { get; set; }
        public string? FotoReferenciaUrl { get; set; }
        public string? NotasInternas { get; set; }
        public string? NotasPublicas { get; set; }
        public bool RequiereRecordatorio { get; set; }
        public DateTime? FechaRecordatorioEnviado { get; set; }
        public TimeSpan Duracion => FechaHoraFin - FechaHoraInicio;
    }

    public class CrearCitaDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int ArtistaReferenciaId { get; set; }
        public IFormFile? FotoReferencia { get; set; }
        public int? AsistenteId { get; set; }

        [Required]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        public DateTime FechaHoraFin { get; set; }

        [Range(0, 999999.99)]
        public decimal PrecioTotal { get; set; }

        [Range(0, 999999.99)]
        public decimal Adelanto { get; set; } = 0;

        public int? ZonaCuerpoId { get; set; }

        [Range(0, 100)]
        public decimal? TamanioCm { get; set; }

        [MaxLength(500)]
        public string? NotasInternas { get; set; }

        [MaxLength(500)]
        public string? NotasPublicas { get; set; }

        [MaxLength(500)]
        public string? FotoReferenciaUrl { get; set; }

        public bool RequiereRecordatorio { get; set; } = true;
    }

    public class ActualizarCitaDto
    {
        public int? AsistenteId { get; set; }
        public DateTime? FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }
        public decimal? PrecioTotal { get; set; }
        public int? ZonaCuerpoId { get; set; }
        public decimal? TamanioCm { get; set; }
        public string? NotasInternas { get; set; }
        public string? NotasPublicas { get; set; }
        public string? FotoReferenciaUrl { get; set; }
        public bool? RequiereRecordatorio { get; set; }
    }

    public class CambiarEstadoDto
    {
        [Required]
        public string Estado { get; set; } = string.Empty;

        public string? Comentario { get; set; }
    }

    public class FiltroCitasDto
    {
        public int? ArtistaReferenciaId { get; set; }
        public int? EstudioId { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? UsuarioId { get; set; }
        public int Pagina { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? OrderBy { get; set; } = "FechaHoraInicio";
        public bool Descendente { get; set; } = true;
    }
}
namespace InkManager.Core.DTOs
{
    public class DashboardResumenDto
    {
        public int CitasHoy { get; set; }
        public int CitasSemana { get; set; }
        public int CitasPendientes { get; set; }
        public int CitasConfirmadas { get; set; }
        public int CitasEnCurso { get; set; }
        public int CitasCompletadasMes { get; set; }
        public decimal IngresosMes { get; set; }
        public decimal IngresosHoy { get; set; }
        public int NuevosClientesMes { get; set; }
        public int ArtistasActivos { get; set; }
        public List<ProximasCitasDto> ProximasCitas { get; set; } = new();
        public List<CitasPorEstadoDto> CitasPorEstado { get; set; } = new();
        public List<IngresosPorArtistaDto> IngresosPorArtista { get; set; } = new();
    }

    public class ProximasCitasDto
    {
        public int Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ArtistaNombre { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? ZonaCuerpo { get; set; }
    }

    public class CitasPorEstadoDto
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class IngresosPorArtistaDto
    {
        public int ArtistaId { get; set; }
        public string ArtistaNombre { get; set; } = string.Empty;
        public decimal TotalIngresos { get; set; }
        public int TotalCitas { get; set; }
    }
}
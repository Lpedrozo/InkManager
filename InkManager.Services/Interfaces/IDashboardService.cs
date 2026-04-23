using InkManager.Core.DTOs;

namespace InkManager.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResumenDto> GetResumenAsync(int? artistaId = null, int? estudioId = null);
        Task<List<ProximasCitasDto>> GetProximasCitasAsync(int? artistaId = null, int dias = 7);
        Task<byte[]> ExportarReporteCitasAsync(DateTime fechaInicio, DateTime fechaFin, int? artistaId = null);
    }
}
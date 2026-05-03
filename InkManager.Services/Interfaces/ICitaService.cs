using InkManager.Core.DTOs;
using InkManager.Core.DTOs.Common;
using Microsoft.AspNetCore.Http;

namespace InkManager.Services.Interfaces
{
    public interface ICitaService
    {
        // CRUD Básico
        Task<CitaDto?> UpdateAsync(int id, EditarCitaDto dto);
        Task<CitaDto?> ReprogramarAsync(int id, ReprogramarCitaDto dto);
        Task<CitaDto?> GetByIdAsync(int id);
        Task<CitaDto> CreateAsync(CrearCitaDto dto);
        Task<CitaDto?> UpdateAsync(int id, ActualizarCitaDto dto);
        Task<bool> DeleteAsync(int id);
        Task<string> GuardarImagenAsync(IFormFile imagen);
        // Gestión de estados
        Task<bool> CambiarEstadoAsync(int id, CambiarEstadoDto dto, string usuarioTipo, int usuarioId);
        Task<bool> CancelarCitaAsync(int id, string motivo, string usuarioTipo, int usuarioId);
        Task<bool> MarcarComoNoAsistioAsync(int id, string usuarioTipo, int usuarioId);
        Task<List<TimeSlotDto>> GetHorariosDisponiblesAsync(int artistaId, DateTime fecha);  // ← Agrega esta línea
        Task<List<UsuarioDto>> GetClientesAsync();
        // Pagos
        Task<decimal> RegistrarPagoAsync(int citaId, RegistrarPagoDto dto);
        Task<decimal> GetSaldoPendienteAsync(int citaId);
        Task<List<PagoParcialDto>> GetHistorialPagosAsync(int citaId);

        // Consultas y filtros
        Task<PagedResult<CitaDto>> GetCitasFiltradasAsync(FiltroCitasDto filtro);
        Task<List<CitaDto>> GetCitasDelDiaAsync(int artistaId, DateTime? fecha = null);
        Task<List<CitaDto>> GetCitasDelArtistaAsync(int artistaId, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<List<CitaDto>> GetCitasDelClienteAsync(int clienteId);

        // Métricas
        Task<int> GetCountByEstadoAsync(int artistaId, string estado);
        Task<Dictionary<string, int>> GetEstadisticasPorEstadoAsync(int artistaId);
        Task<decimal> GetIngresosPorArtistaAsync(int artistaId, DateTime? fechaInicio = null, DateTime? fechaFin = null);

        // Validaciones
        Task<bool> ValidarDisponibilidadAsync(int artistaId, DateTime inicio, DateTime fin, int? citaIdExcluir = null);
        Task<List<TimeSpan>> GetHorasOcupadasAsync(int artistaId, DateTime fecha);
    }
}
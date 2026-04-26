using InkManager.Core.DTOs;
using InkManager.Core.DTOs.Common;

namespace InkManager.Services.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDto?> GetByIdAsync(int id);
        Task<ClienteDto?> GetByTelefonoAsync(string telefono);
        Task<ClienteDto> CreateAsync(CrearClienteDto dto);
        Task<ClienteDto?> UpdateAsync(int id, ActualizarClienteDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<ClienteDto>> GetAllAsync(int pagina = 1, int pageSize = 10, string? search = null);
        Task<int> GetTotalClientesAsync();
        Task<List<ClienteDto>> GetClientesFrecuentesAsync(int top = 10);
        Task<List<ClienteDto>> GetClientesNuevosPorFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
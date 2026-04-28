using InkManager.Core.Entities;

namespace InkManager.Services.Interfaces
{
    public interface IGoogleCalendarService
    {
        // Autenticación
        Task<string> GetAuthUrlAsync(int calendarioId);
        Task<bool> HandleAuthCallbackAsync(int calendarioId, string code);
        Task<bool> RefreshAccessTokenAsync(int calendarioId);
        Task<bool> SyncCitaConGoogleAsync(int citaId);
        Task<bool> CreateEventAsync(Calendario calendario, Cita cita);
        Task<bool> UpdateEventAsync(Calendario calendario, Cita cita);
        Task<bool> DeleteEventAsync(Calendario calendario, string googleEventId);
        Task<bool> CreateCalendarIfNotExistsAsync(Calendario calendario);
        Task<bool> IsTokenValidAsync(Calendario calendario);
    }
}
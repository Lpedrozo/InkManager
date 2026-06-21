using InkManager.Core.Entities;

namespace InkManager.Services.Interfaces
{
    public interface INotificacionCitaService
    {
        Task<bool> EnviarNotificacionesCitasHoyAsync();
    }
}
namespace InkManager.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> EnviarNotificacionCitaAsync(string emailArtista, string nombreArtista, string clienteNombre, DateTime fecha, string zona, decimal precio);
        Task<bool> EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml);
    }
}
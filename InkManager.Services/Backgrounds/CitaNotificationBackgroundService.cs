using InkManager.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InkManager.Services.Implementations
{
    public class CitaNotificationBackgroundService : BackgroundService
    {
        private readonly ILogger<CitaNotificationBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CitaNotificationBackgroundService(
            ILogger<CitaNotificationBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de notificaciones de citas iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calcular el próximo tiempo de ejecución (6:00 AM)
                    var now = DateTime.Now;
                    var nextRun = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);

                    // Si ya pasaron las 6 AM, programar para mañana
                    if (now > nextRun)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    var tiempoEspera = nextRun - now;
                    _logger.LogInformation("Próxima ejecución programada para las {Hora}", nextRun.ToString("HH:mm"));

                    // Esperar hasta la próxima ejecución
                    await Task.Delay(tiempoEspera, stoppingToken);

                    // Ejecutar la tarea
                    await EjecutarNotificacionesAsync(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Servicio de notificaciones detenido.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el servicio de notificaciones de citas.");
                    // Esperar 5 minutos antes de reintentar si hay error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task EjecutarNotificacionesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando ejecución de notificaciones de citas.");

            using var scope = _serviceScopeFactory.CreateScope();
            var notificacionService = scope.ServiceProvider.GetRequiredService<INotificacionCitaService>();

            try
            {
                var resultado = await notificacionService.EnviarNotificacionesCitasHoyAsync();

                if (resultado)
                {
                    _logger.LogInformation("Notificaciones de citas enviadas exitosamente.");
                }
                else
                {
                    _logger.LogWarning("Se completaron las notificaciones con algunos errores.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando notificaciones de citas.");
            }
        }
    }
}
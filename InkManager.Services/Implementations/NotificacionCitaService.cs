using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InkManager.Services.Implementations
{
    public class NotificacionCitaService : INotificacionCitaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificacionCitaService> _logger;

        public NotificacionCitaService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<NotificacionCitaService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> EnviarNotificacionesCitasHoyAsync()
        {
            try
            {
                var fechaHoy = DateTime.Today;
                var fechaInicio = fechaHoy;
                var fechaFin = fechaHoy.AddDays(1);

                _logger.LogInformation("Iniciando envío de notificaciones para citas del día {Fecha}", fechaHoy);

                // Obtener todas las citas de hoy que no estén canceladas
                var citasHoy = await _context.Citas
                    .Include(c => c.Usuario)
                    .Include(c => c.ArtistaReferencia)
                    .Include(c => c.ZonaCuerpo)
                    .Include(c => c.Estudio)
                    .Where(c => c.FechaHoraInicio >= fechaInicio
                                && c.FechaHoraInicio < fechaFin
                                && c.Estado != "cancelada")
                    .ToListAsync();

                if (!citasHoy.Any())
                {
                    _logger.LogInformation("No hay citas programadas para hoy.");
                    return true;
                }

                _logger.LogInformation("Se encontraron {Count} citas para hoy.", citasHoy.Count);

                int exitos = 0;
                int fallos = 0;

                foreach (var cita in citasHoy)
                {
                    try
                    {
                        if (cita.ArtistaReferencia != null && !string.IsNullOrEmpty(cita.ArtistaReferencia.Email))
                        {
                            var enviado = await EnviarNotificacionCitaAsync(
                                cita,
                                cita.ArtistaReferencia.Email,
                                cita.ArtistaReferencia.Nombre
                            );

                            if (enviado)
                            {
                                exitos++;
                                _logger.LogInformation("Notificación enviada para cita {CitaId} - Artista: {Artista}",
                                    cita.Id, cita.ArtistaReferencia.Nombre);
                            }
                            else
                            {
                                fallos++;
                                _logger.LogWarning("No se pudo enviar notificación para cita {CitaId}", cita.Id);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("El artista de la cita {CitaId} no tiene email configurado.", cita.Id);
                            fallos++;
                        }
                    }
                    catch (Exception ex)
                    {
                        fallos++;
                        _logger.LogError(ex, "Error procesando cita {CitaId}", cita.Id);
                    }
                }

                _logger.LogInformation("Notificaciones completadas: {Exitos} exitosas, {Fallos} fallidas", exitos, fallos);
                return fallos == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en EnviarNotificacionesCitasHoyAsync");
                return false;
            }
        }

        private async Task<bool> EnviarNotificacionCitaAsync(Cita cita, string emailArtista, string nombreArtista)
        {
            try
            {
                var fechaFormateada = cita.FechaHoraInicio.ToString(
                    "dddd, dd 'de' MMMM 'de' yyyy 'a las' HH:mm",
                    new System.Globalization.CultureInfo("es-ES")
                );

                var horaInicio = cita.FechaHoraInicio.ToString("HH:mm");
                var horaFin = cita.FechaHoraFin.ToString("HH:mm");

                var cuerpoHtml = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #7c3aed 0%, #6d28d9 100%); color: white; padding: 30px; text-align: center; }}
                        .header h1 {{ margin: 0; font-size: 24px; }}
                        .alert-badge {{ background: #ef4444; color: white; padding: 5px 15px; border-radius: 20px; font-size: 14px; display: inline-block; margin-top: 10px; }}
                        .content {{ padding: 30px; }}
                        .info-card {{ background: #f8fafc; border-radius: 8px; padding: 15px; margin-bottom: 20px; }}
                        .info-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e2e8f0; }}
                        .info-row:last-child {{ border-bottom: none; }}
                        .label {{ font-weight: 600; color: #334155; }}
                        .value {{ color: #1e293b; }}
                        .footer {{ background: #f1f5f9; padding: 15px; text-align: center; font-size: 12px; color: #64748b; }}
                        .timer-card {{ background: #fff7ed; border-left: 4px solid #f97316; padding: 10px 15px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🖤 InkManager</h1>
                            <p>¡Recordatorio de cita para hoy!</p>
                            <div class='alert-badge'>📅 CITA DE HOY</div>
                        </div>
                        <div class='content'>
                            <p style='font-size: 16px;'>Hola <strong>{nombreArtista}</strong>,</p>
                            <p style='font-size: 18px;'>¡Tienes una cita programada para <strong>hoy</strong>!</p>
                            
                            <div class='timer-card'>
                                <strong>⏰ Recuerda:</strong> La cita es hoy a las {horaInicio}
                            </div>
                            
                            <div class='info-card'>
                                <div class='info-row'>
                                    <span class='label'>👤 Cliente:</span>
                                    <span class='value'><strong>{cita.Usuario?.Nombre ?? "No especificado"}</strong></span>
                                </div>
                                <div class='info-row'>
                                    <span class='label'>📅 Fecha:</span>
                                    <span class='value'>{fechaFormateada}</span>
                                </div>
                                <div class='info-row'>
                                    <span class='label'>⏰ Hora:</span>
                                    <span class='value'>{horaInicio} - {horaFin}</span>
                                </div>
                                <div class='info-row'>
                                    <span class='label'>📍 Zona del cuerpo:</span>
                                    <span class='value'>{cita.ZonaCuerpo?.Nombre ?? "No especificada"}</span>
                                </div>
                                <div class='info-row'>
                                    <span class='label'>💰 Precio:</span>
                                    <span class='value'><strong>${cita.PrecioTotal:F2}</strong></span>
                                </div>
                                {(cita.Estudio != null ? $@"
                                <div class='info-row'>
                                    <span class='label'>🏢 Estudio:</span>
                                    <span class='value'>{cita.Estudio.Nombre}</span>
                                </div>" : "")}
                            </div>
                            
                            <div style='background: #fef2f2; border-radius: 8px; padding: 15px; margin-top: 20px;'>
                                <p style='margin: 0; color: #991b1b;'>
                                    <strong>📋 Notas importantes:</strong>
                                </p>
                                <p style='margin: 5px 0 0 0; color: #7f1d1d;'>
                                    {cita.NotasInternas ?? "Sin notas adicionales"}
                                </p>
                            </div>

                            <p style='margin-top: 20px;'>
                                <strong>✅ Preparación:</strong>
                                <br>
                                • Verifica que tu equipo esté listo
                                <br>
                                • Confirma la disponibilidad del estudio
                                <br>
                                • Prepara los materiales necesarios
                            </p>
                        </div>
                        <div class='footer'>
                            <p>Este es un recordatorio automático de InkManager - Sistema de Gestión de Tatuajes</p>
                            <p>&copy; {DateTime.Now.Year} InkManager. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

                var asunto = $"🔔 RECORDATORIO: Cita de hoy - {cita.Usuario?.Nombre ?? "Cliente"}";

                return await _emailService.EnviarCorreoAsync(emailArtista, asunto, cuerpoHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación para cita {CitaId}", cita.Id);
                return false;
            }
        }
    }
}
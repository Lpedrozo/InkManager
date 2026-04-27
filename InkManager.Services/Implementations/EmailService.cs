using System.Net;
using System.Net.Mail;
using InkManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InkManager.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> EnviarNotificacionCitaAsync(string emailArtista, string nombreArtista, string clienteNombre, DateTime fecha, string zona, decimal precio)
        {
            var fechaFormateada = fecha.ToString("dddd, dd MMMM yyyy 'a las' HH:mm", new System.Globalization.CultureInfo("es-ES"));

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
                    .content {{ padding: 30px; }}
                    .info-card {{ background: #f8fafc; border-radius: 8px; padding: 15px; margin-bottom: 20px; }}
                    .info-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e2e8f0; }}
                    .info-row:last-child {{ border-bottom: none; }}
                    .label {{ font-weight: 600; color: #334155; }}
                    .value {{ color: #1e293b; }}
                    .footer {{ background: #f1f5f9; padding: 15px; text-align: center; font-size: 12px; color: #64748b; }}
                    .badge {{ display: inline-block; background: #22c55e; color: white; padding: 4px 12px; border-radius: 20px; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🖤 InkManager</h1>
                        <p>Nueva cita agendada</p>
                    </div>
                    <div class='content'>
                        <p style='font-size: 16px;'>Hola <strong>{nombreArtista}</strong>,</p>
                        <p>Se ha agendado una nueva cita en tu agenda. Aquí están los detalles:</p>
                        
                        <div class='info-card'>
                            <div class='info-row'>
                                <span class='label'>👤 Cliente:</span>
                                <span class='value'>{clienteNombre}</span>
                            </div>
                            <div class='info-row'>
                                <span class='label'>📅 Fecha y Hora:</span>
                                <span class='value'>{fechaFormateada}</span>
                            </div>
                            <div class='info-row'>
                                <span class='label'>📍 Zona del cuerpo:</span>
                                <span class='value'>{zona}</span>
                            </div>
                            <div class='info-row'>
                                <span class='label'>💰 Precio:</span>
                                <span class='value'><strong>${precio:F2}</strong></span>
                            </div>
                        </div>
                        
                        <p style='margin-top: 20px;'>Por favor, revisa tu calendario para confirmar la disponibilidad.</p>
                        <p>Si tienes alguna duda, contacta con el cliente lo antes posible.</p>
                    </div>
                    <div class='footer'>
                        <p>Este es un mensaje automático de InkManager - Sistema de Gestión de Tatuajes</p>
                        <p>&copy; {DateTime.Now.Year} InkManager. Todos los derechos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";

            return await EnviarCorreoAsync(emailArtista, "📅 Nueva cita agendada - InkManager", cuerpoHtml);
        }

        public async Task<bool> EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"] ?? "";
                var smtpPass = _configuration["Email:SmtpPass"] ?? "";
                var emailFrom = _configuration["Email:From"] ?? "notificaciones@inkmanager.com";

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    Console.WriteLine("Configuración de email no disponible - simulando envío");
                    Console.WriteLine($"📧 Email simulado para: {destinatario}");
                    Console.WriteLine($"📧 Asunto: {asunto}");
                    return true;
                }

                using var client = new SmtpClient(smtpServer, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                var mensaje = new MailMessage
                {
                    From = new MailAddress(emailFrom, "InkManager"),
                    Subject = asunto,
                    Body = cuerpoHtml,
                    IsBodyHtml = true
                };
                mensaje.To.Add(destinatario);

                await client.SendMailAsync(mensaje);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando email: {ex.Message}");
                return false;
            }
        }
    }
}
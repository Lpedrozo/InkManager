using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using InkManager.Core.Entities;
using InkManager.Core.DTOs;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InkManager.Services.Implementations
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GoogleCalendarService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        private string ClientId => _configuration["GoogleCalendar:ClientId"] ?? "";
        private string ClientSecret => _configuration["GoogleCalendar:ClientSecret"] ?? "";
        private string RedirectUri => _configuration["GoogleCalendar:RedirectUri"] ?? "";

        public async Task<string> GetAuthUrlAsync(int calendarioId)
        {
            var state = $"{calendarioId}_{Guid.NewGuid()}";
            var url = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                      $"client_id={Uri.EscapeDataString(ClientId)}&" +
                      $"redirect_uri={Uri.EscapeDataString(RedirectUri)}&" +
                      $"response_type=code&" +
                      $"scope={Uri.EscapeDataString("https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/calendar.events")}&" +
                      $"access_type=offline&" +
                      $"prompt=consent&" +
                      $"state={state}";
            return url;
        }

        public async Task<bool> HandleAuthCallbackAsync(int calendarioId, string code)
        {
            try
            {
                var tokenResponse = await ExchangeCodeForTokensAsync(code);
                if (tokenResponse == null) return false;

                var calendario = await _context.Calendarios.FindAsync(calendarioId);
                if (calendario == null) return false;

                calendario.AccessToken = tokenResponse.AccessToken;
                calendario.RefreshToken = tokenResponse.RefreshToken;
                calendario.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                calendario.IsSynced = true;
                calendario.LastSync = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Crear o vincular calendario en Google
                await CreateCalendarIfNotExistsAsync(calendario);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en HandleAuthCallback: {ex.Message}");
                return false;
            }
        }

        private async Task<GoogleAuthResponse?> ExchangeCodeForTokensAsync(string code)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<GoogleAuthResponse>(json);
        }

        public async Task<bool> RefreshAccessTokenAsync(int calendarioId)
        {
            var calendario = await _context.Calendarios.FindAsync(calendarioId);
            if (calendario?.RefreshToken == null) return false;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("refresh_token", calendario.RefreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            });

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleAuthResponse>(json);

            if (tokenResponse?.AccessToken != null)
            {
                calendario.AccessToken = tokenResponse.AccessToken;
                calendario.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> IsTokenValidAsync(Calendario calendario)
        {
            if (calendario?.AccessToken == null) return false;
            if (calendario.TokenExpiry > DateTime.UtcNow.AddMinutes(5)) return true;
            return await RefreshAccessTokenAsync(calendario.Id);
        }

        private CalendarService GetCalendarService(Calendario calendario)
        {
            var credential = GoogleCredential.FromAccessToken(calendario.AccessToken);

            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "InkManager"
            });
        }

        public async Task<bool> CreateCalendarIfNotExistsAsync(Calendario calendario)
        {
            if (!await IsTokenValidAsync(calendario)) return false;

            try
            {
                var service = GetCalendarService(calendario);

                // Si ya tiene GoogleCalendarId, ya existe
                if (!string.IsNullOrEmpty(calendario.GoogleCalendarId))
                {
                    return true;
                }

                var calendar = new Google.Apis.Calendar.v3.Data.Calendar
                {
                    Summary = $"InkManager - {calendario.Nombre}",
                    Description = $"Calendario sincronizado con InkManager - {(calendario.Tipo == "artista" ? "Calendario personal del artista" : "Calendario del estudio")}",
                    TimeZone = "America/Mexico_City"
                };

                var createdCalendar = await service.Calendars.Insert(calendar).ExecuteAsync();
                calendario.GoogleCalendarId = createdCalendar.Id;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando calendario: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SyncCitaConGoogleAsync(int citaId)
        {
            var cita = await _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .Include(c => c.ZonaCuerpo)
                .Include(c => c.Estudio)
                .FirstOrDefaultAsync(c => c.Id == citaId);

            if (cita == null) return false;

            var resultado = true;

            // 1. Sincronizar con calendario del artista
            var calendarioArtista = await _context.Calendarios
                .FirstOrDefaultAsync(c => c.Tipo == "artista" && c.EntidadId == cita.ArtistaReferenciaId && c.IsSynced);

            if (calendarioArtista != null)
            {
                if (string.IsNullOrEmpty(cita.GoogleEventIdArtista))
                {
                    resultado &= await CreateEventAsync(calendarioArtista, cita);
                }
                else
                {
                    resultado &= await UpdateEventAsync(calendarioArtista, cita);
                }
            }

            // 2. Sincronizar con calendario del estudio
            if (cita.EstudioId.HasValue)
            {
                var calendarioEstudio = await _context.Calendarios
                    .FirstOrDefaultAsync(c => c.Tipo == "estudio" && c.EntidadId == cita.EstudioId.Value && c.IsSynced);

                if (calendarioEstudio != null)
                {
                    if (string.IsNullOrEmpty(cita.GoogleEventIdEstudio))
                    {
                        resultado &= await CreateEventAsync(calendarioEstudio, cita);
                    }
                    else
                    {
                        resultado &= await UpdateEventAsync(calendarioEstudio, cita);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return resultado;
        }

        public async Task<bool> CreateEventAsync(Calendario calendario, Cita cita)
        {
            if (!await IsTokenValidAsync(calendario)) return false;

            try
            {
                var service = GetCalendarService(calendario);

                var titulo = calendario.Tipo == "artista"
                    ? $"Cita con {cita.Usuario?.Nombre}"
                    : $"Cita - {cita.ArtistaReferencia?.Nombre} con {cita.Usuario?.Nombre}";

                var descripcion = $"""
                    Cliente: {cita.Usuario?.Nombre}
                    Teléfono: {cita.Usuario?.Telefono}
                    Email: {cita.Usuario?.Email}
                    Zona del cuerpo: {cita.ZonaCuerpo?.Nombre ?? "No especificada"}
                    Tamaño: {(cita.TamanioCm.HasValue ? $"{cita.TamanioCm} cm" : "No especificado")}
                    Precio: ${cita.PrecioTotal:F2}
                    Adelanto: ${cita.Adelanto:F2}
                    
                    Notas: {cita.NotasInternas ?? "Sin notas"}
                    """;

                var googleEvent = new Event
                {
                    Summary = titulo,
                    Description = descripcion,
                    Start = new EventDateTime
                    {
                        DateTime = cita.FechaHoraInicio,
                        TimeZone = "America/Mexico_City"
                    },
                    End = new EventDateTime
                    {
                        DateTime = cita.FechaHoraFin,
                        TimeZone = "America/Mexico_City"
                    },
                    ColorId = GetColorIdByEstado(cita.Estado)
                };

                var createdEvent = await service.Events.Insert(googleEvent, calendario.GoogleCalendarId).ExecuteAsync();

                if (calendario.Tipo == "artista")
                    cita.GoogleEventIdArtista = createdEvent.Id;
                else
                    cita.GoogleEventIdEstudio = createdEvent.Id;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando evento: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateEventAsync(Calendario calendario, Cita cita)
        {
            var googleEventId = calendario.Tipo == "artista" ? cita.GoogleEventIdArtista : cita.GoogleEventIdEstudio;
            if (string.IsNullOrEmpty(googleEventId)) return false;
            if (!await IsTokenValidAsync(calendario)) return false;

            try
            {
                var service = GetCalendarService(calendario);

                var titulo = calendario.Tipo == "artista"
                    ? $"Cita con {cita.Usuario?.Nombre}"
                    : $"Cita - {cita.ArtistaReferencia?.Nombre} con {cita.Usuario?.Nombre}";

                var descripcion = $"""
                    Cliente: {cita.Usuario?.Nombre}
                    Teléfono: {cita.Usuario?.Telefono}
                    Email: {cita.Usuario?.Email}
                    Zona del cuerpo: {cita.ZonaCuerpo?.Nombre ?? "No especificada"}
                    Tamaño: {(cita.TamanioCm.HasValue ? $"{cita.TamanioCm} cm" : "No especificado")}
                    Precio: ${cita.PrecioTotal:F2}
                    Adelanto: ${cita.Adelanto:F2}
                    Estado: {cita.Estado}
                    
                    Notas: {cita.NotasInternas ?? "Sin notas"}
                    """;

                var googleEvent = new Event
                {
                    Summary = titulo,
                    Description = descripcion,
                    Start = new EventDateTime
                    {
                        DateTime = cita.FechaHoraInicio,
                        TimeZone = "America/Mexico_City"
                    },
                    End = new EventDateTime
                    {
                        DateTime = cita.FechaHoraFin,
                        TimeZone = "America/Mexico_City"
                    },
                    ColorId = GetColorIdByEstado(cita.Estado)
                };

                await service.Events.Update(googleEvent, calendario.GoogleCalendarId, googleEventId).ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando evento: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteEventAsync(Calendario calendario, string googleEventId)
        {
            if (string.IsNullOrEmpty(googleEventId)) return false;
            if (!await IsTokenValidAsync(calendario)) return false;

            try
            {
                var service = GetCalendarService(calendario);
                await service.Events.Delete(calendario.GoogleCalendarId, googleEventId).ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando evento: {ex.Message}");
                return false;
            }
        }

        private string GetColorIdByEstado(string estado)
        {
            return estado switch
            {
                "pendiente" => "5",   // Amarillo
                "confirmada" => "2",  // Verde
                "en_curso" => "4",    // Morado
                "completada" => "3",  // Azul
                "cancelada" => "11",  // Rojo
                _ => "1"              // Gris
            };
        }
    }
}
using InkManager.Core.DTOs;
using InkManager.Core.DTOs.Common;
using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace InkManager.Services.Implementations
{
    public class CitaService : ICitaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CitaService(ApplicationDbContext context, IEmailService emailService, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetArtistaIdActual()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return 0;

            var rol = user.FindFirst(ClaimTypes.Role)?.Value;
            var usuarioId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (rol == "artista")
            {
                return usuarioId;
            }
            else if (rol == "asistente")
            {
                var artistaId = user.FindFirst("ArtistaId")?.Value;
                return artistaId != null ? int.Parse(artistaId) : 0;
            }

            return 0;
        }

        // Obtener el ID del estudio actual (de la sesión)
        private int GetEstudioIdActual()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return 0;

            var estudioId = user.FindFirst("EstudioId")?.Value;
            return estudioId != null ? int.Parse(estudioId) : 0;
        }

        public async Task<List<UsuarioDto>> GetClientesAsync()
        {
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0) return new List<UsuarioDto>();

            // Obtener solo clientes asociados a este artista
            var clientes = await _context.ClientesArtistas
                .Include(ca => ca.Cliente)
                .Where(ca => ca.ArtistaId == artistaId)
                .Select(ca => new UsuarioDto
                {
                    Id = ca.Cliente.Id,
                    Nombre = ca.Cliente.Nombre,
                    Email = ca.Cliente.Email,
                    Telefono = ca.Cliente.Telefono
                })
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return clientes;
        }

        public async Task<List<TimeSlotDto>> GetHorariosDisponiblesAsync(int artistaId, DateTime fecha)
        {
            var estudioId = GetEstudioIdActual();

            // Obtener horario laboral del artista en este estudio
            var estudioUsuario = await _context.EstudioUsuarios
                .FirstOrDefaultAsync(eu => eu.UsuarioId == artistaId && eu.EstudioId == estudioId);

            // Parsear horario laboral (formato JSON: {"lunes":"9-18","martes":"9-18",...})
            var diaSemana = fecha.DayOfWeek.ToString().ToLower();
            var horarioLaboral = estudioUsuario?.HorarioLaboral;
            var rangoHorario = "9-20"; // Default

            if (!string.IsNullOrEmpty(horarioLaboral))
            {
                try
                {
                    var horariosJson = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(horarioLaboral);
                    if (horariosJson != null && horariosJson.ContainsKey(diaSemana))
                    {
                        rangoHorario = horariosJson[diaSemana];
                    }
                }
                catch { }
            }

            var partes = rangoHorario.Split('-');
            var horaInicio = TimeSpan.Parse(partes[0]);
            var horaFin = TimeSpan.Parse(partes[1]);
            var duracion = TimeSpan.FromHours(1);

            var citasDelDia = await _context.Citas
                .Where(c => c.ArtistaReferenciaId == artistaId
                    && c.FechaHoraInicio.Date == fecha.Date
                    && c.Estado != "cancelada"
                    && !c.EliminadoLogico)
                .Select(c => new { c.FechaHoraInicio, c.FechaHoraFin })
                .ToListAsync();

            var horariosDisponibles = new List<TimeSlotDto>();

            for (var hora = horaInicio; hora < horaFin; hora = hora.Add(duracion))
            {
                var slotInicio = fecha.Date.Add(hora);
                var slotFin = slotInicio.Add(duracion);

                var ocupado = citasDelDia.Any(c =>
                    (slotInicio >= c.FechaHoraInicio && slotInicio < c.FechaHoraFin) ||
                    (slotFin > c.FechaHoraInicio && slotFin <= c.FechaHoraFin) ||
                    (slotInicio <= c.FechaHoraInicio && slotFin >= c.FechaHoraFin));

                if (!ocupado)
                {
                    horariosDisponibles.Add(new TimeSlotDto
                    {
                        HoraInicio = hora,
                        HoraFin = hora.Add(duracion),
                        Disponible = true
                    });
                }
            }

            return horariosDisponibles;
        }

        public async Task<CitaDto> CreateAsync(CrearCitaDto dto)
        {
            var artistaId = GetArtistaIdActual();
            var estudioId = dto.EstudioId ?? GetEstudioIdActual(); // Usar el del DTO o el de la sesión

            // Validar que el artista pertenece al estudio seleccionado
            var pertenece = await _context.EstudioUsuarios
                .AnyAsync(eu => eu.UsuarioId == artistaId && eu.EstudioId == estudioId);

            if (!pertenece)
                throw new InvalidOperationException("El artista no pertenece a este estudio");

            // Validar disponibilidad del artista
            var disponible = await ValidarDisponibilidadAsync(artistaId, dto.FechaHoraInicio, dto.FechaHoraFin);
            if (!disponible)
                throw new InvalidOperationException("El artista no está disponible en ese horario");

            // Guardar imagen si existe
            string fotoUrl = null;
            if (dto.FotoReferencia != null)
            {
                fotoUrl = await GuardarImagenAsync(dto.FotoReferencia);
            }

            var cita = new Cita
            {
                UsuarioId = dto.UsuarioId,
                ArtistaReferenciaId = artistaId,
                EstudioId = estudioId, 
                FechaHoraInicio = dto.FechaHoraInicio,
                FechaHoraFin = dto.FechaHoraFin,
                PrecioTotal = dto.PrecioTotal,
                Adelanto = dto.Adelanto,
                ZonaCuerpoId = dto.ZonaCuerpoId,
                TamanioCm = dto.TamanioCm,
                NotasInternas = dto.NotasInternas,
                NotasPublicas = dto.NotasPublicas,
                FotoReferenciaUrl = fotoUrl ?? dto.FotoReferenciaUrl,
                RequiereRecordatorio = dto.RequiereRecordatorio,
                Estado = "pendiente"
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            await RegistrarHistorialAsync(cita.Id, null, "pendiente", "sistema", 0, "Cita creada");

            if (dto.Adelanto > 0)
            {
                await RegistrarPagoAsync(cita.Id, new RegistrarPagoDto
                {
                    Monto = dto.Adelanto,
                    MetodoPago = "efectivo",
                    Nota = "Adelanto inicial"
                });
            }

            await EnviarNotificacionArtistaAsync(cita.Id);

            // Sincronizar con Google Calendar
            try
            {
                var googleCalendarService = _httpContextAccessor.HttpContext?.RequestServices
                    .GetRequiredService<IGoogleCalendarService>();

                if (googleCalendarService != null)
                {
                    await googleCalendarService.SyncCitaConGoogleAsync(cita.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sincronizando con Google Calendar: {ex.Message}");
            }

            return (await GetByIdAsync(cita.Id))!;
        }
        private async Task EnviarNotificacionArtistaAsync(int citaId)
        {
            try
            {
                var cita = await _context.Citas
                    .Include(c => c.Usuario)
                    .Include(c => c.ArtistaReferencia)
                    .Include(c => c.ZonaCuerpo)
                    .FirstOrDefaultAsync(c => c.Id == citaId);

                if (cita == null) return;

                var emailArtista = cita.ArtistaReferencia?.Email;
                if (string.IsNullOrEmpty(emailArtista)) return;

                await _emailService.EnviarNotificacionCitaAsync(
                    emailArtista,
                    cita.ArtistaReferencia.Nombre,
                    cita.Usuario.Nombre,
                    cita.FechaHoraInicio,
                    cita.ZonaCuerpo?.Nombre ?? "No especificada",
                    cita.PrecioTotal
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando notificación: {ex.Message}");
            }
        }
        public async Task<string> GuardarImagenAsync(IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "referencias");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imagen.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imagen.CopyToAsync(fileStream);
            }

            return $"/uploads/referencias/{uniqueFileName}";
        }
        public async Task<CitaDto?> GetByIdAsync(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Usuario)  // Cliente
                .Include(c => c.ArtistaReferencia)  // Artista
                .Include(c => c.ZonaCuerpo)
                .Include(c => c.PagosParciales)
                .FirstOrDefaultAsync(c => c.Id == id && !c.EliminadoLogico);

            if (cita == null) return null;

            return MapToDto(cita);
        }
        // Método para obtener clientes disponibles
        public async Task<CitaDto?> UpdateAsync(int id, ActualizarCitaDto dto)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null || cita.EliminadoLogico) return null;

            // Si cambia el horario, validar disponibilidad
            if (dto.FechaHoraInicio.HasValue && dto.FechaHoraFin.HasValue)
            {
                var disponible = await ValidarDisponibilidadAsync(cita.ArtistaReferenciaId, dto.FechaHoraInicio.Value, dto.FechaHoraFin.Value, id);
                if (!disponible)
                    throw new InvalidOperationException("El artista no está disponible en ese horario");

                cita.FechaHoraInicio = dto.FechaHoraInicio.Value;
                cita.FechaHoraFin = dto.FechaHoraFin.Value;
            }

            // Actualizar campos
            if (dto.PrecioTotal.HasValue) cita.PrecioTotal = dto.PrecioTotal.Value;
            if (dto.ZonaCuerpoId.HasValue) cita.ZonaCuerpoId = dto.ZonaCuerpoId;
            if (dto.TamanioCm.HasValue) cita.TamanioCm = dto.TamanioCm;
            if (dto.NotasInternas != null) cita.NotasInternas = dto.NotasInternas;
            if (dto.NotasPublicas != null) cita.NotasPublicas = dto.NotasPublicas;
            if (dto.FotoReferenciaUrl != null) cita.FotoReferenciaUrl = dto.FotoReferenciaUrl;
            if (dto.RequiereRecordatorio.HasValue) cita.RequiereRecordatorio = dto.RequiereRecordatorio.Value;

            cita.FechaModificacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return false;

            cita.EliminadoLogico = true;
            cita.FechaModificacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CambiarEstadoAsync(int id, CambiarEstadoDto dto, string usuarioTipo, int usuarioId)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null || cita.EliminadoLogico) return false;

            var estadoAnterior = cita.Estado;
            cita.Estado = dto.Estado;
            cita.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await RegistrarHistorialAsync(id, estadoAnterior, dto.Estado, usuarioTipo, usuarioId, dto.Comentario);

            return true;
        }

        public async Task<bool> CancelarCitaAsync(int id, string motivo, string usuarioTipo, int usuarioId)
        {
            return await CambiarEstadoAsync(id, new CambiarEstadoDto
            {
                Estado = "cancelada",
                Comentario = $"Cancelada: {motivo}"
            }, usuarioTipo, usuarioId);
        }

        public async Task<bool> MarcarComoNoAsistioAsync(int id, string usuarioTipo, int usuarioId)
        {
            return await CambiarEstadoAsync(id, new CambiarEstadoDto
            {
                Estado = "no_asistio",
                Comentario = "Cliente no asistió a la cita"
            }, usuarioTipo, usuarioId);
        }

        public async Task<decimal> RegistrarPagoAsync(int citaId, RegistrarPagoDto dto)
        {
            var cita = await _context.Citas.FindAsync(citaId);
            if (cita == null) throw new Exception("Cita no encontrada");

            var pago = new PagoParcial
            {
                CitaId = citaId,
                Monto = dto.Monto,
                MetodoPago = dto.MetodoPago,
                ReferenciaPago = dto.ReferenciaPago,
                Nota = dto.Nota,
                FechaPago = DateTime.UtcNow
            };

            _context.PagosParciales.Add(pago);
            await _context.SaveChangesAsync();

            return await GetSaldoPendienteAsync(citaId);
        }

        public async Task<decimal> GetSaldoPendienteAsync(int citaId)
        {
            var cita = await _context.Citas
                .Include(c => c.PagosParciales)
                .FirstOrDefaultAsync(c => c.Id == citaId);

            if (cita == null) return 0;

            var totalPagado = cita.Adelanto + cita.PagosParciales.Sum(p => p.Monto);
            return cita.PrecioTotal - totalPagado;
        }

        public async Task<List<PagoParcialDto>> GetHistorialPagosAsync(int citaId)
        {
            var pagos = await _context.PagosParciales
                .Where(p => p.CitaId == citaId && !p.EliminadoLogico)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            return pagos.Select(p => new PagoParcialDto
            {
                Id = p.Id,
                CitaId = p.CitaId,
                Monto = p.Monto,
                FechaPago = p.FechaPago,
                MetodoPago = p.MetodoPago,
                ReferenciaPago = p.ReferenciaPago,
                Nota = p.Nota
            }).ToList();
        }

        public async Task<PagedResult<CitaDto>> GetCitasFiltradasAsync(FiltroCitasDto filtro)
        {
            var query = _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .ThenInclude(a => a.EstudioUsuarios)  // Agregar esto para acceder a estudios
                .Include(c => c.ZonaCuerpo)
                .Where(c => !c.EliminadoLogico);

            // Aplicar filtros
            if (filtro.ArtistaReferenciaId.HasValue)
                query = query.Where(c => c.ArtistaReferenciaId == filtro.ArtistaReferenciaId.Value);

            // CORREGIDO: Filtrar por estudio usando la tabla puente
            if (filtro.EstudioId.HasValue)
                query = query.Where(c => c.ArtistaReferencia!.EstudioUsuarios.Any(eu => eu.EstudioId == filtro.EstudioId.Value));

            if (!string.IsNullOrEmpty(filtro.Estado))
                query = query.Where(c => c.Estado == filtro.Estado);

            if (filtro.UsuarioId.HasValue)
                query = query.Where(c => c.UsuarioId == filtro.UsuarioId.Value);

            if (filtro.FechaInicio.HasValue)
                query = query.Where(c => c.FechaHoraInicio >= filtro.FechaInicio.Value);

            if (filtro.FechaFin.HasValue)
                query = query.Where(c => c.FechaHoraFin <= filtro.FechaFin.Value);

            // Ordenar
            query = filtro.Descendente
                ? query.OrderByDescending(c => EF.Property<object>(c, filtro.OrderBy ?? "FechaHoraInicio"))
                : query.OrderBy(c => EF.Property<object>(c, filtro.OrderBy ?? "FechaHoraInicio"));

            // Paginar
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filtro.Pagina - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToListAsync();

            return new PagedResult<CitaDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = filtro.Pagina,
                PageSize = filtro.PageSize
            };
        }

        public async Task<List<CitaDto>> GetCitasDelDiaAsync(int artistaId, DateTime? fecha = null)
        {
            var fechaConsulta = fecha?.Date ?? DateTime.UtcNow.Date;

            var citas = await _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .Include(c => c.ZonaCuerpo)
                .Where(c => c.ArtistaReferenciaId == artistaId
                    && c.FechaHoraInicio.Date == fechaConsulta
                    && !c.EliminadoLogico
                    && c.Estado != "cancelada")
                .OrderBy(c => c.FechaHoraInicio)
                .ToListAsync();

            return citas.Select(MapToDto).ToList();
        }

        public async Task<List<CitaDto>> GetCitasDelArtistaAsync(int artistaId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .Include(c => c.ZonaCuerpo)
                .Where(c => c.ArtistaReferenciaId == artistaId && !c.EliminadoLogico);

            if (fechaInicio.HasValue)
                query = query.Where(c => c.FechaHoraInicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(c => c.FechaHoraFin <= fechaFin.Value);

            var citas = await query
                .OrderBy(c => c.FechaHoraInicio)
                .ToListAsync();

            return citas.Select(MapToDto).ToList();
        }

        public async Task<List<CitaDto>> GetCitasDelClienteAsync(int clienteId)
        {
            var citas = await _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .Include(c => c.ZonaCuerpo)
                .Where(c => c.UsuarioId == clienteId && !c.EliminadoLogico)
                .OrderByDescending(c => c.FechaHoraInicio)
                .ToListAsync();

            return citas.Select(MapToDto).ToList();
        }

        public async Task<int> GetCountByEstadoAsync(int artistaId, string estado)
        {
            return await _context.Citas
                .CountAsync(c => c.ArtistaReferenciaId == artistaId && c.Estado == estado && !c.EliminadoLogico);
        }

        public async Task<Dictionary<string, int>> GetEstadisticasPorEstadoAsync(int artistaId)
        {
            var estados = new[] { "pendiente", "confirmada", "en_curso", "completada", "cancelada", "no_asistio" };
            var resultado = new Dictionary<string, int>();

            foreach (var estado in estados)
            {
                var count = await GetCountByEstadoAsync(artistaId, estado);
                resultado[estado] = count;
            }

            return resultado;
        }

        public async Task<decimal> GetIngresosPorArtistaAsync(int artistaId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = _context.Citas
                .Where(c => c.ArtistaReferenciaId == artistaId && c.Estado == "completada" && !c.EliminadoLogico);

            if (fechaInicio.HasValue)
                query = query.Where(c => c.FechaHoraInicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(c => c.FechaHoraFin <= fechaFin.Value);

            return await query.SumAsync(c => c.PrecioTotal);
        }

        public async Task<bool> ValidarDisponibilidadAsync(int artistaId, DateTime inicio, DateTime fin, int? citaIdExcluir = null)
        {
            var query = _context.Citas
                .Where(c => c.ArtistaReferenciaId == artistaId
                    && !c.EliminadoLogico
                    && c.Estado != "cancelada"
                    && ((inicio >= c.FechaHoraInicio && inicio < c.FechaHoraFin)
                        || (fin > c.FechaHoraInicio && fin <= c.FechaHoraFin)
                        || (inicio <= c.FechaHoraInicio && fin >= c.FechaHoraFin)));

            if (citaIdExcluir.HasValue)
                query = query.Where(c => c.Id != citaIdExcluir.Value);

            return !await query.AnyAsync();
        }

        public async Task<List<TimeSpan>> GetHorasOcupadasAsync(int artistaId, DateTime fecha)
        {
            var citas = await _context.Citas
                .Where(c => c.ArtistaReferenciaId == artistaId
                    && c.FechaHoraInicio.Date == fecha.Date
                    && !c.EliminadoLogico
                    && c.Estado != "cancelada")
                .ToListAsync();

            var horasOcupadas = new List<TimeSpan>();
            foreach (var cita in citas)
            {
                var horaInicio = cita.FechaHoraInicio.TimeOfDay;
                var horaFin = cita.FechaHoraFin.TimeOfDay;

                for (var hora = horaInicio; hora < horaFin; hora = hora.Add(TimeSpan.FromHours(1)))
                {
                    horasOcupadas.Add(hora);
                }
            }

            return horasOcupadas.Distinct().OrderBy(h => h).ToList();
        }
        public async Task<CitaDto?> UpdateAsync(int id, EditarCitaDto dto)
        {
            var cita = await _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .Include(c => c.ZonaCuerpo)
                .FirstOrDefaultAsync(c => c.Id == id && !c.EliminadoLogico);

            if (cita == null) return null;

            // Si cambia el horario, validar disponibilidad
            if (dto.FechaHoraInicio.HasValue && dto.FechaHoraFin.HasValue)
            {
                var disponible = await ValidarDisponibilidadAsync(cita.ArtistaReferenciaId,
                    dto.FechaHoraInicio.Value, dto.FechaHoraFin.Value, id);
                if (!disponible)
                    throw new InvalidOperationException("El artista no está disponible en ese horario");

                cita.FechaHoraInicio = dto.FechaHoraInicio.Value;
                cita.FechaHoraFin = dto.FechaHoraFin.Value;
            }

            // Actualizar campos
            if (dto.PrecioTotal.HasValue) cita.PrecioTotal = dto.PrecioTotal.Value;
            if (dto.Adelanto.HasValue) cita.Adelanto = dto.Adelanto.Value;
            if (dto.ZonaCuerpoId.HasValue) cita.ZonaCuerpoId = dto.ZonaCuerpoId;
            if (dto.TamanioCm.HasValue) cita.TamanioCm = dto.TamanioCm;
            if (dto.NotasInternas != null) cita.NotasInternas = dto.NotasInternas;
            if (dto.NotasPublicas != null) cita.NotasPublicas = dto.NotasPublicas;
            if (dto.RequiereRecordatorio.HasValue) cita.RequiereRecordatorio = dto.RequiereRecordatorio.Value;

            cita.FechaModificacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Registrar en historial
            await RegistrarHistorialAsync(cita.Id, cita.Estado, cita.Estado, "sistema", 0, "Cita actualizada");

            // Sincronizar con Google Calendar
            await SincronizarGoogleCalendar(cita.Id);

            return await GetByIdAsync(id);
        }

        public async Task<CitaDto?> ReprogramarAsync(int id, ReprogramarCitaDto dto)
        {
            var cita = await _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .FirstOrDefaultAsync(c => c.Id == id && !c.EliminadoLogico);

            if (cita == null) return null;

            // Validar disponibilidad
            var disponible = await ValidarDisponibilidadAsync(cita.ArtistaReferenciaId,
                dto.FechaHoraInicio, dto.FechaHoraFin, id);
            if (!disponible)
                throw new InvalidOperationException("El artista no está disponible en ese horario");

            var fechaAnterior = cita.FechaHoraInicio;
            var fechaAnteriorFin = cita.FechaHoraFin;

            cita.FechaHoraInicio = dto.FechaHoraInicio;
            cita.FechaHoraFin = dto.FechaHoraFin;
            cita.FechaModificacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Registrar en historial
            await RegistrarHistorialAsync(cita.Id, cita.Estado, cita.Estado, "sistema", 0,
                $"Cita reprogramada de {fechaAnterior:dd/MM/yyyy HH:mm} a {dto.FechaHoraInicio:dd/MM/yyyy HH:mm}");

            // Sincronizar con Google Calendar
            await SincronizarGoogleCalendar(cita.Id);

            return await GetByIdAsync(id);
        }

        private async Task SincronizarGoogleCalendar(int citaId)
        {
            try
            {
                var googleCalendarService = _httpContextAccessor.HttpContext?.RequestServices
                    .GetRequiredService<IGoogleCalendarService>();

                if (googleCalendarService != null)
                {
                    await googleCalendarService.SyncCitaConGoogleAsync(citaId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sincronizando con Google Calendar: {ex.Message}");
            }
        }
        // Métodos privados auxiliares
        private async Task RegistrarHistorialAsync(int citaId, string? estadoAnterior, string estadoNuevo, string usuarioTipo, int usuarioId, string? comentario = null)
        {
            var historial = new HistorialEstadoCita
            {
                CitaId = citaId,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                UsuarioTipo = usuarioTipo,
                UsuarioId = usuarioId,
                Comentario = comentario,
                FechaCambio = DateTime.UtcNow
            };

            _context.HistorialEstadosCita.Add(historial);
            await _context.SaveChangesAsync();
        }

        private CitaDto MapToDto(Cita cita)
        {
            var totalPagado = cita.Adelanto + (cita.PagosParciales?.Sum(p => p.Monto) ?? 0);

            return new CitaDto
            {
                Id = cita.Id,
                UsuarioId = cita.UsuarioId,
                ClienteNombre = cita.Usuario?.Nombre ?? string.Empty,
                ClienteEmail = cita.Usuario?.Email ?? string.Empty,
                ClienteTelefono = cita.Usuario?.Telefono ?? string.Empty,
                ArtistaReferenciaId = cita.ArtistaReferenciaId,
                ArtistaNombre = cita.ArtistaReferencia?.Nombre ?? string.Empty,
                FechaHoraInicio = cita.FechaHoraInicio,
                FechaHoraFin = cita.FechaHoraFin,
                Estado = cita.Estado,
                PrecioTotal = cita.PrecioTotal,
                Adelanto = cita.Adelanto,
                TotalPagado = totalPagado,
                ZonaCuerpo = cita.ZonaCuerpo?.Nombre,
                ZonaCuerpoId = cita.ZonaCuerpoId,
                TamanioCm = cita.TamanioCm,
                FotoReferenciaUrl = cita.FotoReferenciaUrl,
                NotasInternas = cita.NotasInternas,
                NotasPublicas = cita.NotasPublicas,
                RequiereRecordatorio = cita.RequiereRecordatorio,
                FechaRecordatorioEnviado = cita.FechaRecordatorioEnviado,
            };
        }
    }
}
using InkManager.Core.DTOs;
using InkManager.Core.DTOs.Common;
using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InkManager.Services.Implementations
{
    public class CitaService : ICitaService
    {
        private readonly ApplicationDbContext _context;

        public CitaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CitaDto?> GetByIdAsync(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Artista)
                .Include(c => c.Asistente)
                .Include(c => c.ZonaCuerpo)
                .Include(c => c.PagosParciales)
                .FirstOrDefaultAsync(c => c.Id == id && !c.EliminadoLogico);

            if (cita == null) return null;

            return MapToDto(cita);
        }

        public async Task<CitaDto> CreateAsync(CrearCitaDto dto)
        {
            // Validar disponibilidad
            var disponible = await ValidarDisponibilidadAsync(dto.ArtistaId, dto.FechaHoraInicio, dto.FechaHoraFin);
            if (!disponible)
                throw new InvalidOperationException("El artista no está disponible en ese horario");

            var cita = new Cita
            {
                ClienteId = dto.ClienteId,
                ArtistaId = dto.ArtistaId,
                AsistenteId = dto.AsistenteId,
                FechaHoraInicio = dto.FechaHoraInicio,
                FechaHoraFin = dto.FechaHoraFin,
                PrecioTotal = dto.PrecioTotal,
                Adelanto = dto.Adelanto,
                ZonaCuerpoId = dto.ZonaCuerpoId,
                TamanioCm = dto.TamanioCm,
                NotasInternas = dto.NotasInternas,
                NotasPublicas = dto.NotasPublicas,
                FotoReferenciaUrl = dto.FotoReferenciaUrl,
                RequiereRecordatorio = dto.RequiereRecordatorio,
                Estado = "pendiente"
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            // Registrar en historial
            await RegistrarHistorialAsync(cita.Id, null, "pendiente", "sistema", 0, "Cita creada");

            // Si hay adelanto, registrar el pago
            if (dto.Adelanto > 0)
            {
                await RegistrarPagoAsync(cita.Id, new RegistrarPagoDto
                {
                    Monto = dto.Adelanto,
                    MetodoPago = "efectivo",
                    Nota = "Adelanto inicial"
                });
            }

            return (await GetByIdAsync(cita.Id))!;
        }

        public async Task<CitaDto?> UpdateAsync(int id, ActualizarCitaDto dto)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null || cita.EliminadoLogico) return null;

            // Si cambia el horario, validar disponibilidad
            if (dto.FechaHoraInicio.HasValue && dto.FechaHoraFin.HasValue)
            {
                var disponible = await ValidarDisponibilidadAsync(cita.ArtistaId, dto.FechaHoraInicio.Value, dto.FechaHoraFin.Value, id);
                if (!disponible)
                    throw new InvalidOperationException("El artista no está disponible en ese horario");

                cita.FechaHoraInicio = dto.FechaHoraInicio.Value;
                cita.FechaHoraFin = dto.FechaHoraFin.Value;
            }

            // Actualizar campos
            if (dto.AsistenteId.HasValue) cita.AsistenteId = dto.AsistenteId;
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

            // Si el pago completa el saldo, sugerir cambio de estado (opcional)
            var saldo = await GetSaldoPendienteAsync(citaId);
            if (saldo <= 0 && cita.Estado == "confirmada")
            {
                // Podría marcarse automáticamente como pagada si quieres
            }

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
                .Include(c => c.Cliente)
                .Include(c => c.Artista)
                .Include(c => c.Asistente)
                .Include(c => c.ZonaCuerpo)
                .Where(c => !c.EliminadoLogico);

            // Aplicar filtros
            if (filtro.ArtistaId.HasValue)
                query = query.Where(c => c.ArtistaId == filtro.ArtistaId.Value);

            if (filtro.EstudioId.HasValue)
                query = query.Where(c => c.Artista!.EstudioId == filtro.EstudioId.Value);

            if (!string.IsNullOrEmpty(filtro.Estado))
                query = query.Where(c => c.Estado == filtro.Estado);

            if (filtro.ClienteId.HasValue)
                query = query.Where(c => c.ClienteId == filtro.ClienteId.Value);

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
                .Include(c => c.Cliente)
                .Include(c => c.Artista)
                .Include(c => c.ZonaCuerpo)
                .Where(c => c.ArtistaId == artistaId
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
                .Include(c => c.Cliente)
                .Include(c => c.Artista)
                .Include(c => c.ZonaCuerpo)
                .Where(c => c.ArtistaId == artistaId && !c.EliminadoLogico);

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
                .Include(c => c.Cliente)
                .Include(c => c.Artista)
                .Include(c => c.ZonaCuerpo)
                .Where(c => c.ClienteId == clienteId && !c.EliminadoLogico)
                .OrderByDescending(c => c.FechaHoraInicio)
                .ToListAsync();

            return citas.Select(MapToDto).ToList();
        }

        public async Task<int> GetCountByEstadoAsync(int artistaId, string estado)
        {
            return await _context.Citas
                .CountAsync(c => c.ArtistaId == artistaId && c.Estado == estado && !c.EliminadoLogico);
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
                .Where(c => c.ArtistaId == artistaId && c.Estado == "completada" && !c.EliminadoLogico);

            if (fechaInicio.HasValue)
                query = query.Where(c => c.FechaHoraInicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(c => c.FechaHoraFin <= fechaFin.Value);

            return await query.SumAsync(c => c.PrecioTotal);
        }

        public async Task<bool> ValidarDisponibilidadAsync(int artistaId, DateTime inicio, DateTime fin, int? citaIdExcluir = null)
        {
            var query = _context.Citas
                .Where(c => c.ArtistaId == artistaId
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
                .Where(c => c.ArtistaId == artistaId
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
                ClienteId = cita.ClienteId,
                ClienteNombre = cita.Cliente?.Nombre ?? string.Empty,
                ClienteEmail = cita.Cliente?.Email ?? string.Empty,
                ClienteTelefono = cita.Cliente?.Telefono ?? string.Empty,
                ArtistaId = cita.ArtistaId,
                ArtistaNombre = cita.Artista?.Nombre ?? string.Empty,
                AsistenteId = cita.AsistenteId,
                AsistenteNombre = cita.Asistente?.Nombre,
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
                FechaRecordatorioEnviado = cita.FechaRecordatorioEnviado
            };
        }
    }
}
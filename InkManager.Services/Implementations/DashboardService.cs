using InkManager.Core.DTOs;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InkManager.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardResumenDto> GetResumenAsync(int? artistaId = null, int? estudioId = null)
        {
            var queryCitas = _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .ThenInclude(a => a.EstudioUsuarios)  // Agregar para acceder a estudios
                .AsQueryable();

            var queryUsuarios = _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .Include(u => u.EstudioUsuarios)  // Agregar para acceder a estudios
                .AsQueryable();

            // Filtrar por artista
            if (artistaId.HasValue)
            {
                queryCitas = queryCitas.Where(c => c.ArtistaReferenciaId == artistaId.Value);
            }
            // CORREGIDO: Filtrar por estudio usando la tabla puente
            else if (estudioId.HasValue)
            {
                queryCitas = queryCitas.Where(c => c.ArtistaReferencia!.EstudioUsuarios.Any(eu => eu.EstudioId == estudioId.Value));
                queryUsuarios = queryUsuarios.Where(u => u.EstudioUsuarios.Any(eu => eu.EstudioId == estudioId.Value));
            }

            var hoy = DateTime.UtcNow.Date;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var dashboard = new DashboardResumenDto
            {
                CitasHoy = await queryCitas.CountAsync(c => c.FechaHoraInicio.Date == hoy && c.Estado != "cancelada"),
                CitasSemana = await queryCitas.CountAsync(c => c.FechaHoraInicio.Date >= inicioSemana && c.FechaHoraInicio.Date <= hoy && c.Estado != "cancelada"),
                CitasPendientes = await queryCitas.CountAsync(c => c.Estado == "pendiente"),
                CitasConfirmadas = await queryCitas.CountAsync(c => c.Estado == "confirmada"),
                CitasEnCurso = await queryCitas.CountAsync(c => c.Estado == "en_curso"),
                CitasCompletadasMes = await queryCitas.CountAsync(c => c.Estado == "completada" && c.FechaHoraInicio >= inicioMes),
                IngresosMes = await queryCitas.Where(c => c.Estado == "completada" && c.FechaHoraInicio >= inicioMes).SumAsync(c => c.PrecioTotal),
                IngresosHoy = await queryCitas.Where(c => c.Estado == "completada" && c.FechaHoraInicio.Date == hoy).SumAsync(c => c.PrecioTotal),
                NuevosClientesMes = await _context.Usuarios
                    .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                    .Where(u => u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "cliente")
                        && u.FechaCreacion >= inicioMes
                        && !u.EliminadoLogico)
                    .CountAsync(),
                ArtistasActivos = await _context.Usuarios
                    .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                    .Where(u => u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "artista")
                        && u.Activo
                        && !u.EliminadoLogico)
                    .CountAsync(),
                ProximasCitas = await GetProximasCitasAsync(artistaId, 7),
                CitasPorEstado = await GetCitasPorEstadoAsync(artistaId, estudioId),
                IngresosPorArtista = await GetIngresosPorArtistaAsync(estudioId, inicioMes)
            };

            return dashboard;
        }
        public async Task<List<ProximasCitasDto>> GetProximasCitasAsync(int? artistaId = null, int dias = 7)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(dias);

            var query = _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .Include(c => c.ZonaCuerpo)
                .Where(c => c.FechaHoraInicio >= DateTime.UtcNow
                    && c.FechaHoraInicio <= fechaLimite
                    && c.Estado != "cancelada"
                    && c.Estado != "completada"
                    && !c.EliminadoLogico);

            if (artistaId.HasValue)
                query = query.Where(c => c.ArtistaReferenciaId == artistaId.Value);

            var citas = await query
                .OrderBy(c => c.FechaHoraInicio)
                .Take(10)
                .ToListAsync();

            return citas.Select(c => new ProximasCitasDto
            {
                Id = c.Id,
                ClienteNombre = c.Usuario?.Nombre ?? string.Empty,
                ArtistaNombre = c.ArtistaReferencia?.Nombre ?? string.Empty,
                FechaHoraInicio = c.FechaHoraInicio,
                Estado = c.Estado,
                ZonaCuerpo = c.ZonaCuerpo?.Nombre
            }).ToList();
        }

        public async Task<byte[]> ExportarReporteCitasAsync(DateTime fechaInicio, DateTime fechaFin, int? artistaId = null)
        {
            var query = _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .Where(c => c.FechaHoraInicio >= fechaInicio
                    && c.FechaHoraFin <= fechaFin
                    && !c.EliminadoLogico);

            if (artistaId.HasValue)
                query = query.Where(c => c.ArtistaReferenciaId == artistaId.Value);

            var citas = await query.ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Cliente,Artista,Fecha Inicio,Fecha Fin,Estado,Precio Total,Adelanto,Saldo");

            foreach (var cita in citas)
            {
                var pagado = cita.Adelanto + (cita.PagosParciales?.Sum(p => p.Monto) ?? 0);
                csv.AppendLine($"\"{cita.Id}\",\"{cita.Usuario?.Nombre}\",\"{cita.ArtistaReferencia?.Nombre}\",{cita.FechaHoraInicio:yyyy-MM-dd HH:mm},{cita.FechaHoraFin:yyyy-MM-dd HH:mm},{cita.Estado},{cita.PrecioTotal:C},{pagado:C},{cita.PrecioTotal - pagado:C}");
            }

            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }

        private async Task<List<CitasPorEstadoDto>> GetCitasPorEstadoAsync(int? artistaId = null, int? estudioId = null)
        {
            var query = _context.Citas
                .Include(c => c.ArtistaReferencia)
                .ThenInclude(a => a.EstudioUsuarios)
                .AsQueryable();

            if (artistaId.HasValue)
                query = query.Where(c => c.ArtistaReferenciaId == artistaId.Value);
            else if (estudioId.HasValue)
                query = query.Where(c => c.ArtistaReferencia!.EstudioUsuarios.Any(eu => eu.EstudioId == estudioId.Value));

            var estados = new[] { "pendiente", "confirmada", "en_curso", "completada", "cancelada", "no_asistio" };
            var colores = new Dictionary<string, string>
    {
        { "pendiente", "#F59E0B" },
        { "confirmada", "#3B82F6" },
        { "en_curso", "#8B5CF6" },
        { "completada", "#10B981" },
        { "cancelada", "#EF4444" },
        { "no_asistio", "#6B7280" }
    };

            var resultado = new List<CitasPorEstadoDto>();
            foreach (var estado in estados)
            {
                var cantidad = await query.CountAsync(c => c.Estado == estado);
                resultado.Add(new CitasPorEstadoDto
                {
                    Estado = estado,
                    Cantidad = cantidad,
                    Color = colores[estado]
                });
            }

            return resultado;
        }
        private async Task<List<IngresosPorArtistaDto>> GetIngresosPorArtistaAsync(int? estudioId, DateTime desde)
        {
            var query = _context.Citas
                .Include(c => c.ArtistaReferencia)
                .ThenInclude(a => a.EstudioUsuarios)
                .Where(c => c.Estado == "completada"
                    && c.FechaHoraInicio >= desde
                    && !c.EliminadoLogico);

            if (estudioId.HasValue)
                query = query.Where(c => c.ArtistaReferencia!.EstudioUsuarios.Any(eu => eu.EstudioId == estudioId.Value));

            var ingresos = await query
                .GroupBy(c => new { c.ArtistaReferenciaId, c.ArtistaReferencia!.Nombre })
                .Select(g => new IngresosPorArtistaDto
                {
                    ArtistaId = g.Key.ArtistaReferenciaId,
                    ArtistaNombre = g.Key.Nombre,
                    TotalIngresos = g.Sum(c => c.PrecioTotal),
                    TotalCitas = g.Count()
                })
                .OrderByDescending(g => g.TotalIngresos)
                .Take(5)
                .ToListAsync();

            return ingresos;
        }
    }
}
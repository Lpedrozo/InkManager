using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace InkManager.Web.Controllers
{
    [Authorize]
    [Route("reportes")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICitaService _citaService;
        private readonly IClienteService _clienteService;

        public ReportesController(ApplicationDbContext context, ICitaService citaService, IClienteService clienteService)
        {
            _context = context;
            _citaService = citaService;
            _clienteService = clienteService;
        }

        // GET: /reportes/ingresos
        [HttpGet("ingresos")]
        public IActionResult Ingresos()
        {
            return View();
        }

        // GET: /reportes/clientes
        [HttpGet("clientes")]
        public IActionResult Clientes()
        {
            return View();
        }

        // GET: /reportes/artistas
        [HttpGet("artistas")]
        public IActionResult Artistas()
        {
            return View();
        }

        // ============================================
        // API ENDPOINTS
        // ============================================

        // GET: /api/reportes/ingresos
        [HttpGet("/api/reportes/ingresos")]
        public async Task<IActionResult> GetReporteIngresos(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? artistaId = null,
            int? estudioId = null)
        {
            try
            {
                var query = _context.Citas
                    .Include(c => c.Usuario)
                    .Include(c => c.ArtistaReferencia)
                    .Include(c => c.PagosParciales)
                    .Include(c => c.Estudio)
                    .Where(c => c.Estado == "completada" && !c.EliminadoLogico);

                if (fechaInicio.HasValue)
                    query = query.Where(c => c.FechaHoraInicio >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(c => c.FechaHoraInicio <= fechaFin.Value);

                if (artistaId.HasValue)
                    query = query.Where(c => c.ArtistaReferenciaId == artistaId.Value);

                if (estudioId.HasValue)
                    query = query.Where(c => c.EstudioId == estudioId.Value);

                var citas = await query
                    .OrderByDescending(c => c.FechaHoraInicio)
                    .ToListAsync();

                var totalIngresos = citas.Sum(c => c.PrecioTotal);
                var totalCitas = citas.Count;
                var totalAdelantos = citas.Sum(c => c.Adelanto);
                var totalPagosParciales = citas.Sum(c => c.PagosParciales?.Sum(p => p.Monto) ?? 0);
                var totalPagado = totalAdelantos + totalPagosParciales;
                var saldoPendiente = totalIngresos - totalPagado;

                var ingresosPorMes = citas
                    .GroupBy(c => new { c.FechaHoraInicio.Year, c.FechaHoraInicio.Month })
                    .Select(g => new {
                        Año = g.Key.Year,
                        Mes = g.Key.Month,
                        NombreMes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM", new System.Globalization.CultureInfo("es-ES")),
                        Total = g.Sum(c => c.PrecioTotal),
                        Cantidad = g.Count()
                    })
                    .OrderBy(g => g.Año).ThenBy(g => g.Mes)
                    .ToList();

                var ingresosPorArtista = citas
                    .GroupBy(c => new { c.ArtistaReferenciaId, c.ArtistaReferencia!.Nombre })
                    .Select(g => new {
                        ArtistaId = g.Key.ArtistaReferenciaId,
                        ArtistaNombre = g.Key.Nombre,
                        Total = g.Sum(c => c.PrecioTotal),
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(g => g.Total)
                    .ToList();

                var citasDto = citas.Select(c => new {
                    c.Id,
                    ClienteNombre = c.Usuario?.Nombre ?? "N/A",
                    ClienteTelefono = c.Usuario?.Telefono ?? "N/A",
                    ArtistaNombre = c.ArtistaReferencia?.Nombre ?? "N/A",
                    EstudioNombre = c.Estudio?.Nombre ?? "N/A",
                    FechaHoraInicio = c.FechaHoraInicio,
                    c.PrecioTotal,
                    c.Adelanto,
                    TotalPagado = c.Adelanto + (c.PagosParciales?.Sum(p => p.Monto) ?? 0),
                    SaldoPendiente = c.PrecioTotal - (c.Adelanto + (c.PagosParciales?.Sum(p => p.Monto) ?? 0)),
                    Pagos = c.PagosParciales?.Select(p => new { p.Monto, p.FechaPago, p.MetodoPago })
                });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        citas = citasDto,
                        totalIngresos,
                        totalCitas,
                        totalAdelantos,
                        totalPagosParciales,
                        totalPagado,
                        saldoPendiente,
                        ingresosPorMes,
                        ingresosPorArtista
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: /api/reportes/clientes
        [HttpGet("/api/reportes/clientes")]
        public async Task<IActionResult> GetReporteClientes(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? artistaId = null,
            bool soloConCitas = false)
        {
            try
            {
                var query = _context.ClientesArtistas
                    .Include(ca => ca.Cliente)
                    .Include(ca => ca.Cliente.CitasComoCliente.Where(c => c.Estado == "completada"))
                    .AsQueryable();

                if (artistaId.HasValue)
                    query = query.Where(ca => ca.ArtistaId == artistaId.Value);

                if (fechaInicio.HasValue)
                    query = query.Where(ca => ca.FechaAsociacion >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(ca => ca.FechaAsociacion <= fechaFin.Value);

                var resultados = await query.ToListAsync();

                var clientes = resultados
                    .GroupBy(ca => ca.Cliente)
                    .Select(g => new
                    {
                        ClienteId = g.Key.Id,
                        ClienteNombre = g.Key.Nombre,
                        ClienteTelefono = g.Key.Telefono,
                        ClienteEmail = g.Key.Email,
                        FechaRegistro = g.Key.FechaCreacion,
                        TotalCitas = g.Key.CitasComoCliente?.Count ?? 0,
                        TotalGastado = g.Key.CitasComoCliente?.Sum(c => c.PrecioTotal) ?? 0,
                        UltimaCita = g.Key.CitasComoCliente?.OrderByDescending(c => c.FechaHoraInicio).FirstOrDefault()?.FechaHoraInicio
                    })
                    .OrderByDescending(c => c.TotalGastado)
                    .ToList();

                if (soloConCitas)
                    clientes = clientes.Where(c => c.TotalCitas > 0).ToList();

                var totalClientes = clientes.Count;
                var totalGastadoGeneral = clientes.Sum(c => c.TotalGastado);
                var promedioGasto = totalClientes > 0 ? totalGastadoGeneral / totalClientes : 0;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        clientes,
                        totalClientes,
                        totalGastadoGeneral,
                        promedioGasto
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: /api/reportes/artistas
        [HttpGet("/api/reportes/artistas")]
        public async Task<IActionResult> GetReporteArtistas(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? estudioId = null)
        {
            try
            {
                var query = _context.Citas
                    .Include(c => c.ArtistaReferencia)
                    .Include(c => c.PagosParciales)
                    .Where(c => c.Estado == "completada" && !c.EliminadoLogico);

                if (fechaInicio.HasValue)
                    query = query.Where(c => c.FechaHoraInicio >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(c => c.FechaHoraInicio <= fechaFin.Value);

                if (estudioId.HasValue)
                    query = query.Where(c => c.ArtistaReferencia!.EstudioUsuarios.Any(eu => eu.EstudioId == estudioId.Value));

                var citas = await query.ToListAsync();

                var artistas = citas
                    .GroupBy(c => new { c.ArtistaReferenciaId, c.ArtistaReferencia!.Nombre, c.ArtistaReferencia.Telefono, c.ArtistaReferencia.Email })
                    .Select(g => new
                    {
                        ArtistaId = g.Key.ArtistaReferenciaId,
                        ArtistaNombre = g.Key.Nombre,
                        Telefono = g.Key.Telefono,
                        Email = g.Key.Email,
                        TotalCitas = g.Count(),
                        TotalIngresos = g.Sum(c => c.PrecioTotal),
                        TotalAdelantos = g.Sum(c => c.Adelanto),
                        TicketPromedio = g.Average(c => c.PrecioTotal),
                        ComisionEstimada = g.Sum(c => c.PrecioTotal) * 0.7m // 70% para el artista
                    })
                    .OrderByDescending(a => a.TotalIngresos)
                    .ToList();

                return Ok(new { success = true, data = artistas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: /api/reportes/exportar/ingresos
        [HttpGet("/api/reportes/exportar/ingresos")]
        public async Task<IActionResult> ExportarIngresos(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? artistaId = null,
            int? estudioId = null)
        {
            var query = _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.ArtistaReferencia)
                .Include(c => c.PagosParciales)
                .Include(c => c.Estudio)
                .Where(c => c.Estado == "completada" && !c.EliminadoLogico);

            if (fechaInicio.HasValue)
                query = query.Where(c => c.FechaHoraInicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(c => c.FechaHoraInicio <= fechaFin.Value);

            if (artistaId.HasValue)
                query = query.Where(c => c.ArtistaReferenciaId == artistaId.Value);

            if (estudioId.HasValue)
                query = query.Where(c => c.EstudioId == estudioId.Value);

            var citas = await query
                .OrderByDescending(c => c.FechaHoraInicio)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Fecha,Cliente,Teléfono Cliente,Artista,Estudio,Precio Total,Adelanto,Pagado,Saldo");

            foreach (var cita in citas)
            {
                var pagado = cita.Adelanto + (cita.PagosParciales?.Sum(p => p.Monto) ?? 0);
                var saldo = cita.PrecioTotal - pagado;

                csv.AppendLine($"\"{cita.FechaHoraInicio:yyyy-MM-dd HH:mm}\"," +
                              $"\"{EscapeCsv(cita.Usuario?.Nombre ?? "N/A")}\"," +
                              $"\"{cita.Usuario?.Telefono ?? "N/A"}\"," +
                              $"\"{EscapeCsv(cita.ArtistaReferencia?.Nombre ?? "N/A")}\"," +
                              $"\"{EscapeCsv(cita.Estudio?.Nombre ?? "N/A")}\"," +
                              $"{cita.PrecioTotal:F2}," +
                              $"{cita.Adelanto:F2}," +
                              $"{pagado:F2}," +
                              $"{saldo:F2}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fechaStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(bytes, "text/csv", $"reporte_ingresos_{fechaStr}.csv");
        }

        // GET: /api/reportes/exportar/clientes
        [HttpGet("/api/reportes/exportar/clientes")]
        public async Task<IActionResult> ExportarClientes(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? artistaId = null)
        {
            var query = _context.ClientesArtistas
                .Include(ca => ca.Cliente)
                .Include(ca => ca.Cliente.CitasComoCliente.Where(c => c.Estado == "completada"))
                .AsQueryable();

            if (artistaId.HasValue)
                query = query.Where(ca => ca.ArtistaId == artistaId.Value);

            if (fechaInicio.HasValue)
                query = query.Where(ca => ca.FechaAsociacion >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(ca => ca.FechaAsociacion <= fechaFin.Value);

            var resultados = await query.ToListAsync();

            var clientes = resultados
                .GroupBy(ca => ca.Cliente)
                .Select(g => new
                {
                    Nombre = g.Key.Nombre,
                    Telefono = g.Key.Telefono,
                    Email = g.Key.Email,
                    FechaRegistro = g.Key.FechaCreacion,
                    TotalCitas = g.Key.CitasComoCliente?.Count ?? 0,
                    TotalGastado = g.Key.CitasComoCliente?.Sum(c => c.PrecioTotal) ?? 0
                })
                .OrderByDescending(c => c.TotalGastado)
                .ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Nombre,Teléfono,Email,Fecha Registro,Total Citas,Total Gastado");

            foreach (var cliente in clientes)
            {
                csv.AppendLine($"\"{EscapeCsv(cliente.Nombre)}\"," +
                              $"\"{cliente.Telefono}\"," +
                              $"\"{cliente.Email ?? ""}\"," +
                              $"\"{cliente.FechaRegistro:yyyy-MM-dd}\"," +
                              $"{cliente.TotalCitas}," +
                              $"{cliente.TotalGastado:F2}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fechaStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return File(bytes, "text/csv", $"reporte_clientes_{fechaStr}.csv");
        }

        private string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\"", "\"\"");
        }
    }
}
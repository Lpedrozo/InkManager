using InkManager.Core.DTOs;
using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using InkManager.Services.Implementations;
using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InkManager.Web.Controllers
{
    [Authorize]

    public class CitasController : Controller
    {
        private readonly ICitaService _citaService;
        private readonly IClienteService _clienteService;
        private readonly ApplicationDbContext _context;

        public CitasController(ICitaService citaService, ApplicationDbContext context, IClienteService clienteService)
        {
            _citaService = citaService;
            _context = context;
            _clienteService = clienteService;
        }

        // GET: /citas
        [HttpGet("/citas")]
        public IActionResult Index()
        {
            return View("~/Views/Citas/Index.cshtml");
        }

        // GET: /citas/detalle/{id}
        [HttpGet("/citas/detalle/{id}")]
        public IActionResult Detalle(int id)
        {
            ViewBag.CitaId = id;
            return View("~/Views/Citas/Detalle.cshtml");
        }

        // GET: /api/citas
        [HttpGet("/api/citas")]
        public async Task<IActionResult> GetCitas([FromQuery] FiltroCitasDto filtro)
        {
            var result = await _citaService.GetCitasFiltradasAsync(filtro);
            return Ok(new { success = true, data = result });
        }

        // GET: /api/citas/{id}
        [HttpGet("/api/citas/{id}")]
        public async Task<IActionResult> GetCita(int id)
        {
            var cita = await _citaService.GetByIdAsync(id);
            if (cita == null)
                return NotFound(new { success = false, message = "Cita no encontrada" });
            return Ok(new { success = true, data = cita });
        }

        // GET: /api/citas/artista/{artistaId}/estadisticas
        [HttpGet("/api/citas/artista/{artistaId}/estadisticas")]
        public async Task<IActionResult> GetEstadisticas(int artistaId)
        {
            var estadisticas = await _citaService.GetEstadisticasPorEstadoAsync(artistaId);
            return Ok(new { success = true, data = estadisticas });
        }
        // GET: /api/zonas-cuerpo
        [HttpGet("/api/zonas-cuerpo")]
        public async Task<IActionResult> GetZonasCuerpo()
        {
            var zonas = await _context.ZonasCuerpo
                .Where(z => !z.EliminadoLogico)
                .OrderBy(z => z.OrdenVisual)
                .Select(z => new { z.Id, z.Nombre, z.Categoria, z.CoordenadasJson })
                .ToListAsync();

            return Ok(new { success = true, data = zonas });
        }

        // GET: /api/horarios-disponibles
        [HttpGet("/api/horarios-disponibles")]
        public async Task<IActionResult> GetHorariosDisponibles(int artistaId, DateTime fecha)
        {
            var horarios = await _citaService.GetHorariosDisponiblesAsync(artistaId, fecha);
            return Ok(new { success = true, data = horarios });
        }
        // POST: /api/citas
        [HttpPost("/api/citas")]
        public async Task<IActionResult> CreateCita([FromForm] CrearCitaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var cita = await _citaService.CreateAsync(dto);
                return Ok(new { success = true, message = "Cita creada exitosamente", data = cita });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno al crear la cita" });
            }
        }        // PATCH: /api/citas/{id}/estado
        [HttpPatch("/api/citas/{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
        {
            var result = await _citaService.CambiarEstadoAsync(id, dto, "artista", 1);
            if (!result)
                return NotFound(new { success = false, message = "Cita no encontrada" });
            return Ok(new { success = true, message = $"Estado cambiado a {dto.Estado}" });
        }

        // POST: /api/citas/{id}/pagos
        [HttpPost("/api/citas/{id}/pagos")]
        public async Task<IActionResult> RegistrarPago(int id, [FromBody] RegistrarPagoDto dto)
        {
            try
            {
                var nuevoSaldo = await _citaService.RegistrarPagoAsync(id, dto);
                return Ok(new { success = true, message = "Pago registrado", data = new { nuevoSaldo } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        // POST: /api/clientes/rapido
        [HttpPost("/api/clientes/rapido")]
        public async Task<IActionResult> CreateClienteRapido([FromBody] CrearClienteRapidoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Telefono))
                return BadRequest(new { success = false, message = "Nombre y teléfono son requeridos" });

            try
            {
                var cliente = await _clienteService.CreateAsync(new CrearClienteDto
                {
                    Nombre = dto.Nombre,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    Password = null  // Usará la genérica
                });

                return Ok(new { success = true, message = "Cliente creado exitosamente", data = cliente });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        // GET: /api/citas/estadisticas (para el dashboard)
        [HttpGet("/api/citas/estadisticas")]
        public async Task<IActionResult> GetEstadisticasPorArtista()
        {
            var artistaIdClaim = User.FindFirst("ArtistaId")?.Value;
            var artistaId = artistaIdClaim != null ? int.Parse(artistaIdClaim) : 0;

            if (artistaId == 0)
                return BadRequest(new { success = false, message = "No se pudo identificar al artista" });

            var estadisticas = await _citaService.GetEstadisticasPorEstadoAsync(artistaId);
            return Ok(new { success = true, data = estadisticas });
        }
        // GET: /api/citas/{id}/pagos
        [HttpGet("/api/citas/{id}/pagos")]
        public async Task<IActionResult> GetPagos(int id)
        {
            var pagos = await _citaService.GetHistorialPagosAsync(id);
            return Ok(new { success = true, data = pagos });
        }
        // PUT: /api/citas/{id}
        [HttpPut("/api/citas/{id}")]
        public async Task<IActionResult> UpdateCita(int id, [FromBody] EditarCitaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var cita = await _citaService.UpdateAsync(id, dto);
                if (cita == null)
                    return NotFound(new { success = false, message = "Cita no encontrada" });

                return Ok(new { success = true, message = "Cita actualizada exitosamente", data = cita });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno al actualizar la cita" });
            }
        }

        // PATCH: /api/citas/{id}/reprogramar
        [HttpPatch("/api/citas/{id}/reprogramar")]
        public async Task<IActionResult> ReprogramarCita(int id, [FromBody] ReprogramarCitaDto dto)
        {
            try
            {
                var cita = await _citaService.ReprogramarAsync(id, dto);
                if (cita == null)
                    return NotFound(new { success = false, message = "Cita no encontrada" });

                return Ok(new { success = true, message = "Cita reprogramada exitosamente", data = cita });
            }
            catch (InvalidOperationException ex)
            {  
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno al reprogramar la cita" });
            }
        }
    }
}
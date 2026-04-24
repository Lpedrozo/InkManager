using InkManager.Core.DTOs;
using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InkManager.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private readonly ICitaService _citaService;

        public CitasController(ICitaService citaService)
        {
            _citaService = citaService;
        }

        // GET: api/citas/artista/{artistaId}/estadisticas
        [HttpGet("artista/{artistaId}/estadisticas")]
        public async Task<IActionResult> GetEstadisticas(int artistaId)
        {
            var estadisticas = await _citaService.GetEstadisticasPorEstadoAsync(artistaId);
            return Ok(new { success = true, data = estadisticas });
        }

        // GET: api/citas?artistaId=1&pagina=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetCitas([FromQuery] FiltroCitasDto filtro)
        {
            var result = await _citaService.GetCitasFiltradasAsync(filtro);
            return Ok(new { success = true, data = result });
        }

        // GET: api/citas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cita = await _citaService.GetByIdAsync(id);
            if (cita == null)
                return NotFound(new { success = false, message = "Cita no encontrada" });

            return Ok(new { success = true, data = cita });
        }

        // GET: api/citas/artista/{artistaId}/rango
        [HttpGet("artista/{artistaId}/rango")]
        public async Task<IActionResult> GetCitasPorRango(int artistaId, DateTime? inicio = null, DateTime? fin = null)
        {
            var citas = await _citaService.GetCitasDelArtistaAsync(artistaId, inicio, fin);
            return Ok(new { success = true, data = citas });
        }

        // GET: api/citas/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> GetCitasDelCliente(int clienteId)
        {
            var citas = await _citaService.GetCitasDelClienteAsync(clienteId);
            return Ok(new { success = true, data = citas });
        }

        // POST: api/citas
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearCitaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

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
        }

        // PUT: api/citas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarCitaDto dto)
        {
            try
            {
                var cita = await _citaService.UpdateAsync(id, dto);
                if (cita == null)
                    return NotFound(new { success = false, message = "Cita no encontrada" });

                return Ok(new { success = true, message = "Cita actualizada", data = cita });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/citas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _citaService.DeleteAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "Cita no encontrada" });

            return Ok(new { success = true, message = "Cita eliminada" });
        }

        // PATCH: api/citas/{id}/estado
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
        {
            var result = await _citaService.CambiarEstadoAsync(id, dto, "artista", 1);

            if (!result)
                return NotFound(new { success = false, message = "Cita no encontrada" });

            return Ok(new { success = true, message = $"Estado cambiado a {dto.Estado}" });
        }

        // POST: api/citas/{id}/cancelar
        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id, [FromBody] string motivo)
        {
            var result = await _citaService.CancelarCitaAsync(id, motivo, "artista", 1);

            if (!result)
                return NotFound(new { success = false, message = "Cita no encontrada" });

            return Ok(new { success = true, message = "Cita cancelada" });
        }

        // GET: api/citas/{id}/saldo
        [HttpGet("{id}/saldo")]
        public async Task<IActionResult> GetSaldo(int id)
        {
            var saldo = await _citaService.GetSaldoPendienteAsync(id);
            return Ok(new { success = true, data = new { saldo } });
        }

        // POST: api/citas/{id}/pagos
        [HttpPost("{id}/pagos")]
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

        // GET: api/citas/{id}/pagos
        [HttpGet("{id}/pagos")]
        public async Task<IActionResult> GetPagos(int id)
        {
            var pagos = await _citaService.GetHistorialPagosAsync(id);
            return Ok(new { success = true, data = pagos });
        }
    }
}
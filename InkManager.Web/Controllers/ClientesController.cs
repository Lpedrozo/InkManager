using InkManager.Core.DTOs;
using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InkManager.Web.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // ============================================
        // VISTAS
        // ============================================

        // GET: /clientes
        [HttpGet("/clientes")]
        public IActionResult Index()
        {
            return View("~/Views/Clientes/Index.cshtml");
        }

        // ============================================
        // API ENDPOINTS
        // ============================================

        // GET: /api/clientes
        [HttpGet("/api/clientes")]
        public async Task<IActionResult> GetAll(int pagina = 1, int pageSize = 10, string? search = null)
        {
            var result = await _clienteService.GetAllAsync(pagina, pageSize, search);
            return Ok(new { success = true, data = result });
        }

        // GET: /api/clientes/{id}
        [HttpGet("/api/clientes/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _clienteService.GetByIdAsync(id);
            if (cliente == null)
                return NotFound(new { success = false, message = "Cliente no encontrado" });
            return Ok(new { success = true, data = cliente });
        }

        // GET: /api/clientes/telefono/{telefono}
        [HttpGet("/api/clientes/telefono/{telefono}")]
        public async Task<IActionResult> GetByTelefono(string telefono)
        {
            var cliente = await _clienteService.GetByTelefonoAsync(telefono);
            if (cliente == null)
                return NotFound(new { success = false, message = "Cliente no encontrado" });
            return Ok(new { success = true, data = cliente });
        }

        // POST: /api/clientes
        [HttpPost("/api/clientes")]
        public async Task<IActionResult> Create([FromBody] CrearClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            try
            {
                var cliente = await _clienteService.CreateAsync(dto);
                return Ok(new { success = true, message = "Cliente creado exitosamente", data = cliente });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // PUT: /api/clientes/{id}
        [HttpPut("/api/clientes/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarClienteDto dto)
        {
            try
            {
                var cliente = await _clienteService.UpdateAsync(id, dto);
                if (cliente == null)
                    return NotFound(new { success = false, message = "Cliente no encontrado" });
                return Ok(new { success = true, message = "Cliente actualizado", data = cliente });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // DELETE: /api/clientes/{id}
        [HttpDelete("/api/clientes/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _clienteService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Cliente no encontrado" });
                return Ok(new { success = true, message = "Cliente eliminado" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: /api/clientes/frecuentes
        [HttpGet("/api/clientes/frecuentes")]
        public async Task<IActionResult> GetFrecuentes(int top = 10)
        {
            var clientes = await _clienteService.GetClientesFrecuentesAsync(top);
            return Ok(new { success = true, data = clientes });
        }
        // GET: /api/clientes/reporte/nuevos?fechaInicio=2024-01-01&fechaFin=2024-12-31
        [HttpGet("/api/clientes/reporte/nuevos")]
        public async Task<IActionResult> GetClientesNuevosReporte(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var fechaInicioReal = fechaInicio ?? DateTime.UtcNow.AddDays(-30);
            var fechaFinReal = fechaFin ?? DateTime.UtcNow;

            var clientes = await _clienteService.GetClientesNuevosPorFechaAsync(fechaInicioReal, fechaFinReal);
            return Ok(new { success = true, data = clientes });
        }

        // GET: /api/clientes/reporte/exportar?fechaInicio=2024-01-01&fechaFin=2024-12-31
        [HttpGet("/api/clientes/reporte/exportar")]
        public async Task<IActionResult> ExportarClientesNuevos(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var fechaInicioReal = fechaInicio ?? DateTime.UtcNow.AddDays(-30);
            var fechaFinReal = fechaFin ?? DateTime.UtcNow;

            var clientes = await _clienteService.GetClientesNuevosPorFechaAsync(fechaInicioReal, fechaFinReal);

            // Generar CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Nombre,Email,Teléfono,Fecha Registro,Total Citas,Total Gastado");

            foreach (var cliente in clientes)
            {
                csv.AppendLine($"\"{cliente.Id}\",\"{EscapeCsv(cliente.Nombre)}\",\"{cliente.Email ?? ""}\",\"{cliente.Telefono}\",\"{cliente.FechaRegistro:yyyy-MM-dd HH:mm}\",{cliente.TotalCitas},\"{cliente.TotalGastado:F2}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var fechaStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            return File(bytes, "text/csv", $"clientes_nuevos_{fechaStr}.csv");
        }
        // GET: /api/clientes/reporte/exportar-vcf
        [HttpGet("/api/clientes/reporte/exportar-vcf")]
        public async Task<IActionResult> ExportarClientesVCF(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var fechaInicioReal = fechaInicio ?? DateTime.UtcNow.AddDays(-30);
            var fechaFinReal = fechaFin ?? DateTime.UtcNow;

            var clientes = await _clienteService.GetClientesNuevosPorFechaAsync(fechaInicioReal, fechaFinReal);

            // Generar archivo VCF
            var vcf = new System.Text.StringBuilder();

            foreach (var cliente in clientes)
            {
                vcf.AppendLine("BEGIN:VCARD");
                vcf.AppendLine("VERSION:3.0");
                vcf.AppendLine($"FN:{cliente.Nombre}");
                vcf.AppendLine($"N:{cliente.Nombre};;;");
                vcf.AppendLine($"TEL:{cliente.Telefono}");
                if (!string.IsNullOrEmpty(cliente.Email))
                    vcf.AppendLine($"EMAIL:{cliente.Email}");
                vcf.AppendLine($"NOTE:Cliente desde {cliente.FechaRegistro:yyyy-MM-dd} | ${cliente.TotalGastado:F2} gastado | {cliente.TotalCitas} citas");
                vcf.AppendLine("END:VCARD");
                vcf.AppendLine();
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(vcf.ToString());
            var fechaStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            return File(bytes, "text/vcard", $"clientes_contactos_{fechaStr}.vcf");
        }
        private string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            // Reemplazar comillas dobles con comillas dobles escapadas
            return text.Replace("\"", "\"\"");
        }
    }
}
using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InkManager.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: /dashboard - Muestra la vista
        [HttpGet("/dashboard")]
        public IActionResult Index()
        {
            return View("~/Views/Dashboard/Index.cshtml");
        }

        // GET: /api/dashboard/resumen
        [HttpGet("/api/dashboard/resumen")]
        public async Task<IActionResult> GetResumen(int? artistaId = null, int? estudioId = null)
        {
            var resumen = await _dashboardService.GetResumenAsync(artistaId, estudioId);
            return Ok(new { success = true, data = resumen });
        }

        // GET: /api/dashboard/proximas-citas
        [HttpGet("/api/dashboard/proximas-citas")]
        public async Task<IActionResult> GetProximasCitas(int? artistaId = null, int dias = 7)
        {
            var citas = await _dashboardService.GetProximasCitasAsync(artistaId, dias);
            return Ok(new { success = true, data = citas });
        }

        // GET: /api/dashboard/exportar
        [HttpGet("/api/dashboard/exportar")]
        public async Task<IActionResult> ExportarReporte(DateTime fechaInicio, DateTime fechaFin, int? artistaId = null)
        {
            var reporte = await _dashboardService.ExportarReporteCitasAsync(fechaInicio, fechaFin, artistaId);
            return File(reporte, "text/csv", $"reporte_citas_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
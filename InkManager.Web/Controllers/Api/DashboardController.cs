using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InkManager.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            return View("~/Views/Dashboard/Index.cshtml");
        }
        // GET: api/dashboard/resumen?artistaId=1
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen(int? artistaId = null, int? estudioId = null)
        {
            var resumen = await _dashboardService.GetResumenAsync(artistaId, estudioId);
            return Ok(new { success = true, data = resumen });
        }

        // GET: api/dashboard/proximas-citas?artistaId=1&dias=7
        [HttpGet("proximas-citas")]
        public async Task<IActionResult> GetProximasCitas(int? artistaId = null, int dias = 7)
        {
            var citas = await _dashboardService.GetProximasCitasAsync(artistaId, dias);
            return Ok(new { success = true, data = citas });
        }

        // GET: api/dashboard/exportar?fechaInicio=2024-01-01&fechaFin=2024-12-31&artistaId=1
        [HttpGet("exportar")]
        public async Task<IActionResult> ExportarReporte(DateTime fechaInicio, DateTime fechaFin, int? artistaId = null)
        {
            var reporte = await _dashboardService.ExportarReporteCitasAsync(fechaInicio, fechaFin, artistaId);
            return File(reporte, "text/csv", $"reporte_citas_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
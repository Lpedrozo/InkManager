using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InkManager.Web.Controllers
{
    [Authorize]
    [Route("configuracion")]
    public class ConfiguracionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ConfiguracionController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            ViewBag.UsuarioId = usuarioId;
            ViewBag.Rol = rol;

            // Obtener calendarios del usuario
            var calendarios = await _context.Calendarios
                .Where(c => c.EntidadId == usuarioId && c.Tipo == "artista" && !c.EliminadoLogico)
                .FirstOrDefaultAsync();

            ViewBag.TieneCalendarConectado = calendarios?.IsSynced == true;
            ViewBag.CalendarioId = calendarios?.Id;

            return View();
        }

        [HttpGet("calendario/conectar")]
        public async Task<IActionResult> ConectarCalendar()
        {
            var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Obtener o crear el calendario del usuario
            var calendario = await _context.Calendarios
                .FirstOrDefaultAsync(c => c.Tipo == "artista" && c.EntidadId == usuarioId && !c.EliminadoLogico);

            if (calendario == null)
            {
                calendario = new Calendario
                {
                    Tipo = "artista",
                    EntidadId = usuarioId,
                    Nombre = "Mi Calendario",
                    EsPrincipal = true,
                    Color = "#3B82F6",
                    Activo = true
                };
                _context.Calendarios.Add(calendario);
                await _context.SaveChangesAsync();
            }

            // 🔧 USAR LAS CREDENCIALES DEL APPSETTINGS.JSON
            var clientId = _configuration["GoogleCalendar:ClientId"];
            var redirectUri = _configuration["GoogleCalendar:RedirectUri"];

            var state = $"{calendario.Id}_{Guid.NewGuid()}";
            var url = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                      $"client_id={Uri.EscapeDataString(clientId)}&" +
                      $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                      $"response_type=code&" +
                      $"scope={Uri.EscapeDataString("https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/calendar.events")}&" +
                      $"access_type=offline&" +
                      $"prompt=consent&" +
                      $"state={state}";

            return Redirect(url);
        }
    }
}
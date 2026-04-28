using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InkManager.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CalendarController : ControllerBase
    {
        private readonly IGoogleCalendarService _googleCalendarService;

        public CalendarController(IGoogleCalendarService googleCalendarService)
        {
            _googleCalendarService = googleCalendarService;
        }

        [HttpGet("redirect")]
        public async Task<IActionResult> ConnectRedirect([FromQuery] string? code, [FromQuery] string? state)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                return BadRequest("Faltan parámetros");

            var parts = state.Split('_');
            if (parts.Length < 1 || !int.TryParse(parts[0], out var calendarioId))
                return BadRequest("State inválido");

            var success = await _googleCalendarService.HandleAuthCallbackAsync(calendarioId, code);

            // Guardar el resultado en una cookie temporal
            if (success)
            {
                Response.Cookies.Append("calendar_connected", "success", new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    HttpOnly = true
                });
            }
            else
            {
                Response.Cookies.Append("calendar_connected", "error", new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    HttpOnly = true
                });
            }

            // Redirigir a login, después de login se mostrará el mensaje
            return Redirect($"/login?redirectAfter=/configuracion");
        }

        [HttpGet("auth-url/{calendarioId}")]
        [Authorize]
        public async Task<IActionResult> GetAuthUrl(int calendarioId)
        {
            var url = await _googleCalendarService.GetAuthUrlAsync(calendarioId);
            return Ok(new { url });
        }

        [HttpPost("sync/{citaId}")]
        [Authorize]
        public async Task<IActionResult> SyncCita(int citaId)
        {
            var success = await _googleCalendarService.SyncCitaConGoogleAsync(citaId);
            return Ok(new { success, message = success ? "Sincronizado correctamente" : "Error al sincronizar" });
        }
    }
}
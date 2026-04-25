using InkManager.Core.DTOs;
using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InkManager.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /login - Muestra la vista de login
        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View();
        }

        // GET: / - Redirige a login
        [HttpGet("/")]
        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // POST: /api/auth/login
        [HttpPost("/api/auth/login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, data = result });
        }

        // POST: /api/auth/select-role-estudio
        [HttpPost("/api/auth/select-role-estudio")]
        public async Task<IActionResult> ApiSelectRolAndEstudio([FromBody] SelectRolEstudioDto dto)
        {
            try
            {
                var session = await _authService.SelectRolAndEstudioAsync(dto);
                var token = _authService.GenerateJwtToken(session);

                return Ok(new
                {
                    success = true,
                    token = token,
                    session = session,
                    message = "Sesión iniciada correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
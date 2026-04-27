using System.Security.Claims;
using InkManager.Core.DTOs;
using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpGet("/login")]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpGet("/")]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
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

            // Si ya tiene claims (login directo), crear cookie
            if (result.Claims != null && result.Claims.Any())
            {
                var identity = new ClaimsIdentity(result.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = dto.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(dto.RememberMe ? 7 : 1)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                return Ok(new { success = true, data = result, redirectUrl = "/dashboard" });
            }

            // Si necesita selección, devolver opciones
            return Ok(new { success = true, data = result });
        }

        [AllowAnonymous]
        [HttpPost("/api/auth/select-role-estudio")]
        public async Task<IActionResult> ApiSelectRolAndEstudio([FromBody] SelectRolEstudioDto dto)
        {
            try
            {
                var result = await _authService.SelectRolAndEstudioAsync(dto);

                if (!result.Success || result.Claims == null)
                {
                    return BadRequest(new { success = false, message = result.Message ?? "Error al seleccionar" });
                }

                var identity = new ClaimsIdentity(result.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = dto.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(dto.RememberMe ? 7 : 1)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                return Ok(new { success = true, message = result.Message, redirectUrl = "/dashboard" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("/api/auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, message = "Sesión cerrada correctamente" });
        }

        [AllowAnonymous]
        [HttpGet("/logout")]
        public async Task<IActionResult> LogoutPage()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
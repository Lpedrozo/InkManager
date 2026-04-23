using Microsoft.AspNetCore.Mvc;

namespace InkManager.Web.Controllers
{
    [Route("citas")]
    public class CitasController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Mis Citas";
            return View("~/Views/Citas/Index.cshtml");
        }

        [HttpGet("detalle/{id}")]
        public IActionResult Detalle(int id)
        {
            ViewData["Title"] = "Detalle de Cita";
            ViewBag.CitaId = id;
            return View("~/Views/Citas/Detalle.cshtml");
        }

        [HttpGet("crear")]
        public IActionResult Crear()
        {
            ViewData["Title"] = "Nueva Cita";
            return View();
        }

        [HttpGet("editar/{id}")]
        public IActionResult Editar(int id)
        {
            ViewData["Title"] = "Editar Cita";
            ViewBag.CitaId = id;
            return View();
        }
    }
}
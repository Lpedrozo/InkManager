using InkManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InkManager.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EstudiosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstudiosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEstudios()
        {
            var estudios = await _context.Estudios
                .Where(e => !e.EliminadoLogico)
                .Select(e => new { e.Id, e.Nombre, e.Direccion })
                .ToListAsync();

            return Ok(new { success = true, data = estudios });
        }
    }
}
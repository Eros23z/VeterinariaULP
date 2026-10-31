using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers
{
    public class ServiciosController : Controller
    {
        private readonly DataContext _context;

        public ServiciosController(DataContext context)
        {
            _context = context;
        }

        // GET: Servicios
        public IActionResult Index()
        {
            return View();
        }

        // GET: Servicios/ListarJson
        [HttpGet]
        public IActionResult ListarJson()
        {
            var servicios = _context.Servicios
                .AsNoTracking()
                .OrderBy(s => s.Descripcion)
                .ToList();

            return Json(servicios);
        }

        // GET: Servicios/ObtenerJson/5
        [HttpGet]
        public IActionResult ObtenerJson(int id)
        {
            var servicio = _context.Servicios.AsNoTracking().FirstOrDefault(s => s.IdServicio == id);
            if (servicio == null) return NotFound(new { mensaje = "Servicio no encontrado." });

            return Json(servicio);
        }

        // POST: Servicios/CrearJson
        [HttpPost]
        public IActionResult CrearJson([FromBody] Servicio servicio)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { errores });
            }

            bool duplicado = _context.Servicios.Any(s => s.Descripcion.ToLower() == servicio.Descripcion.Trim().ToLower());
            if (duplicado)
            {
                return BadRequest(new { errores = new[] { "Ya existe un servicio registrado con este nombre." } });
            }

            servicio.Descripcion = servicio.Descripcion.Trim();
            _context.Servicios.Add(servicio);
            _context.SaveChanges();

            return Ok(servicio);
        }

        // POST: Servicios/ModificarJson
        [HttpPost]
        public IActionResult ModificarJson([FromBody] Servicio servicio)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { errores });
            }

            var entidad = _context.Servicios.Find(servicio.IdServicio);
            if (entidad == null) return NotFound(new { mensaje = "El servicio no existe." });

            bool duplicado = _context.Servicios.Any(s => s.Descripcion.ToLower() == servicio.Descripcion.Trim().ToLower() && s.IdServicio != servicio.IdServicio);
            if (duplicado)
            {
                return BadRequest(new { errores = new[] { "Ya existe otro servicio con esa misma descripción." } });
            }

            entidad.Descripcion = servicio.Descripcion.Trim();
            entidad.PrecioBase = servicio.PrecioBase;
            entidad.DuracionMinutos = servicio.DuracionMinutos;
            entidad.Activo = servicio.Activo;

            _context.SaveChanges();
            return Ok(entidad);
        }

        // POST: Servicios/EliminarJson/5
        [HttpPost]
        public IActionResult EliminarJson(int id)
        {
            var servicio = _context.Servicios.Find(id);
            if (servicio == null) return NotFound(new { mensaje = "El servicio no existe." });

            try
            {
                _context.Servicios.Remove(servicio);
                _context.SaveChanges();
                return Ok(new { mensaje = "Servicio eliminado correctamente." });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el servicio porque ya se encuentra vinculado a turnos médicos agendados." });
            }
        }
    }
}
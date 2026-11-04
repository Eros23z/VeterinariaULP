using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MascotasApiController : ControllerBase
    {
        private readonly DataContext _context;

        public MascotasApiController(DataContext context)
        {
            _context = context;
        }

        // GET: api/MascotasApi
        [HttpGet]
        public async Task<IActionResult> GetMascotas()
        {
            var mascotas = await _context.Mascotas
                .Include(m => m.Especie)
                .Include(m => m.Dueño)
                .AsNoTracking()
                .Where(m => m.Activo)
                .Select(m => new
                {
                    m.IdMascota,
                    m.Nombre,
                    m.Raza,
                    m.Sexo,
                    m.FechaNacimiento,
                    m.Foto,
                    Especie = m.Especie!.Nombre,
                    Dueño = new
                    {
                        m.Dueño!.IdDueño,
                        m.Dueño.NombreCompleto,
                        m.Dueño.Telefono,
                        m.Dueño.Dni
                    }
                })
                .ToListAsync();

            return Ok(mascotas);
        }

        // GET: api/MascotasApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMascota(int id)
        {
            var m = await _context.Mascotas
                .Include(x => x.Especie)
                .Include(x => x.Dueño)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdMascota == id);

            if (m == null) return NotFound(new { mensaje = "Mascota no encontrada." });

            return Ok(new
            {
                m.IdMascota,
                m.Nombre,
                m.Raza,
                m.Sexo,
                m.FechaNacimiento,
                m.Foto,
                m.Observaciones,
                Especie = m.Especie?.Nombre,
                Dueño = m.Dueño?.NombreCompleto,
                TelefonoDueño = m.Dueño?.Telefono
            });
        }

        // GET: api/MascotasApi/TurnosMascota/5
        [HttpGet("TurnosMascota/{id}")]
        public async Task<IActionResult> GetTurnosMascota(int id)
        {
            var turnos = await _context.Turnos
                .Include(t => t.Servicio)
                .Include(t => t.Veterinario)
                .Where(t => t.IdMascota == id)
                .OrderByDescending(t => t.FechaHora)
                .Select(t => new
                {
                    t.IdTurno,
                    t.FechaHora,
                    t.Motivo,
                    t.Estado,
                    Servicio = t.Servicio!.Descripcion,
                    Veterinario = t.Veterinario!.NombreCompleto
                })
                .ToListAsync();

            return Ok(turnos);
        }
    }
}
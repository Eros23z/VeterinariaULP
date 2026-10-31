using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers
{
    public class AtencionesController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public AtencionesController(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Atenciones (Listado general de consultas con paginación)
        public IActionResult Index(string? search, int page = 1, int pageSize = 10)
        {
            var query = _context.AtencionesClinicas
                .Include(a => a.Turno)!.ThenInclude(t => t.Mascota)!.ThenInclude(m => m.Dueño)
                .Include(a => a.Turno)!.ThenInclude(t => t.Servicio)
                .Include(a => a.Turno)!.ThenInclude(t => t.Veterinario)
                .Include(a => a.Pago)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.Turno!.Mascota!.Nombre.Contains(search) ||
                                         a.Turno!.Mascota!.Dueño!.Apellido.Contains(search) ||
                                         a.Turno!.Mascota!.Dueño!.Dni.Contains(search) ||
                                         a.Diagnostico.Contains(search));
            }

            int totalItems = query.Count();

            var items = query
                .OrderByDescending(a => a.FechaAtencion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Atenciones/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var atencion = _context.AtencionesClinicas
                .Include(a => a.Turno)!.ThenInclude(t => t.Mascota)!.ThenInclude(m => m.Dueño)
                .Include(a => a.Turno)!.ThenInclude(t => t.Mascota)!.ThenInclude(m => m.Especie)
                .Include(a => a.Turno)!.ThenInclude(t => t.Servicio)
                .Include(a => a.Turno)!.ThenInclude(t => t.Veterinario)
                .Include(a => a.Pago)!.ThenInclude(p => p.UsuarioCrea)
                .AsNoTracking()
                .FirstOrDefault(a => a.IdAtencion == id);

            if (atencion == null) return NotFound();

            return View(atencion);
        }

        // GET: Atenciones/Registrar?idTurno=5
        public IActionResult Registrar(int idTurno)
        {
            var turno = _context.Turnos
                .Include(t => t.Mascota)!.ThenInclude(m => m.Dueño)
                .Include(t => t.Servicio)
                .Include(t => t.Veterinario)
                .FirstOrDefault(t => t.IdTurno == idTurno);

            if (turno == null) return NotFound();

            // verifico si el turno ya fue atendido
            bool yaAtendido = _context.AtencionesClinicas.Any(a => a.IdTurno == idTurno);
            if (yaAtendido)
            {
                TempData["Error"] = "Este turno ya cuenta con una atención médica registrada.";
                return RedirectToAction("Details", "Turnos", new { id = idTurno });
            }

            var atencion = new AtencionClinica
            {
                IdTurno = idTurno,
                Turno = turno,
                FechaAtencion = DateTime.Now
            };

            ViewBag.ImporteSugerido = turno.Servicio?.PrecioBase ?? 0;
            return View(atencion);
        }

        // POST: Atenciones/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(AtencionClinica atencion, IFormFile? archivoEstudioFile, decimal importeCobrado, string metodoPago)
        {
            var turno = await _context.Turnos
                .Include(t => t.Mascota)
                .Include(t => t.Servicio)
                .FirstOrDefaultAsync(t => t.IdTurno == atencion.IdTurno);

            if (turno == null) return NotFound();

            if (ModelState.IsValid)
            {
                using var transaccion = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Procesar archivo de estudio 
                    if (archivoEstudioFile != null && archivoEstudioFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "estudios");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(archivoEstudioFile.FileName)}";
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await archivoEstudioFile.CopyToAsync(stream);
                        }
                        atencion.ArchivoEstudio = $"/uploads/estudios/{fileName}";
                    }

                    // Guardar atención médica
                    _context.AtencionesClinicas.Add(atencion);
                    await _context.SaveChangesAsync();

                    turno.Estado = "Atendido";
                    _context.Turnos.Update(turno);

                    // Crear comprobante de pago vinculado
                    var pago = new Pago
                    {
                        IdAtencion = atencion.IdAtencion,
                        FechaPago = DateTime.Now,
                        Concepto = $"Atención: {turno.Servicio?.Descripcion} - Paciente: {turno.Mascota?.Nombre}",
                        Importe = importeCobrado > 0 ? importeCobrado : (turno.Servicio?.PrecioBase ?? 0),
                        MetodoPago = string.IsNullOrEmpty(metodoPago) ? "Efectivo" : metodoPago,
                        Anulado = false,
                        UsuarioCreaId = 1
                    };
                    _context.Pagos.Add(pago);

                    await _context.SaveChangesAsync();
                    await transaccion.CommitAsync();

                    TempData["Success"] = "Atención médica e ingreso de cobro asentados con éxito.";
                    return RedirectToAction(nameof(Details), new { id = atencion.IdAtencion });
                }
                catch (Exception)
                {
                    await transaccion.RollbackAsync();
                    TempData["Error"] = "Ocurrió un error al procesar la atención clínica.";
                }
            }

            atencion.Turno = turno;
            ViewBag.ImporteSugerido = importeCobrado;
            return View(atencion);
        }

        // GET: Atenciones/HistorialMascota/5
        public IActionResult HistorialMascota(int id)
        {
            var mascota = _context.Mascotas
                .Include(m => m.Dueño)
                .Include(m => m.Especie)
                .AsNoTracking()
                .FirstOrDefault(m => m.IdMascota == id);

            if (mascota == null) return NotFound();

            var historial = _context.AtencionesClinicas
                .Include(a => a.Turno)!.ThenInclude(t => t.Servicio)
                .Include(a => a.Turno)!.ThenInclude(t => t.Veterinario)
                .Include(a => a.Pago)
                .Where(a => a.Turno!.IdMascota == id)
                .OrderByDescending(a => a.FechaAtencion)
                .AsNoTracking()
                .ToList();

            ViewBag.Mascota = mascota;
            return View(historial);
        }
    }
}
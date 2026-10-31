using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers
{
    public class TurnosController : Controller
    {
        private readonly DataContext _context;

        public TurnosController(DataContext context)
        {
            _context = context;
        }

        // GET: Turnos (Paginación por servidor y filtros combinados)
        public IActionResult Index(string? search, string? estado, int? idVeterinario, DateTime? fecha, int page = 1, int pageSize = 10)
        {
            var query = _context.Turnos
                .Include(t => t.Mascota).ThenInclude(m => m!.Dueño)
                .Include(t => t.Servicio)
                .Include(t => t.Veterinario)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Mascota!.Nombre.Contains(search) ||
                                         t.Mascota!.Dueño!.Apellido.Contains(search) ||
                                         t.Mascota!.Dueño!.Dni.Contains(search) ||
                                         t.Motivo.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(t => t.Estado == estado);
            }

            if (idVeterinario.HasValue && idVeterinario.Value > 0)
            {
                query = query.Where(t => t.IdVeterinario == idVeterinario.Value);
            }

            if (fecha.HasValue)
            {
                query = query.Where(t => t.FechaHora.Date == fecha.Value.Date);
            }

            int totalItems = query.Count();

            var items = query
                .OrderByDescending(t => t.FechaHora)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.Estado = estado;
            ViewBag.IdVeterinario = idVeterinario;
            ViewBag.Fecha = fecha?.ToString("yyyy-MM-dd");

            CargarCombosFiltro(idVeterinario);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Turnos/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var turno = _context.Turnos
                .Include(t => t.Mascota).ThenInclude(m => m!.Dueño)
                .Include(t => t.Mascota).ThenInclude(m => m!.Especie)
                .Include(t => t.Servicio)
                .Include(t => t.Veterinario)
                .AsNoTracking()
                .FirstOrDefault(t => t.IdTurno == id);

            if (turno == null) return NotFound();

            return View(turno);
        }

        // GET: Turnos/Create
        public IActionResult Create(int? idMascota)
        {
            CargarListasDesplegables(null, null);

            var turno = new Turno
            {
                FechaHora = DateTime.Today.AddDays(1).AddHours(10),
                Estado = "Pendiente"
            };

            if (idMascota.HasValue && idMascota.Value > 0)
            {
                var mascota = _context.Mascotas.Include(m => m.Dueño).FirstOrDefault(m => m.IdMascota == idMascota.Value);
                if (mascota != null)
                {
                    turno.IdMascota = mascota.IdMascota;
                    ViewBag.MascotaPreseleccionada = $"{mascota.Nombre} (Dueño: {mascota.Dueño?.NombreCompleto})";
                    ViewBag.IdDueñoPreseleccionado = mascota.IdDueño;
                }
            }

            return View(turno);
        }

        // POST: Turnos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Turno turno)
        {
            var servicio = _context.Servicios.Find(turno.IdServicio);

            if (servicio == null)
            {
                ModelState.AddModelError("IdServicio", "El servicio seleccionado no es válido.");
            }
            else
            {
                // Validación de solapamiento de turnos
                DateTime inicioNuevo = turno.FechaHora;
                DateTime finNuevo = inicioNuevo.AddMinutes(servicio.DuracionMinutos);

                if (HaySolapamiento(turno.IdVeterinario, inicioNuevo, finNuevo, null))
                {
                    ModelState.AddModelError("FechaHora", "El profesional seleccionado ya cuenta con un turno agendado que se superpone en esa franja horaria.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Turnos.Add(turno);
                _context.SaveChanges();
                TempData["Success"] = "Turno agendado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarListasDesplegables(turno.IdServicio, turno.IdVeterinario);
            if (turno.IdMascota > 0)
            {
                var m = _context.Mascotas.Include(x => x.Dueño).FirstOrDefault(x => x.IdMascota == turno.IdMascota);
                ViewBag.MascotaPreseleccionada = $"{m?.Nombre} (Dueño: {m?.Dueño?.NombreCompleto})";
                ViewBag.IdDueñoPreseleccionado = m?.IdDueño;
            }
            return View(turno);
        }

        // GET: Turnos/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var turno = _context.Turnos
                .Include(t => t.Mascota).ThenInclude(m => m!.Dueño)
                .FirstOrDefault(t => t.IdTurno == id);

            if (turno == null) return NotFound();

            CargarListasDesplegables(turno.IdServicio, turno.IdVeterinario);
            ViewBag.MascotaNombre = $"{turno.Mascota?.Nombre} (Dueño: {turno.Mascota?.Dueño?.NombreCompleto})";
            return View(turno);
        }

        // POST: Turnos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Turno turno)
        {
            if (id != turno.IdTurno) return NotFound();

            var servicio = _context.Servicios.Find(turno.IdServicio);
            if (servicio == null)
            {
                ModelState.AddModelError("IdServicio", "Servicio no válido.");
            }
            else
            {
                DateTime inicioNuevo = turno.FechaHora;
                DateTime finNuevo = inicioNuevo.AddMinutes(servicio.DuracionMinutos);

                if (HaySolapamiento(turno.IdVeterinario, inicioNuevo, finNuevo, id))
                {
                    ModelState.AddModelError("FechaHora", "El turno colisiona con otra cita programada para el mismo veterinario.");
                }
            }

            if (ModelState.IsValid)
            {
                var turnoDb = _context.Turnos.Find(id);
                if (turnoDb == null) return NotFound();

                turnoDb.FechaHora = turno.FechaHora;
                turnoDb.Motivo = turno.Motivo;
                turnoDb.Estado = turno.Estado;
                turnoDb.IdServicio = turno.IdServicio;
                turnoDb.IdVeterinario = turno.IdVeterinario;

                _context.SaveChanges();
                TempData["Success"] = "Turno modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarListasDesplegables(turno.IdServicio, turno.IdVeterinario);
            return View(turno);
        }

        // POST: Cancelar Turno Directo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancelar(int id)
        {
            var turno = _context.Turnos.Find(id);
            if (turno != null)
            {
                turno.Estado = "Cancelado";
                _context.SaveChanges();
                TempData["Success"] = $"El Turno #{id} ha sido cancelado.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Turnos/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var turno = _context.Turnos
                .Include(t => t.Mascota).ThenInclude(m => m!.Dueño)
                .Include(t => t.Servicio)
                .Include(t => t.Veterinario)
                .AsNoTracking()
                .FirstOrDefault(t => t.IdTurno == id);

            if (turno == null) return NotFound();

            return View(turno);
        }

        // POST: Turnos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var turno = _context.Turnos.Find(id);
            if (turno == null) return NotFound();

            try
            {
                _context.Turnos.Remove(turno);
                _context.SaveChanges();
                TempData["Success"] = "Turno eliminado del registro.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se puede eliminar el turno porque posee atenciones clínicas o pagos registrados.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Control de solapamiento de agenda
        private bool HaySolapamiento(int idVeterinario, DateTime inicioNuevo, DateTime finNuevo, int? idTurnoExcluir)
        {
            var turnosExistentes = _context.Turnos
                .Include(t => t.Servicio)
                .Where(t => t.IdVeterinario == idVeterinario &&
                            t.Estado != "Cancelado" &&
                            t.FechaHora.Date == inicioNuevo.Date &&
                            (!idTurnoExcluir.HasValue || t.IdTurno != idTurnoExcluir.Value))
                .ToList();

            foreach (var t in turnosExistentes)
            {
                DateTime inicioExistente = t.FechaHora;
                DateTime finExistente = inicioExistente.AddMinutes(t.Servicio?.DuracionMinutos ?? 30);

                // Condición de colisión: (InicioA < FinB) && (FinA > InicioB)
                if (inicioNuevo < finExistente && finNuevo > inicioExistente)
                {
                    return true;
                }
            }

            return false;
        }

        private void CargarListasDesplegables(object? servicioSel, object? vetSel)
        {
            ViewBag.IdServicio = new SelectList(_context.Servicios.Where(s => s.Activo).OrderBy(s => s.Descripcion).ToList(), "IdServicio", "Descripcion", servicioSel);
            ViewBag.IdVeterinario = new SelectList(_context.Usuarios.Where(u => u.Rol == "Veterinario" && u.Estado).OrderBy(u => u.Apellido).ToList(), "IdUsuario", "NombreCompleto", vetSel);
        }

        private void CargarCombosFiltro(object? vetSel)
        {
            ViewBag.Veterinarios = new SelectList(_context.Usuarios.Where(u => u.Rol == "Veterinario" && u.Estado).OrderBy(u => u.Apellido).ToList(), "IdUsuario", "NombreCompleto", vetSel);
        }
    }
}
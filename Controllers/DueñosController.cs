using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers
{
    public class DueñosController : Controller
    {
        private readonly DataContext _context;

        public DueñosController(DataContext context)
        {
            _context = context;
        }

        // GET: Dueños (Con Paginado por Servidor y Búsqueda)
        public IActionResult Index(string? search, int page = 1, int pageSize = 10)
        {
            var query = _context.Dueños.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Dni.Contains(search) ||
                                         d.Apellido.Contains(search) ||
                                         d.Nombre.Contains(search) ||
                                         d.Email.Contains(search));
            }

            int totalItems = query.Count();

            var items = query
                .OrderBy(d => d.Apellido)
                .ThenBy(d => d.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Dueños/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var dueño = _context.Dueños.AsNoTracking().FirstOrDefault(d => d.IdDueño == id);
            if (dueño == null) return NotFound();

            return View(dueño);
        }

        // GET: Dueños/Create
        public IActionResult Create()
        {
            return View(new Dueño { Estado = true });
        }

        // POST: Dueños/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("IdDueño,Dni,Nombre,Apellido,Telefono,Email,Direccion,Estado")] Dueño dueño)
        {
            if (ModelState.IsValid)
            {
                bool dniExiste = _context.Dueños.Any(d => d.Dni == dueño.Dni);
                if (dniExiste)
                {
                    ModelState.AddModelError("Dni", "Ya existe un dueño registrado con este número de DNI.");
                    return View(dueño);
                }

                _context.Dueños.Add(dueño);
                _context.SaveChanges();
                TempData["Success"] = "Dueño registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(dueño);
        }

        // GET: Dueños/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var dueño = _context.Dueños.Find(id);
            if (dueño == null) return NotFound();

            return View(dueño);
        }

        // POST: Dueños/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("IdDueño,Dni,Nombre,Apellido,Telefono,Email,Direccion,Estado")] Dueño dueño)
        {
            if (id != dueño.IdDueño) return NotFound();

            if (ModelState.IsValid)
            {
                bool dniDuplicado = _context.Dueños.Any(d => d.Dni == dueño.Dni && d.IdDueño != id);
                if (dniDuplicado)
                {
                    ModelState.AddModelError("Dni", "El DNI ingresado ya pertenece a otro dueño.");
                    return View(dueño);
                }

                _context.Dueños.Update(dueño);
                _context.SaveChanges();
                TempData["Success"] = "Datos del dueño actualizados correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(dueño);
        }

        // GET: Dueños/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var dueño = _context.Dueños.AsNoTracking().FirstOrDefault(d => d.IdDueño == id);
            if (dueño == null) return NotFound();

            return View(dueño);
        }

        // POST: Dueños/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var dueño = _context.Dueños.Find(id);
                if (dueño != null)
                {
                    _context.Dueños.Remove(dueño);
                    _context.SaveChanges();
                    TempData["Success"] = "Dueño eliminado correctamente.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se puede eliminar el dueño porque posee mascotas asociadas en el sistema.";
            }
            return RedirectToAction(nameof(Index));
        }

        // REQUERIMIENTO 6: Endpoint para Búsqueda Asíncrona (AJAX) en formularios
        [HttpGet]
        public IActionResult BuscarAjax(string? q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Json(new List<object>());
            }

            var resultados = _context.Dueños
                .AsNoTracking()
                .Where(d => d.Estado && (d.Dni.Contains(q) || d.Apellido.Contains(q) || d.Nombre.Contains(q)))
                .OrderBy(d => d.Apellido)
                .Take(8)
                .Select(d => new
                {
                    idDueño = d.IdDueño,
                    dni = d.Dni,
                    nombreCompleto = $"{d.Apellido}, {d.Nombre}",
                    telefono = d.Telefono
                })
                .ToList();

            return Json(resultados);
        }
    }
}
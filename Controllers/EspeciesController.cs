using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers
{
    public class EspeciesController : Controller
    {
        private readonly DataContext _context;

        public EspeciesController(DataContext context)
        {
            _context = context;
        }

        // GET: Especies
        public IActionResult Index(string? search)
        {
            var query = _context.Especies.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Nombre.Contains(search));
            }

            ViewBag.Search = search;
            var lista = query.OrderBy(e => e.Nombre).ToList();
            return View(lista);
        }

        // GET: Especies/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var especie = _context.Especies.AsNoTracking().FirstOrDefault(e => e.IdEspecie == id);
            if (especie == null) return NotFound();

            return View(especie);
        }

        // GET: Especies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Especies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("IdEspecie,Nombre")] Especie especie)
        {
            if (ModelState.IsValid)
            {
                bool existe = _context.Especies.Any(e => e.Nombre.ToLower() == especie.Nombre.Trim().ToLower());
                if (existe)
                {
                    ModelState.AddModelError("Nombre", "Ya existe una especie registrada con este nombre.");
                    return View(especie);
                }

                _context.Especies.Add(especie);
                _context.SaveChanges();
                TempData["Success"] = "Especie registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(especie);
        }

        // GET: Especies/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var especie = _context.Especies.Find(id);
            if (especie == null) return NotFound();

            return View(especie);
        }

        // POST: Especies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("IdEspecie,Nombre")] Especie especie)
        {
            if (id != especie.IdEspecie) return NotFound();

            if (ModelState.IsValid)
            {
                bool duplicado = _context.Especies.Any(e => e.Nombre.ToLower() == especie.Nombre.Trim().ToLower() && e.IdEspecie != id);
                if (duplicado)
                {
                    ModelState.AddModelError("Nombre", "Ya existe otra especie con este nombre.");
                    return View(especie);
                }

                _context.Especies.Update(especie);
                _context.SaveChanges();
                TempData["Success"] = "Especie actualizada con éxito.";
                return RedirectToAction(nameof(Index));
            }
            return View(especie);
        }

        // GET: Especies/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var especie = _context.Especies.AsNoTracking().FirstOrDefault(e => e.IdEspecie == id);
            if (especie == null) return NotFound();

            return View(especie);
        }

        // POST: Especies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var especie = _context.Especies.Find(id);
                if (especie != null)
                {
                    _context.Especies.Remove(especie);
                    _context.SaveChanges();
                    TempData["Success"] = "Especie eliminada correctamente.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se puede eliminar la especie porque existen mascotas asociadas a ella.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
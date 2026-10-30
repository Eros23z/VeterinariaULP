using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers
{
    public class MascotasController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public MascotasController(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Mascotas (Con Paginado por Servidor y Filtros)
        public IActionResult Index(string? search, int? idEspecie, int page = 1, int pageSize = 8)
        {
            var query = _context.Mascotas
                .Include(m => m.Especie)
                .Include(m => m.Dueño)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Nombre.Contains(search) ||
                                         m.Raza.Contains(search) ||
                                         m.Dueño!.Apellido.Contains(search) ||
                                         m.Dueño!.Dni.Contains(search));
            }

            if (idEspecie.HasValue && idEspecie.Value > 0)
            {
                query = query.Where(m => m.IdEspecie == idEspecie.Value);
            }

            int totalItems = query.Count();

            var items = query
                .OrderBy(m => m.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.IdEspecie = idEspecie;
            ViewBag.Especies = new SelectList(_context.Especies.OrderBy(e => e.Nombre).ToList(), "IdEspecie", "Nombre", idEspecie);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Mascotas/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var mascota = _context.Mascotas
                .Include(m => m.Especie)
                .Include(m => m.Dueño)
                .AsNoTracking()
                .FirstOrDefault(m => m.IdMascota == id);

            if (mascota == null) return NotFound();

            return View(mascota);
        }

        // GET: Mascotas/Create
        public IActionResult Create()
        {
            CargarCombosEspecies();
            return View(new Mascota { Activo = true, FechaNacimiento = DateTime.Today.AddYears(-1) });
        }

        // POST: Mascotas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mascota mascota, IFormFile? fotoFile)
        {
            if (ModelState.IsValid)
            {
                if (fotoFile != null && fotoFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "mascotas");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(fotoFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fotoFile.CopyToAsync(stream);
                    }
                    mascota.Foto = $"/uploads/mascotas/{fileName}";
                }

                _context.Mascotas.Add(mascota);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Mascota registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarCombosEspecies(mascota.IdEspecie);
            if (mascota.IdDueño > 0)
            {
                ViewBag.DueñoSeleccionado = _context.Dueños.Find(mascota.IdDueño)?.NombreCompleto;
            }
            return View(mascota);
        }

        // GET: Mascotas/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var mascota = _context.Mascotas
                .Include(m => m.Dueño)
                .FirstOrDefault(m => m.IdMascota == id);

            if (mascota == null) return NotFound();

            CargarCombosEspecies(mascota.IdEspecie);
            ViewBag.DueñoSeleccionado = mascota.Dueño?.NombreCompleto;
            return View(mascota);
        }

        // POST: Mascotas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Mascota mascota, IFormFile? fotoFile)
        {
            if (id != mascota.IdMascota) return NotFound();

            if (ModelState.IsValid)
            {
                var mascotaDb = await _context.Mascotas.FindAsync(id);
                if (mascotaDb == null) return NotFound();

                mascotaDb.Nombre = mascota.Nombre;
                mascotaDb.Raza = mascota.Raza;
                mascotaDb.Sexo = mascota.Sexo;
                mascotaDb.FechaNacimiento = mascota.FechaNacimiento;
                mascotaDb.Observaciones = mascota.Observaciones;
                mascotaDb.Activo = mascota.Activo;
                mascotaDb.IdEspecie = mascota.IdEspecie;
                mascotaDb.IdDueño = mascota.IdDueño;

                if (fotoFile != null && fotoFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "mascotas");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    // Borrar foto previa si existía
                    if (!string.IsNullOrEmpty(mascotaDb.Foto))
                    {
                        string rutaAntigua = Path.Combine(_environment.WebRootPath, mascotaDb.Foto.TrimStart('/'));
                        if (System.IO.File.Exists(rutaAntigua)) System.IO.File.Delete(rutaAntigua);
                    }

                    string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(fotoFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fotoFile.CopyToAsync(stream);
                    }
                    mascotaDb.Foto = $"/uploads/mascotas/{fileName}";
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Datos de la mascota actualizados con éxito.";
                return RedirectToAction(nameof(Index));
            }

            CargarCombosEspecies(mascota.IdEspecie);
            return View(mascota);
        }

        // GET: Mascotas/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var mascota = _context.Mascotas
                .Include(m => m.Especie)
                .Include(m => m.Dueño)
                .AsNoTracking()
                .FirstOrDefault(m => m.IdMascota == id);

            if (mascota == null) return NotFound();

            return View(mascota);
        }

        // POST: Mascotas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota == null) return NotFound();

            try
            {
                if (!string.IsNullOrEmpty(mascota.Foto))
                {
                    string rutaFoto = Path.Combine(_environment.WebRootPath, mascota.Foto.TrimStart('/'));
                    if (System.IO.File.Exists(rutaFoto)) System.IO.File.Delete(rutaFoto);
                }

                _context.Mascotas.Remove(mascota);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ficha de mascota eliminada correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se puede eliminar la mascota porque posee turnos e historiales médicos registrados.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Endpoint auxiliar para combos en cascada al agendar turnos
        [HttpGet]
        public IActionResult PorDueñoJson(int idDueño)
        {
            var mascotas = _context.Mascotas
                .AsNoTracking()
                .Where(m => m.IdDueño == idDueño && m.Activo)
                .Select(m => new { m.IdMascota, m.Nombre, m.Raza })
                .ToList();

            return Json(mascotas);
        }

        private void CargarCombosEspecies(object? seleccionado = null)
        {
            ViewBag.IdEspecie = new SelectList(_context.Especies.OrderBy(e => e.Nombre).ToList(), "IdEspecie", "Nombre", seleccionado);
        }
    }
}
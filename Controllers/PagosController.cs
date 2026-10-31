using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers
{
    public class PagosController : Controller
    {
        private readonly DataContext _context;

        public PagosController(DataContext context)
        {
            _context = context;
        }

        // GET: Pagos (Paginación por servidor y totales)
        public IActionResult Index(string? search, string? metodoPago, int page = 1, int pageSize = 10)
        {
            var query = _context.Pagos
                .Include(p => p.Atencion)!.ThenInclude(a => a.Turno)!.ThenInclude(t => t.Mascota)!.ThenInclude(m => m.Dueño)
                .Include(p => p.UsuarioCrea)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Concepto.Contains(search) ||
                                         p.Atencion!.Turno!.Mascota!.Nombre.Contains(search) ||
                                         p.Atencion!.Turno!.Mascota!.Dueño!.Apellido.Contains(search) ||
                                         p.Atencion!.Turno!.Mascota!.Dueño!.Dni.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(metodoPago))
            {
                query = query.Where(p => p.MetodoPago == metodoPago);
            }

            // Calculo recaudación activa 
            ViewBag.TotalRecaudado = query.Where(p => !p.Anulado).Sum(p => (decimal?)p.Importe) ?? 0m;

            int totalItems = query.Count();

            var items = query
                .OrderByDescending(p => p.FechaPago)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.MetodoPago = metodoPago;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Pagos/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var pago = _context.Pagos
                .Include(p => p.Atencion)!.ThenInclude(a => a.Turno)!.ThenInclude(t => t.Mascota)!.ThenInclude(m => m.Dueño)
                .Include(p => p.Atencion)!.ThenInclude(a => a.Turno)!.ThenInclude(t => t.Servicio)
                .Include(p => p.UsuarioCrea)
                .AsNoTracking()
                .FirstOrDefault(p => p.IdPago == id);

            if (pago == null) return NotFound();

            return View(pago);
        }

        // POST: Pagos/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Anular(int id)
        {
            var pago = _context.Pagos.Find(id);
            if (pago == null) return NotFound();

            if (pago.Anulado)
            {
                TempData["Error"] = "Este pago ya se encuentra anulado.";
            }
            else
            {
                pago.Anulado = true;
                _context.SaveChanges();
                TempData["Success"] = $"Transacción #{id} anulada correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
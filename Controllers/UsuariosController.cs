using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly DataContext _context;

        public UsuariosController(DataContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios.AsNoTracking().OrderBy(u => u.Apellido).ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View(new Usuario { Estado = true, Rol = "Veterinario" });
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                bool existe = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == usuario.Email.Trim().ToLower());
                if (existe)
                {
                    ModelState.AddModelError("Email", "El correo electrónico ya se encuentra registrado.");
                    return View(usuario);
                }

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario) return NotFound();

            if (ModelState.IsValid)
            {
                var usuarioDb = await _context.Usuarios.FindAsync(id);
                if (usuarioDb == null) return NotFound();

                usuarioDb.Nombre = usuario.Nombre;
                usuarioDb.Apellido = usuario.Apellido;
                usuarioDb.Email = usuario.Email;
                usuarioDb.Rol = usuario.Rol;
                usuarioDb.Matricula = usuario.Matricula;
                usuarioDb.Estado = usuario.Estado;

                if (!string.IsNullOrWhiteSpace(usuario.Clave))
                {
                    usuarioDb.Clave = usuario.Clave;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // POST: Usuarios/ToggleEstado/5 (Baja/Alta)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                usuario.Estado = !usuario.Estado;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Estado de {usuario.NombreCompleto} modificado a {(usuario.Estado ? "Activo" : "Inactivo")}.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
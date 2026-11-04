using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public AccountController(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string clave, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(clave))
            {
                ModelState.AddModelError(string.Empty, "Debe ingresar correo y contraseña.");
                return View();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower() && u.Clave == clave && u.Estado);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales incorrectas o usuario inhabilitado.");
                return View();
            }

            // Construir Claims de Sesión
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("Avatar", usuario.Avatar ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/Restringido
        [AllowAnonymous]
        public IActionResult Restringido()
        {
            return View();
        }

        // GET: Account/Perfil
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Account/ModificarPerfil
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarPerfil(Usuario model, IFormFile? avatarFile)
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return NotFound();

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Email = model.Email;
            if (!string.IsNullOrEmpty(model.Matricula))
            {
                usuario.Matricula = model.Matricula;
            }

            // Procesar Avatar
            if (avatarFile != null && avatarFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Borrar avatar viejo si existía
                if (!string.IsNullOrEmpty(usuario.Avatar))
                {
                    string rutaVieja = Path.Combine(_environment.WebRootPath, usuario.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(rutaVieja)) System.IO.File.Delete(rutaVieja);
                }

                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(avatarFile.FileName)}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }
                usuario.Avatar = $"/uploads/avatars/{fileName}";
            }

            await _context.SaveChangesAsync();

            // Refrescar Claims para que el avatar se actualice de inmediato en la barra
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("Avatar", usuario.Avatar ?? "")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            TempData["Success"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        // POST: Account/CambiarClave
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarClave(string claveActual, string nuevaClave, string confirmarClave)
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return NotFound();

            if (usuario.Clave != claveActual)
            {
                TempData["Error"] = "La contraseña actual no es válida.";
                return RedirectToAction(nameof(Perfil));
            }

            if (string.IsNullOrWhiteSpace(nuevaClave) || nuevaClave != confirmarClave)
            {
                TempData["Error"] = "La nueva contraseña y su confirmación no coinciden.";
                return RedirectToAction(nameof(Perfil));
            }

            usuario.Clave = nuevaClave;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Contraseña modificada exitosamente.";
            return RedirectToAction(nameof(Perfil));
        }
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UbiBasuraWeb.Models;

namespace UbiBasuraWeb.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public LoginModel(AppDbContext db, IPasswordHasher<Usuario> passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        [BindProperty]
        public string Correo { get; set; } = string.Empty;

        [BindProperty]
        public string Contrasena { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Correo = Correo.Trim().ToLowerInvariant();
            var usuario = _db.Usuarios.FirstOrDefault(u => u.Correo == Correo);
            var contraseñaValida = usuario != null &&
                (_passwordHasher.VerifyHashedPassword(usuario, usuario.Contrasena, Contrasena) != PasswordVerificationResult.Failed ||
                 usuario.Contrasena == Contrasena);

            if (!contraseñaValida)
            {
                Mensaje = "Correo o contraseña incorrectos";
                return Page();
            }

            if (usuario!.Contrasena == Contrasena)
            {
                usuario.Contrasena = _passwordHasher.HashPassword(usuario, Contrasena);
                _db.SaveChanges();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Nombre),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToPage("/Mapa");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using UbiBasuraWeb.Models;

namespace UbiBasuraWeb.Pages;

public class RegistroModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public RegistroModel(AppDbContext db, IPasswordHasher<Usuario> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [BindProperty]
    public string Nombre { get; set; } = string.Empty;

    [BindProperty]
    public string Correo { get; set; } = string.Empty;

    [BindProperty]
    public string Contrasena { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public IActionResult OnPost()
    {
        Nombre = Nombre.Trim();
        Correo = Correo.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Correo) || Contrasena.Length < 6)
        {
            Mensaje = "Completa todos los campos. La contraseña debe tener al menos 6 caracteres.";
            return Page();
        }

        if (_db.Usuarios.Any(usuario => usuario.Correo == Correo))
        {
            Mensaje = "Ya existe una cuenta con ese correo.";
            return Page();
        }

        _db.Usuarios.Add(new Usuario
        {
            Nombre = Nombre,
            Correo = Correo,
            Contrasena = _passwordHasher.HashPassword(null!, Contrasena),
            Rol = "usuario"
        });
        _db.SaveChanges();

        return RedirectToPage("/Login");
    }
}

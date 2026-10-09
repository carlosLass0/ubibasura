using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UbiBasuraWeb.Models;

namespace UbiBasuraWeb.Pages;

[Authorize(Roles = "administrador")]
public class UsuariosModel : PageModel
{
    private readonly AppDbContext _db;

    public UsuariosModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public int UsuarioId { get; set; }

    [BindProperty]
    public string NuevoRol { get; set; } = string.Empty;

    public IList<UsuarioAdminFila> Usuarios { get; private set; } = new List<UsuarioAdminFila>();
    public string Mensaje { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        await CargarUsuariosAsync();
    }

    public async Task<IActionResult> OnPostCambiarRolAsync()
    {
        if (NuevoRol is not ("usuario" or "administrador"))
        {
            Mensaje = "Selecciona un rol válido.";
            await CargarUsuariosAsync();
            return Page();
        }

        var idActual = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idActual, out var usuarioActualId))
        {
            return Forbid();
        }

        var usuario = await _db.Usuarios.AsNoTracking()
            .Where(cuenta => cuenta.Id == UsuarioId)
            .Select(cuenta => new { cuenta.Id })
            .SingleOrDefaultAsync();
        if (usuario == null)
        {
            return NotFound();
        }

        if (usuario.Id == usuarioActualId && NuevoRol != "administrador")
        {
            Mensaje = "No puedes quitarte el rol de administrador a ti mismo.";
            await CargarUsuariosAsync();
            return Page();
        }

        var filasActualizadas = await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "Usuarios"
            SET "Rol" = {NuevoRol}
            WHERE "Id" = {UsuarioId}
              AND (
                  {NuevoRol} = 'administrador'
                  OR "Rol" <> 'administrador'
                  OR (SELECT COUNT(*) FROM "Usuarios" WHERE "Rol" = 'administrador') > 1
              )
            """);

        if (filasActualizadas == 0)
        {
            Mensaje = "Debe permanecer al menos un administrador.";
            await CargarUsuariosAsync();
            return Page();
        }

        return RedirectToPage();
    }

    private async Task CargarUsuariosAsync()
    {
        Usuarios = await _db.Usuarios.AsNoTracking()
            .OrderBy(usuario => usuario.Nombre)
            .Select(usuario => new UsuarioAdminFila(usuario.Id, usuario.Nombre, usuario.Correo, usuario.Rol))
            .ToListAsync();
    }
}

public sealed record UsuarioAdminFila(int Id, string Nombre, string Correo, string Rol);
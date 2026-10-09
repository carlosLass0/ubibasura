using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UbiBasuraWeb.Models;

namespace UbiBasuraWeb.Pages;

[Authorize]
public class ReportesModel : PageModel
{
    private readonly AppDbContext _db;

    public ReportesModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public Reporte NuevoReporte { get; set; } = new() { Estado = "pendiente" };

    [BindProperty]
    public int ReporteId { get; set; }

    [BindProperty]
    public string NuevoEstado { get; set; } = string.Empty;

    public IList<Reporte> Reportes { get; private set; } = new List<Reporte>();
    public IList<ReporteAdminFila> ReportesAdmin { get; private set; } = new List<ReporteAdminFila>();
    public IList<Contenedor> Contenedores { get; private set; } = new List<Contenedor>();
    public string Mensaje { get; private set; } = string.Empty;
    public bool EsAdministrador => User.IsInRole("administrador");

    public async Task OnGetAsync()
    {
        await CargarDatosAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (EsAdministrador)
        {
            return Forbid();
        }

        if (NuevoReporte.ContenedorId <= 0 || string.IsNullOrWhiteSpace(NuevoReporte.TipoReporte))
        {
            Mensaje = "Selecciona un contenedor y el tipo de problema.";
            await CargarDatosAsync();
            return Page();
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioId, out var idUsuario))
        {
            return RedirectToPage("/Login");
        }

        NuevoReporte.UsuarioId = idUsuario;
        NuevoReporte.Estado = "pendiente";
        NuevoReporte.Fecha = DateTime.UtcNow;
        NuevoReporte.Comentario = NuevoReporte.Comentario?.Trim() ?? string.Empty;

        _db.Reportes.Add(NuevoReporte);
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActualizarEstadoAsync()
    {
        if (!EsAdministrador)
        {
            return Forbid();
        }

        var estadosPermitidos = new[] { "pendiente", "en_revision", "resuelto" };
        if (!estadosPermitidos.Contains(NuevoEstado))
        {
            Mensaje = "Selecciona un estado válido.";
            await CargarDatosAsync();
            return Page();
        }

        var reporte = await _db.Reportes.FindAsync(ReporteId);
        if (reporte == null)
        {
            return NotFound();
        }

        reporte.Estado = NuevoEstado;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    private async Task CargarDatosAsync()
    {
        Contenedores = await _db.Contenedores.AsNoTracking().ToListAsync();

        if (EsAdministrador)
        {
            ReportesAdmin = await (
                from reporte in _db.Reportes.AsNoTracking()
                join contenedor in _db.Contenedores.AsNoTracking() on reporte.ContenedorId equals contenedor.Id
                join usuario in _db.Usuarios.AsNoTracking() on reporte.UsuarioId equals usuario.Id
                orderby reporte.Fecha descending
                select new ReporteAdminFila(
                    reporte.Id,
                    contenedor.Codigo,
                    usuario.Nombre,
                    usuario.Correo,
                    reporte.TipoReporte,
                    reporte.Comentario,
                    reporte.Fecha,
                    reporte.Estado))
                .ToListAsync();
            return;
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(usuarioId, out var idUsuario))
        {
            Reportes = await _db.Reportes.AsNoTracking()
                .Where(reporte => reporte.UsuarioId == idUsuario)
                .OrderByDescending(reporte => reporte.Fecha)
                .ToListAsync();
        }
    }
}

public sealed record ReporteAdminFila(
    int Id,
    string CodigoContenedor,
    string NombreUsuario,
    string CorreoUsuario,
    string TipoReporte,
    string Comentario,
    DateTime Fecha,
    string Estado);

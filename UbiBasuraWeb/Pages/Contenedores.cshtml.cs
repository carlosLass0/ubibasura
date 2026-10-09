using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UbiBasuraWeb.Models;

namespace UbiBasuraWeb.Pages;

[Authorize(Roles = "administrador")]
public class ContenedoresModel : PageModel
{
    private readonly AppDbContext _db;

    public ContenedoresModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public Contenedor NuevoContenedor { get; set; } = new()
    {
        Latitud = 2.4448,
        Longitud = -76.6147,
        Estado = "disponible",
        Tipo = "general"
    };

    [BindProperty]
    public string LatitudSeleccionada { get; set; } = "2.444800";

    [BindProperty]
    public string LongitudSeleccionada { get; set; } = "-76.614700";

    [BindProperty]
    public int ContenedorId { get; set; }

    [BindProperty]
    public string? NuevoEstado { get; set; }

    public IList<Contenedor> Contenedores { get; private set; } = new List<Contenedor>();
    public string Mensaje { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        await CargarContenedoresAsync();
    }

    public async Task<IActionResult> OnPostAgregarAsync()
    {
        NuevoContenedor.Codigo = NuevoContenedor.Codigo?.Trim().ToUpperInvariant() ?? string.Empty;
        NuevoContenedor.Tipo = NuevoContenedor.Tipo?.Trim().ToLowerInvariant() ?? string.Empty;
        NuevoContenedor.Estado = NuevoContenedor.Estado?.Trim().ToLowerInvariant() ?? string.Empty;
        var latitudValida = double.TryParse(LatitudSeleccionada, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitud);
        var longitudValida = double.TryParse(LongitudSeleccionada, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitud);

        if (string.IsNullOrWhiteSpace(NuevoContenedor.Codigo) ||
            await _db.Contenedores.AnyAsync(contenedor => contenedor.Codigo.ToUpper() == NuevoContenedor.Codigo))
        {
            Mensaje = "El código es obligatorio y debe ser único.";
            await CargarContenedoresAsync();
            return Page();
        }

        var tiposPermitidos = new[] { "organico", "reciclable", "general", "peligroso" };
        var estadosPermitidos = new[] { "disponible", "lleno", "dañado" };
        if (!ModelState.IsValid)
        {
            Mensaje = string.Join(" ", ModelState.Values
                .SelectMany(valor => valor.Errors)
                .Select(error => error.ErrorMessage)
                .Where(mensaje => !string.IsNullOrWhiteSpace(mensaje)));
            if (string.IsNullOrWhiteSpace(Mensaje))
            {
                Mensaje = "Revisa los campos del contenedor e inténtalo de nuevo.";
            }

            await CargarContenedoresAsync();
            return Page();
        }

        if (!tiposPermitidos.Contains(NuevoContenedor.Tipo) || !estadosPermitidos.Contains(NuevoContenedor.Estado))
        {
            Mensaje = "Selecciona un tipo y un estado válidos.";
            await CargarContenedoresAsync();
            return Page();
        }

        if (!latitudValida || !longitudValida ||
            !double.IsFinite(latitud) || !double.IsFinite(longitud) ||
            latitud is < -90 or > 90 ||
            longitud is < -180 or > 180 ||
            (latitud == 0 && longitud == 0))
        {
            Mensaje = "Verifica los datos y selecciona una ubicación válida en el mapa.";
            await CargarContenedoresAsync();
            return Page();
        }

        NuevoContenedor.Latitud = latitud;
        NuevoContenedor.Longitud = longitud;
        _db.Contenedores.Add(NuevoContenedor);
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEstadoAsync()
    {
        if (NuevoEstado is not ("disponible" or "lleno" or "dañado"))
        {
            return BadRequest();
        }

        var contenedor = await _db.Contenedores.FindAsync(ContenedorId);
        if (contenedor != null)
        {
            contenedor.Estado = NuevoEstado;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync()
    {
        var contenedor = await _db.Contenedores.FindAsync(ContenedorId);
        if (contenedor != null)
        {
            _db.Contenedores.Remove(contenedor);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task CargarContenedoresAsync()
    {
        Contenedores = await _db.Contenedores.AsNoTracking().OrderBy(contenedor => contenedor.Codigo).ToListAsync();
    }
}

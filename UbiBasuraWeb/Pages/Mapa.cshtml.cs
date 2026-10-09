using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UbiBasuraWeb.Models;

namespace UbiBasuraWeb.Pages;

[Authorize]
public class MapaModel : PageModel
{
    private readonly AppDbContext _db;

    public MapaModel(AppDbContext db)
    {
        _db = db;
    }

    public IList<Contenedor> Contenedores { get; private set; } = new List<Contenedor>();

    public async Task OnGetAsync()
    {
        Contenedores = await _db.Contenedores.AsNoTracking().ToListAsync();
    }
}

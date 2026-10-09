using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UbiBasuraWeb.Models;

namespace UbiBasuraWeb.Pages;

[Authorize(Roles = "administrador")]
public class DashboardModel : PageModel
{
	private readonly AppDbContext _db;

	public DashboardModel(AppDbContext db)
	{
		_db = db;
	}

	public int TotalContenedores { get; private set; }
	public int TotalReportes { get; private set; }
	public int ReportesPendientes { get; private set; }
	public int TotalUsuarios { get; private set; }

	public async Task OnGetAsync()
	{
		TotalContenedores = await _db.Contenedores.CountAsync();
		TotalReportes = await _db.Reportes.CountAsync();
		ReportesPendientes = await _db.Reportes.CountAsync(reporte => reporte.Estado == "pendiente");
		TotalUsuarios = await _db.Usuarios.CountAsync();
	}
}

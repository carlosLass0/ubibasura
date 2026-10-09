using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using UbiBasuraWeb.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.Events.OnValidatePrincipal = async context =>
        {
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out var id))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var currentRole = await db.Usuarios.AsNoTracking()
                .Where(usuario => usuario.Id == id)
                .Select(usuario => usuario.Rol)
                .SingleOrDefaultAsync();
            var cookieRole = context.Principal?.FindFirstValue(ClaimTypes.Role);

            if (currentRole == null || currentRole != cookieRole)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var seedAdminEmail = builder.Configuration["SeedAdmin:Email"];
    var seedAdminPassword = builder.Configuration["SeedAdmin:Password"];
    if (!db.Usuarios.Any() &&
        !string.IsNullOrWhiteSpace(seedAdminEmail) &&
        !string.IsNullOrWhiteSpace(seedAdminPassword))
    {
        db.Usuarios.Add(new Usuario
        {
            Nombre = "Administrador",
            Correo = seedAdminEmail.Trim().ToLowerInvariant(),
            Contrasena = new PasswordHasher<Usuario>().HashPassword(null!, seedAdminPassword),
            Rol = "administrador"
        });
        db.SaveChanges();
    }

    if (!db.Contenedores.Any())
    {
        db.Contenedores.AddRange(
            new Contenedor { Codigo = "C-001", Latitud = 2.4448, Longitud = -76.6147, Tipo = "organico", Estado = "disponible" },
            new Contenedor { Codigo = "C-002", Latitud = 2.4430, Longitud = -76.6120, Tipo = "reciclable", Estado = "disponible" },
            new Contenedor { Codigo = "C-003", Latitud = 2.4460, Longitud = -76.6180, Tipo = "general", Estado = "lleno" },
            new Contenedor { Codigo = "C-004", Latitud = 2.4410, Longitud = -76.6090, Tipo = "peligroso", Estado = "disponible" });
        db.SaveChanges();
    }
}

app.Run();
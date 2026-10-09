# UbiBasura

Aplicacion web para consultar contenedores en un mapa y gestionar reportes de residuos. El proyecto principal esta en `UbiBasuraWeb` y usa ASP.NET Core Razor Pages, Entity Framework Core y SQLite.

## Requisitos

- .NET SDK 10

## Ejecutar en desarrollo

Desde PowerShell, configura las credenciales iniciales del administrador antes del primer inicio:

```powershell
$env:SeedAdmin__Email = "admin@ejemplo.com"
$env:SeedAdmin__Password = "Una-clave-larga-y-unica"
dotnet run --project UbiBasuraWeb/UbiBasuraWeb.csproj --launch-profile http
```

La cuenta administradora se crea solo cuando la base de datos no contiene usuarios y ambas variables estan configuradas. Si no se configuran, la aplicacion no crea una cuenta con credenciales predeterminadas.

La base de datos SQLite se crea localmente como `UbiBasuraWeb/ubibasura.db` y no debe subirse al repositorio.
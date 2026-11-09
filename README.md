# SSDAP

Sistema de Soporte a la Decisión para Agua Potable. Permite gestionar fuentes de abastecimiento, programar baterías de pozos y estimar producción y calidad de mezclas de agua.

## Proyectos

- `SSDAP.Api`: API ASP.NET Core, autorización y persistencia con Entity Framework Core y MySQL.
- `SSDAP.Web`: aplicación Blazor con administración, planificación, análisis y reportes PDF.
- `SSDAP.Mobile`: aplicación .NET MAUI Blazor Hybrid para consulta y registro operativo con conexión.
- `SSDAP.Shared`: contratos y validaciones compartidos.

## Desarrollo local

Requiere .NET 10, MySQL y las cargas de trabajo de MAUI para la plataforma elegida.

1. Copiar `SSDAP.Api/appsettings.Development.example.json` como `SSDAP.Api/appsettings.Development.json` y completar la conexión local. Este archivo no se versiona ni se publica.
2. Restaurar las dependencias y aplicar las migraciones de la API a la base de desarrollo.
3. Ejecutar `SSDAP.Api` y `SSDAP.Web` con sus perfiles de desarrollo. La Web utiliza `Api:BaseUrl` para localizar la API.
4. Para crear el primer administrador, usar `scripts/CrearAdministrador.ps1`.

Los cambios de esquema se mantienen en las migraciones. Las aplicaciones Web y Mobile no acceden directamente a MySQL.

## Publicación

La API y la Web se publican como aplicaciones separadas. Ambas pueden alojarse en el mismo proveedor, con un sitio para cada una. El paquete publicado incluye automáticamente la biblioteca Shared.

Configurar en el servidor, fuera del repositorio:

| Aplicación | Configuración |
| --- | --- |
| API | `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__DefaultConnection`, `Jwt__Key` (clave aleatoria de al menos 32 bytes) |
| Web | `ASPNETCORE_ENVIRONMENT=Production`, `Api__BaseUrl` (URL HTTPS de la API), `GoogleMaps__ApiKey`, `Pdf__Licencia` según la licencia aplicable de QuestPDF |

Habilitar HTTPS en ambos sitios y comprobar WebSockets/SignalR para Blazor. Mantener persistentes las claves de ASP.NET Core Data Protection en la Web.

La base alojada se prepara con las migraciones o mediante una exportación completa de la base actual que conserve el historial de migraciones. No aplicar ambos procedimientos como cargas iniciales independientes.

La app móvil debe configurarse con la URL HTTPS pública de la API antes de distribuirse. La dirección localhost de desarrollo no permite acceder desde otro dispositivo. Android admite instalación directa mediante un APK firmado; iOS requiere su proceso de firma y distribución correspondiente.

### Mapa en instalaciones móviles

Copiar `SSDAP.Mobile/Resources/Raw/maps.example.json` como `maps.local.json` en la misma carpeta y completar `GoogleMaps.ApiKey`. El archivo local se excluye de Git y se incorpora al APK al compilar. La app prioriza esa clave y conserva como alternativa la configuración guardada en dispositivos anteriores. Sin clave, las demás funciones continúan disponibles.

El mapa usa **Maps JavaScript API** dentro de Blazor Hybrid, no Maps SDK for Android. Usar una clave dedicada restringida a esa API y comprobar las restricciones de sitio en el dispositivo: los WebView locales pueden omitir el encabezado Referer y Google puede rechazar la solicitud. No quitar las restricciones de una clave de producción para resolverlo. Véase la [guía de seguridad de Google Maps](https://developers.google.com/maps/api-security-best-practices). La clave incluida en un cliente es extraíble del APK; excluirla de Git no la convierte en un secreto del servidor.

Generar el paquete con `scripts/CrearApkPruebas.ps1` y actualizar la instalación con el APK resultante, manteniendo la misma firma. Cambiar la clave requiere generar e instalar otro APK, sin volver a publicar la API ni la Web.

## Compilación

```powershell
dotnet build SSDAP.Api -c Release
```

No versionar contraseñas, claves privadas, perfiles de publicación, copias de la base ni paquetes de instalación. Los resultados de calidad son estimaciones sobre los parámetros evaluados y no certifican potabilidad.

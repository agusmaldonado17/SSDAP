[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$proyecto = Join-Path $PSScriptRoot '../SSDAP.Mobile/SSDAP.Mobile.csproj'
$mapas = Join-Path $PSScriptRoot '../SSDAP.Mobile/Resources/Raw/maps.local.json'
if (Test-Path -LiteralPath $mapas) {
    try { $configuracionMapa = Get-Content -LiteralPath $mapas -Raw | ConvertFrom-Json }
    catch { throw 'maps.local.json no es un JSON valido. Revisalo antes de generar el APK.' }
    if ([string]::IsNullOrWhiteSpace($configuracionMapa.GoogleMaps.ApiKey)) {
        Write-Warning 'La clave de Google Maps esta vacia. El APK funcionara sin el mapa integrado.'
    }
} else {
    Write-Warning 'No existe maps.local.json. Copia maps.example.json y completa la clave para incluir el mapa.'
}

# Incluye los ensamblados: el APK se puede instalar sin despliegue rápido de Visual Studio.
# Usa la firma de depuración local; este paquete es para pruebas, no para publicar en tiendas.
dotnet build $proyecto -f net10.0-android -c Debug -m:1 -t:SignAndroidPackage `
    -p:EmbedAssembliesIntoApk=true -p:AndroidPackageFormats=apk
if ($LASTEXITCODE -ne 0) { throw 'No se pudo generar el APK de pruebas.' }

$carpeta = Join-Path $PSScriptRoot '../SSDAP.Mobile/bin/Debug/net10.0-android'
$apk = Join-Path $carpeta 'com.companyname.ssdap.mobile-Signed.apk'
if (-not (Test-Path -LiteralPath $apk)) { throw 'No se encontro el APK firmado esperado.' }
$destino = Join-Path $carpeta 'SSDAP-pruebas.apk'
Copy-Item -LiteralPath $apk -Destination $destino -Force
Write-Output "APK de pruebas: $((Resolve-Path -LiteralPath $destino).Path)"
Get-FileHash -LiteralPath $destino -Algorithm SHA256

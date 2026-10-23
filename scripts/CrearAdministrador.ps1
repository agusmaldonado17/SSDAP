param(
    [Parameter(Mandatory = $true)]
    [string]$Email
)

$ErrorActionPreference = 'Stop'
$previousEmail = $env:AdministradorInicial__Email
$previousPassword = $env:AdministradorInicial__Password
$password = Read-Host 'Password del administrador' -AsSecureString
try {
    $env:AdministradorInicial__Email = $Email
    $env:AdministradorInicial__Password = [System.Net.NetworkCredential]::new('', $password).Password
    $projectPath = Join-Path $PSScriptRoot '../SSDAP.Api'
    dotnet run --project $projectPath --configuration Release --launch-profile https -- --inicializar-admin
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear el administrador. Revisá el error anterior.' }
}
finally {
    $env:AdministradorInicial__Email = $previousEmail
    $env:AdministradorInicial__Password = $previousPassword
    $password.Dispose()
}

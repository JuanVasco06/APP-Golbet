$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$settings = Get-Content -LiteralPath (Join-Path $repositoryRoot 'GolBet.Web\appsettings.json') -Raw | ConvertFrom-Json
$connectionString = $settings.ConnectionStrings.DefaultConnection

# The launchers use Development, so read the same connection overrides as the app.
$developmentPath = Join-Path $repositoryRoot 'GolBet.Web\appsettings.Development.json'
if (Test-Path -LiteralPath $developmentPath) {
    $development = Get-Content -LiteralPath $developmentPath -Raw | ConvertFrom-Json
    if ($development.ConnectionStrings.DefaultConnection) {
        $connectionString = $development.ConnectionStrings.DefaultConnection
    }
}
if ($env:ConnectionStrings__DefaultConnection) {
    $connectionString = $env:ConnectionStrings__DefaultConnection
}
if (!$connectionString) { throw 'Falta configurar la conexion DefaultConnection.' }

$connectionSettings = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $connectionString
$localDbPrefix = '(localdb)\'
if (!$connectionSettings.DataSource.StartsWith($localDbPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    Write-Host 'La conexion usa otro servidor SQL; no es necesario iniciar LocalDB.'
    return
}
$instanceName = $connectionSettings.DataSource.Substring($localDbPrefix.Length)

$localDbCommand = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
$localDbPath = if ($localDbCommand) { $localDbCommand.Source } else { $null }
if (!$localDbPath) {
    $sqlRoot = Join-Path $env:ProgramFiles 'Microsoft SQL Server'
    $localDbPath = Get-ChildItem -LiteralPath $sqlRoot -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '^\d+$' } |
        Sort-Object { [int]$_.Name } -Descending |
        ForEach-Object { Join-Path $_.FullName 'Tools\Binn\SqlLocalDB.exe' } |
        Where-Object { Test-Path -LiteralPath $_ } |
        Select-Object -First 1
}
if (!$localDbPath) { throw 'Falta SQL Server LocalDB. Instalalo desde el instalador de Visual Studio.' }

# Start the existing instance. Never recreate it or remove its database files.
& $localDbPath start $instanceName
if ($LASTEXITCODE -ne 0) {
    throw "LocalDB no pudo iniciar '$instanceName'. Revisa su error.log en %LOCALAPPDATA%\Microsoft\Microsoft SQL Server Local DB\Instances. Conserva los archivos de la base."
}

# Opening master also checks Windows authentication before launching the app.
$connectionSettings['Initial Catalog'] = 'master'
$connectionSettings['AttachDBFilename'] = ''
$connectionSettings['Connect Timeout'] = 10
$connection = New-Object System.Data.SqlClient.SqlConnection $connectionSettings.ConnectionString
try {
    $connection.Open()
    Write-Host "LocalDB '$instanceName' esta listo. Ya puedes ejecutar GolBet." -ForegroundColor Green
} finally {
    $connection.Dispose()
}

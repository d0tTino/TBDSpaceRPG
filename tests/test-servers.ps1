param(
    [int]$GitPort = 8080,
    [int]$PostgresPort = 8003
)

$scriptDir = $PSScriptRoot
$root = Split-Path -Parent $scriptDir

$gitScript = Join-Path $root 'servers/git/index.js'
$pgScript  = Join-Path $root 'servers/postgres/index.js'

$env:PORT = $GitPort
$gitProc = Start-Process -FilePath node -ArgumentList $gitScript -PassThru -NoNewWindow

$env:PORT = $PostgresPort
$pgProc  = Start-Process -FilePath node -ArgumentList $pgScript -PassThru -NoNewWindow

Start-Sleep -Seconds 2

try {
    $gitResponse = Invoke-WebRequest -Uri "http://localhost:$GitPort" -UseBasicParsing -TimeoutSec 5
    Write-Host "Git server responded: $($gitResponse.Content)"

    $pgResponse = Invoke-WebRequest -Uri "http://localhost:$PostgresPort" -UseBasicParsing -TimeoutSec 5
    Write-Host "Postgres server responded: $($pgResponse.Content)"
} finally {
    $gitProcess = Get-Process -Id $gitProc.Id -ErrorAction SilentlyContinue
    if ($gitProcess) {
        Stop-Process -Id $gitProcess.Id -Force
    }

    $pgProcess = Get-Process -Id $pgProc.Id -ErrorAction SilentlyContinue
    if ($pgProcess) {
        Stop-Process -Id $pgProcess.Id -Force
    }
}

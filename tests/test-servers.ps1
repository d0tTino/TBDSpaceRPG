param(
    [int]$GitPort = 8080,
    [int]$PostgresPort = 8003
)

$scriptDir = $PSScriptRoot
$root = Split-Path -Parent $scriptDir

$gitScript = Join-Path $root 'run-git-server.ps1'
$pgScript  = Join-Path $root 'run-postgres-server.ps1'

$gitProc = Start-Process -FilePath pwsh -ArgumentList '-File', $gitScript, '-Port', $GitPort -PassThru
$pgProc  = Start-Process -FilePath pwsh -ArgumentList '-File', $pgScript, '-Port', $PostgresPort -PassThru

Start-Sleep -Seconds 2

try {
    $gitResponse = Invoke-WebRequest -Uri "http://localhost:$GitPort" -UseBasicParsing -TimeoutSec 5
    Write-Host "Git server responded: $($gitResponse.Content)"

    $pgResponse = Invoke-WebRequest -Uri "http://localhost:$PostgresPort" -UseBasicParsing -TimeoutSec 5
    Write-Host "Postgres server responded: $($pgResponse.Content)"
} finally {
    Stop-Process -Id $gitProc.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $pgProc.Id -Force -ErrorAction SilentlyContinue

}

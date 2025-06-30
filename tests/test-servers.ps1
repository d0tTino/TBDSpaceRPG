param(
    [int]$GitPort = 8080,
    [int]$PostgresPort = 8003
)

$scriptDir = $PSScriptRoot
$root = Split-Path -Parent $scriptDir

$gitServer = Join-Path $root 'servers/git/index.js'
$pgServer  = Join-Path $root 'servers/postgres/index.js'

$gitProc = Start-Process node -ArgumentList $gitServer -NoNewWindow -PassThru -Environment @{
    'PORT' = $GitPort
}
$pgProc  = Start-Process node -ArgumentList $pgServer -NoNewWindow -PassThru -Environment @{
    'PORT' = $PostgresPort
}

Start-Sleep -Seconds 2

try {
    $gitResponse = Invoke-WebRequest -Uri "http://localhost:$GitPort" -UseBasicParsing -TimeoutSec 5
    Write-Host "Git server responded: $($gitResponse.Content)"

    $pgResponse = Invoke-WebRequest -Uri "http://localhost:$PostgresPort" -UseBasicParsing -TimeoutSec 5
    Write-Host "Postgres server responded: $($pgResponse.Content)"
} finally {
    $gitRunning = Get-Process -Id $gitProc.Id -ErrorAction SilentlyContinue
    if ($gitRunning) { Stop-Process -Id $gitProc.Id -Force }

    $pgRunning = Get-Process -Id $pgProc.Id -ErrorAction SilentlyContinue
    if ($pgRunning) { Stop-Process -Id $pgProc.Id -Force }

    # Ensure child Node.js servers are terminated
    Get-Process node -ErrorAction SilentlyContinue | Stop-Process -Force

}

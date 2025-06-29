param(
    [int]$Port = 8003,
    [string]$ServerPath
)

function Test-ValidPort {
    param([int]$Port)
    return ($Port -ge 1 -and $Port -le 65535)
}

if (-not (Test-ValidPort -Port $Port)) {
    Write-Error "Invalid port number $Port. Port must be between 1 and 65535." -ErrorAction Stop
}

$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (-not $ServerPath) {
    $ServerPath = Join-Path $scriptDir 'servers/postgres'
}

if (-not (Test-Path $ServerPath)) {
    Write-Error "Postgres server path not found: $ServerPath"
    exit 1
}

try {
    $nodeVersion = node --version
    Write-Host "Using Node.js $nodeVersion"
} catch {
    Write-Error 'Node.js is required to run the Postgres server.'
    exit 1
}

Push-Location $ServerPath
try {
    $env:PORT = $Port
    $process = Start-Process node -ArgumentList 'index.js' -NoNewWindow -PassThru
    Write-Host "Postgres MCP Server started on port $Port (PID: $($process.Id))" -ForegroundColor Green
} finally {
    Pop-Location
}

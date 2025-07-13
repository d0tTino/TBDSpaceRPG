param(
    [int]$Port = 8090,
    [string]$ServerPath,
    [string]$ConfigFile = "engine-config.json"
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
if (Test-Path $ConfigFile) {
    try {
        $cfg = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json
        if ($cfg.telemetry) {
            if (-not $PSBoundParameters.ContainsKey('Port') -and $cfg.telemetry.port) { $Port = [int]$cfg.telemetry.port }
            if (-not $ServerPath -and $cfg.telemetry.directory) { $ServerPath = Join-Path $scriptDir $cfg.telemetry.directory }
        }
    } catch {
        Write-Warning "Failed to load engine config from $ConfigFile: $_"
    }
}
if (-not $ServerPath) {
    $ServerPath = Join-Path $scriptDir 'servers/telemetry'
}

if (-not (Test-Path $ServerPath)) {
    Write-Error "Telemetry server path not found: $ServerPath"
    exit 1
}

try {
    $nodeVersion = node --version
    Write-Host "Using Node.js $nodeVersion"
} catch {
    Write-Error 'Node.js is required to run the telemetry server.'
    exit 1
}

Push-Location $ServerPath
try {
    $env:PORT = $Port
    $process = Start-Process node -ArgumentList 'index.cjs' -NoNewWindow -PassThru
    Write-Host "Telemetry Server started on port $Port (PID: $($process.Id))" -ForegroundColor Green
} finally {
    Pop-Location
}


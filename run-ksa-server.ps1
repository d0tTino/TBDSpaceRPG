# Starts the KSA adapter server. The script now reports an error and exits if
# the configuration file or server path are missing.
param(
    [int]$Port,
    [string]$ServerPath,
    [string]$ConfigFile = "engine-config.json"
)

function Test-ValidPort {
    param([int]$Port)
    return ($Port -ge 1 -and $Port -le 65535)
}

if ($PSBoundParameters.ContainsKey('Port') -and -not (Test-ValidPort -Port $Port)) {
    Write-Error "Invalid port number $Port. Port must be between 1 and 65535." -ErrorAction Stop
}

$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
# Fail early when the configuration file is missing
if (-not (Test-Path $ConfigFile)) {
    Write-Error "Configuration file not found: $ConfigFile"
    exit 1
}
if (Test-Path $ConfigFile) {
    try {
        $cfg = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json
        if ($cfg.ksa) {
            if (-not $PSBoundParameters.ContainsKey('Port') -and $cfg.ksa.port) { $Port = [int]$cfg.ksa.port }
            if (-not $ServerPath -and $cfg.ksa.directory) { $ServerPath = Join-Path $scriptDir $cfg.ksa.directory }
        }
    } catch {
        Write-Warning "Failed to load engine config from $ConfigFile: $_"
    }
}
if (-not $Port) {
    $portsPath = Join-Path $scriptDir 'servers/ports.cjs'
    $Port = [int](node -e "const p=require(process.argv[1]);console.log(p.ksa);" $portsPath)
}
if (-not (Test-ValidPort -Port $Port)) {
    Write-Error "Invalid port number $Port. Port must be between 1 and 65535." -ErrorAction Stop
}
if (-not $ServerPath) {
    $ServerPath = Join-Path $scriptDir 'servers/ksa'
}

# Fail early if the server directory cannot be located
if (-not (Test-Path $ServerPath)) {
    Write-Error "KSA adapter path not found: $ServerPath"
    exit 1
}

try {
    $nodeVersion = node --version
    Write-Host "Using Node.js $nodeVersion"
} catch {
    Write-Error 'Node.js is required to run the KSA adapter.'
    exit 1
}

Push-Location $ServerPath
try {
    $env:PORT = $Port
    $process = Start-Process node -ArgumentList 'index.cjs' -NoNewWindow -PassThru
    Write-Host "KSA adapter started on port $Port (PID: $($process.Id))" -ForegroundColor Green
} finally {
    Pop-Location
}

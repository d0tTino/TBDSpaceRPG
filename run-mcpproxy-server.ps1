
param(
    [int]$Port,
    [string]$ProxyPath,
    [string]$ConfigFile = "engine-config.json"
)

# PowerShell script to start the MCP Proxy server
Write-Host "Starting MCP Proxy Server..."

$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (Test-Path $ConfigFile) {
    try {
        $cfg = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json
        if ($cfg.mcpproxy) {
            if (-not $PSBoundParameters.ContainsKey('Port') -and $cfg.mcpproxy.port) { $Port = [int]$cfg.mcpproxy.port }
            if (-not $ProxyPath -and $cfg.mcpproxy.directory) { $ProxyPath = Join-Path $scriptDir $cfg.mcpproxy.directory }
        }
    } catch {
        Write-Warning "Failed to load engine config from $ConfigFile: $_"
    }
}
if (-not $Port) {
    $portsPath = Join-Path $scriptDir 'servers/ports.cjs'
    $Port = [int](node -e "const p=require(process.argv[1]);console.log(p.mcpproxy);" $portsPath)
}
if (-not (Test-ValidPort -Port $Port)) {
    Write-Error "Invalid port: $Port. Must be between 1 and 65535."
    exit 1
}
if (-not $ProxyPath) {
    # Default to the "servers/mcpProxy" directory relative to this script
    $ProxyPath = Join-Path $scriptDir 'servers/mcpProxy'
}

function Test-ValidPort {
    param([int]$Port)
    return $Port -ge 1 -and $Port -le 65535
}

if ($PSBoundParameters.ContainsKey('Port') -and -not (Test-ValidPort -Port $Port)) {
    Write-Error "Invalid port: $Port. Must be between 1 and 65535."
    exit 1
}

# Check if Node.js is available
try {
    $nodeVersion = node --version
    Write-Host "Using Node.js $nodeVersion"
} catch {
    Write-Error "Node.js not found. Please install Node.js 18+ to run the MCP Proxy server."
    exit 1
}

# Change to the mcpProxy server directory
try {
    Set-Location -Path $ProxyPath
} catch {
    Write-Error "Unable to change directory"
    exit 1
}


# Start the mcpProxy server as a background job
Start-Process node -ArgumentList "dist/src/index.js" -NoNewWindow -PassThru

Write-Host "MCP Proxy Server started on port $Port"
Write-Host "Server will run in the background. To stop, close the Node.js process."


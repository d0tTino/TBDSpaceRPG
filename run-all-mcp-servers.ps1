# PowerShell script to start all MCP servers
param(
    [ValidateSet('unity','godot','ksa')]
    [string]$Engine = 'unity',
    [string]$ConfigFile = 'engine-config.json'
)

Write-Host "Starting All MCP Servers..." -ForegroundColor Cyan

# Get the current script directory
$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (-not $scriptDir) {
    $scriptDir = $PWD.Path
}

# Load engine configuration
$engineCfg = $null
if (Test-Path $ConfigFile) {
    try {
        $engineCfg = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json
        Write-Host "Using configuration from $ConfigFile" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to load engine configuration from $ConfigFile: $_"
    }
}

# Determine ports
$unityPort = 8001
$gitPort = 8080
$postgresPort = 8003
$telemetryPort = 8090
$proxyPort = 8004
$ksaPort = 8005

if ($engineCfg) {
    if ($engineCfg.unity.port) { $unityPort = [int]$engineCfg.unity.port }
    if ($engineCfg.git.port) { $gitPort = [int]$engineCfg.git.port }
    if ($engineCfg.postgres.port) { $postgresPort = [int]$engineCfg.postgres.port }
    if ($engineCfg.telemetry.port) { $telemetryPort = [int]$engineCfg.telemetry.port }
    if ($engineCfg.mcpproxy.port) { $proxyPort = [int]$engineCfg.mcpproxy.port }
    if ($engineCfg.ksa.port) { $ksaPort = [int]$engineCfg.ksa.port }
}

# Function to check server ports
function Test-ServerPort {
    param (
        [string]$ServerName,
        [int]$Port
    )
    
    if (Test-Path Function:\Test-PortInUse) {
        $portTest = Test-PortInUse -Port $Port
    } else {
        if (Get-Command Test-NetConnection -ErrorAction SilentlyContinue) {
            $portTest = Test-NetConnection -ComputerName 'localhost' -Port $Port -InformationLevel Quiet
        } else {
            $client = [System.Net.Sockets.TcpClient]::new()
            $task = $client.ConnectAsync('localhost', $Port)
            $portTest = $task.Wait(500)
            $client.Dispose()
        }
    }

    if ($portTest) {
        Write-Host "$ServerName is already running on port $Port." -ForegroundColor Yellow
        return $true
    }
    return $false
}

# First, check if any server is already running
$unityRunning = Test-ServerPort -ServerName "Unity MCP Server" -Port $unityPort
$gitRunning = Test-ServerPort -ServerName "Git MCP Server" -Port $gitPort
$postgresRunning = Test-ServerPort -ServerName "Postgres MCP Server" -Port $postgresPort
$telemetryRunning = Test-ServerPort -ServerName "Telemetry Server" -Port $telemetryPort
$proxyRunning = Test-ServerPort -ServerName "MCP Proxy Server" -Port $proxyPort
$ksaRunning = Test-ServerPort -ServerName "KSA Adapter" -Port $ksaPort

# 1. Start Unity MCP Server if not already running
if (-not $unityRunning) {
    Write-Host "Starting Unity MCP Server..." -ForegroundColor Green
    $unityScript = Join-Path $scriptDir "run-mcp-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$unityScript`" -Engine $Engine -Port $unityPort -EngineConfigFile $ConfigFile" -NoNewWindow
} else {
    Write-Host "Skipping Unity MCP Server (already running)" -ForegroundColor Yellow
}

# 2. Start Git MCP Server
if (-not $gitRunning) {
    Write-Host "Starting Git MCP Server..." -ForegroundColor Green
    $gitScript = Join-Path $scriptDir "run-git-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$gitScript`" -Port $gitPort -ConfigFile $ConfigFile" -NoNewWindow
} else {
    Write-Host "Skipping Git MCP Server (already running)" -ForegroundColor Yellow
}

# 3. Start Postgres MCP Server
if (-not $postgresRunning) {
    Write-Host "Starting Postgres MCP Server..." -ForegroundColor Green
    $postgresScript = Join-Path $scriptDir "run-postgres-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$postgresScript`" -Port $postgresPort -ConfigFile $ConfigFile" -NoNewWindow
} else {
    Write-Host "Skipping Postgres MCP Server (already running)" -ForegroundColor Yellow
}

# 4. Start Telemetry Server
if (-not $telemetryRunning) {
    Write-Host "Starting Telemetry Server..." -ForegroundColor Green
    $telemetryScript = Join-Path $scriptDir "run-telemetry-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$telemetryScript`" -Port $telemetryPort -ConfigFile $ConfigFile" -NoNewWindow
} else {
    Write-Host "Skipping Telemetry Server (already running)" -ForegroundColor Yellow
}

# 5. Start MCP Proxy Server
if (-not $proxyRunning) {
    Write-Host "Starting MCP Proxy Server..." -ForegroundColor Green
    $proxyScript = Join-Path $scriptDir "run-mcpproxy-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$proxyScript`" -Port $proxyPort -ConfigFile $ConfigFile" -NoNewWindow
} else {
    Write-Host "Skipping MCP Proxy Server (already running)" -ForegroundColor Yellow
}

# 6. Start KSA Adapter
if (-not $ksaRunning) {
    Write-Host "Starting KSA Adapter..." -ForegroundColor Green
    $ksaScript = Join-Path $scriptDir "run-ksa-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$ksaScript`" -Port $ksaPort -ConfigFile $ConfigFile" -NoNewWindow
} else {
    Write-Host "Skipping KSA Adapter (already running)" -ForegroundColor Yellow
}

Write-Host "All MCP servers have been started!" -ForegroundColor Cyan
Write-Host "Use the following servers in Cursor:" -ForegroundColor White
Write-Host "- mcp-unity: ws://localhost:$unityPort/McpUnity" -ForegroundColor White
Write-Host "- git: http://localhost:$gitPort" -ForegroundColor White
Write-Host "- postgres: http://localhost:$postgresPort" -ForegroundColor White
Write-Host "- telemetry: http://localhost:$telemetryPort" -ForegroundColor White
Write-Host "- mcpProxy: http://localhost:$proxyPort" -ForegroundColor White
Write-Host "- ksa-adapter: http://localhost:$ksaPort" -ForegroundColor White
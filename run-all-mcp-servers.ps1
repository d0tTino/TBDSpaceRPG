# PowerShell script to start all MCP servers
Write-Host "Starting All MCP Servers..." -ForegroundColor Cyan

# Get the current script directory
$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (-not $scriptDir) {
    $scriptDir = $PWD.Path
}

# Function to check server ports
function Test-ServerPort {
    param (
        [string]$ServerName,
        [int]$Port
    )
    
    $portTest = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    
    if ($portTest) {
        Write-Host "$ServerName is already running on port $Port." -ForegroundColor Yellow
        return $true
    }
    return $false
}

# First, check if any server is already running
$unityRunning = Test-ServerPort -ServerName "Unity MCP Server" -Port 8001
$gitRunning = Test-ServerPort -ServerName "Git MCP Server" -Port 8080
$postgresRunning = Test-ServerPort -ServerName "Postgres MCP Server" -Port 8003
$telemetryRunning = Test-ServerPort -ServerName "Telemetry Server" -Port 8090
$proxyRunning = Test-ServerPort -ServerName "MCP Proxy Server" -Port 8004

# 1. Start Unity MCP Server if not already running
if (-not $unityRunning) {
    Write-Host "Starting Unity MCP Server..." -ForegroundColor Green
    $unityScript = Join-Path $scriptDir "run-mcp-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$unityScript`"" -NoNewWindow
} else {
    Write-Host "Skipping Unity MCP Server (already running)" -ForegroundColor Yellow
}

# 2. Start Git MCP Server
if (-not $gitRunning) {
    Write-Host "Starting Git MCP Server..." -ForegroundColor Green
    $gitScript = Join-Path $scriptDir "run-git-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$gitScript`"" -NoNewWindow
} else {
    Write-Host "Skipping Git MCP Server (already running)" -ForegroundColor Yellow
}

# 3. Start Postgres MCP Server
if (-not $postgresRunning) {
    Write-Host "Starting Postgres MCP Server..." -ForegroundColor Green
    $postgresScript = Join-Path $scriptDir "run-postgres-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$postgresScript`"" -NoNewWindow
} else {
    Write-Host "Skipping Postgres MCP Server (already running)" -ForegroundColor Yellow
}

# 4. Start Telemetry Server
if (-not $telemetryRunning) {
    Write-Host "Starting Telemetry Server..." -ForegroundColor Green
    $telemetryScript = Join-Path $scriptDir "run-telemetry-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$telemetryScript`"" -NoNewWindow
} else {
    Write-Host "Skipping Telemetry Server (already running)" -ForegroundColor Yellow
}

# 5. Start MCP Proxy Server
if (-not $proxyRunning) {
    Write-Host "Starting MCP Proxy Server..." -ForegroundColor Green
    $proxyScript = Join-Path $scriptDir "run-mcpproxy-server.ps1"
    Start-Process PowerShell -ArgumentList "-File `"$proxyScript`"" -NoNewWindow
} else {
    Write-Host "Skipping MCP Proxy Server (already running)" -ForegroundColor Yellow
}

Write-Host "All MCP servers have been started!" -ForegroundColor Cyan
Write-Host "Use the following servers in Cursor:" -ForegroundColor White
Write-Host "- mcp-unity: ws://localhost:8001/McpUnity" -ForegroundColor White
Write-Host "- git: http://localhost:8080" -ForegroundColor White
Write-Host "- postgres: http://localhost:8003" -ForegroundColor White
Write-Host "- telemetry: http://localhost:8090" -ForegroundColor White
Write-Host "- mcpProxy: http://localhost:8004" -ForegroundColor White
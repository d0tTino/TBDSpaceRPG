# MCP Server Manager
# A robust script for managing the MCP Unity server

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("start", "stop", "status", "restart")]
    [string]$Action = "start",
    
    [Parameter(Mandatory=$false)]
    [string]$Port = "8001"
)

$ErrorActionPreference = "Stop"
$serverPath = Join-Path (Get-Location) "TBD SpaceGame\TBD SpaceGame\Library\PackageCache\com.gamelovers.mcp-unity@3acfb232f564\Server"
$logFile = Join-Path (Get-Location) "mcp-server.log"

# Function to check if the server is running
function Test-ServerRunning {
    param([string]$port)

    try {
        if (Get-Command Test-NetConnection -ErrorAction SilentlyContinue) {
            return Test-NetConnection -ComputerName 'localhost' -Port $port -InformationLevel Quiet
        } else {
            $client = [System.Net.Sockets.TcpClient]::new()
            $task = $client.ConnectAsync('localhost', [int]$port)
            $connected = $task.Wait(500)
            $client.Dispose()
            return $connected
        }
    }
    catch {
        Write-Host "Error checking server status: $_" -ForegroundColor Red
        return $false
    }
}

function Get-ProcessIdOnPort {
    param([int]$Port)
    try {
        if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
            $conn = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($conn) { return $conn.OwningProcess }
        } elseif (Get-Command lsof -ErrorAction SilentlyContinue) {
            $pid = lsof -i :$Port -sTCP:LISTEN -t 2>$null | Select-Object -First 1
            if ($pid) { return [int]$pid }
        }
    } catch {}
    return $null
}

# Function to start the MCP server
function Start-McpServer {
    param([string]$port)
    
    if (Test-ServerRunning -port $port) {
        Write-Host "MCP server is already running on port $port" -ForegroundColor Yellow
        return
    }
    
    try {
        Write-Host "Starting MCP server on port $port..." -ForegroundColor Cyan
        
        # Change to server directory
        Push-Location $serverPath
        
        # Set environment variable and start server
        $env:UNITY_PORT = $port
        
        # Start the process in a new window to keep it running
        Start-Process powershell.exe -ArgumentList "-Command `"Set-Location '$serverPath'; `$env:UNITY_PORT='$port'; node build/index.js | Tee-Object -FilePath '$logFile' -Append`"" -WindowStyle Hidden
        
        # Return to original directory
        Pop-Location
        
        # Wait for server to start
        $retry = 0
        $maxRetry = 10
        
        while (-not (Test-ServerRunning -port $port) -and $retry -lt $maxRetry) {
            Write-Host "Waiting for server to start... ($($retry+1)/$maxRetry)" -ForegroundColor Yellow
            Start-Sleep -Seconds 1
            $retry++
        }
        
        if (Test-ServerRunning -port $port) {
            Write-Host "MCP server started successfully on port $port" -ForegroundColor Green
        }
        else {
            Write-Host "Failed to start MCP server. Check log file: $logFile" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "Error starting MCP server: $_" -ForegroundColor Red
    }
}

# Function to stop the MCP server
function Stop-McpServer {
    param([string]$port)
    
    if (-not (Test-ServerRunning -port $port)) {
        Write-Host "No MCP server running on port $port" -ForegroundColor Yellow
        return
    }
    
    try {
        Write-Host "Stopping MCP server on port $port..." -ForegroundColor Cyan
        
        # Find and stop the process
        $processId = Get-ProcessIdOnPort -Port $port
        if ($processId) {
            Stop-Process -Id $processId -Force
            Write-Host "MCP server stopped successfully" -ForegroundColor Green
        }
        else {
            Write-Host "Could not find MCP server process" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "Error stopping MCP server: $_" -ForegroundColor Red
    }
}

# Function to show server status
function Show-ServerStatus {
    param([string]$port)
    
    Write-Host "Checking MCP server status..." -ForegroundColor Cyan
    
    if (Test-ServerRunning -port $port) {
        $listeningCount = 0
        $establishedCount = 0
        if (Get-Command netstat -ErrorAction SilentlyContinue) {
            $connections = netstat -ano | Select-String ":$port"
            $listeningCount = ($connections | Select-String "LISTENING").Count
            $establishedCount = ($connections | Select-String "ESTABLISHED").Count
        }
        
        Write-Host "MCP server is RUNNING on port $port" -ForegroundColor Green
        Write-Host "Listening connections: $listeningCount" -ForegroundColor Green
        Write-Host "Established connections: $establishedCount" -ForegroundColor Green
        
        # Try to get last log entries
        if (Test-Path $logFile) {
            Write-Host "`nLast 5 log entries:" -ForegroundColor Cyan
            Get-Content $logFile -Tail 5 | ForEach-Object { Write-Host "  $_" }
        }
    }
    else {
        Write-Host "MCP server is NOT RUNNING on port $port" -ForegroundColor Red
    }
}

# Main execution based on action parameter
switch ($Action) {
    "start" {
        Start-McpServer -port $Port
    }
    "stop" {
        Stop-McpServer -port $Port
    }
    "restart" {
        Stop-McpServer -port $Port
        Start-Sleep -Seconds 2
        Start-McpServer -port $Port
    }
    "status" {
        Show-ServerStatus -port $Port
    }
}

# Show usage instructions
Write-Host "`nUsage:" -ForegroundColor Cyan
Write-Host "  .\mcp-server-manager.ps1 -Action <start|stop|status|restart> -Port <port_number>" -ForegroundColor Cyan
Write-Host "  Default action: start" -ForegroundColor Cyan
Write-Host "  Default port: 8001" -ForegroundColor Cyan 
# run-mcp-server.ps1
# Purpose: Starts the MCP Unity server with proper environment configuration
# Usage: .\run-mcp-server.ps1 [-Port <port_number>] [-Monitor] [-ConfigFile <path>] [-EngineConfigFile <path>]

param (
    [int]$Port = 8001,
    [ValidateSet("unity","godot")]
    [string]$Engine = "unity",
    [switch]$Monitor,
    [string]$ConfigFile = ".mcp-server-config.json",
    [string]$EngineConfigFile = "engine-config.json",
    [string]$ServerConfigFile = "server-config.json"
)

# Validate the supplied port number
function Test-ValidPort {
    param([int]$Port)
    return ($Port -ge 1 -and $Port -le 65535)
}

if (-not (Test-ValidPort -Port $Port)) {
    Write-Error "Invalid port number $Port. Port must be between 1 and 65535." -ErrorAction Stop

}

# Load configuration from file if it exists
$config = @{
    "port" = $Port
    "serverPath" = $null
    "logFile" = $null
    "nodeEnv" = "production"
    "engine" = $Engine
    "runtime" = ($Engine -eq 'godot' ? 'dotnet' : 'node')
}
$portFromConfig = $false

if (Test-Path $ConfigFile) {
    try {
        $configData = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json
        
        # Override defaults with config file values
        if ($configData.port) { $config.port = $configData.port; $portFromConfig = $true }
        if ($configData.serverPath) { $config.serverPath = $configData.serverPath }
        if ($configData.logFile) { $config.logFile = $configData.logFile }
        if ($configData.nodeEnv) { $config.nodeEnv = $configData.nodeEnv }
        if ($configData.engine) { $config.engine = $configData.engine }
        if ($configData.runtime) { $config.runtime = $configData.runtime }
        
        Write-Host "Configuration loaded from $ConfigFile" -ForegroundColor Green
    }
    catch {
        Write-Warning "Failed to load configuration from $ConfigFile: $_"
    }
}

# If port was explicitly specified via parameter, override config file value
if ($PSBoundParameters.ContainsKey('Port')) {
    $config.port = $Port
}
if ($PSBoundParameters.ContainsKey('Engine')) {
    $config.engine = $Engine
}

# Define paths
$projectRoot = $PSScriptRoot
$unityProjectPath = Join-Path $projectRoot "TBD SpaceGame"
$godotServerPath = Join-Path $projectRoot "servers\godot"

# Load engine-specific defaults from the engine config file
if (Test-Path $EngineConfigFile) {
    try {
        $engineData = Get-Content -Path $EngineConfigFile -Raw | ConvertFrom-Json
        $engineEntry = $engineData.$($config.engine)
        if ($engineEntry) {
            if (-not $PSBoundParameters.ContainsKey('Port') -and -not $portFromConfig -and $engineEntry.port) {
                $config.port = [int]$engineEntry.port
            }
            if (-not $config.serverPath -and $engineEntry.directory) {
                $config.serverPath = Join-Path $projectRoot $engineEntry.directory
            }
        }
    } catch {
        Write-Warning "Failed to load engine configuration from $EngineConfigFile: $_"
    }
}

# Load shared runtime/port settings from the server config file
if (Test-Path $ServerConfigFile) {
    try {
        $serverData = Get-Content -Path $ServerConfigFile -Raw | ConvertFrom-Json
        $serverEntry = $serverData.$($config.engine)
        if ($serverEntry) {
            if (-not $PSBoundParameters.ContainsKey('Port') -and -not $portFromConfig -and $serverEntry.port) {
                $config.port = [int]$serverEntry.port
            }
            if ($serverEntry.engine) { $config.runtime = $serverEntry.engine }
        }
    } catch {
        Write-Warning "Failed to load server configuration from $ServerConfigFile: $_"
    }
}

# Use config serverPath if provided, otherwise use default
if (-not $config.serverPath) {
    if ($config.engine -eq 'godot') {
        $config.serverPath = $godotServerPath
    } else {
        $config.serverPath = Join-Path $unityProjectPath "TBD SpaceGame\Library\PackageCache\com.gamelovers.mcp-unity@3acfb232f564\Server"
    }
}

# Use config logFile if provided, otherwise use default
if (-not $config.logFile) {
    $config.logFile = Join-Path $projectRoot "mcp-server.log"
}

if ($config.runtime -eq 'dotnet') {
    try {
        $dotnetVersion = dotnet --version
        Write-Host "dotnet version: $dotnetVersion"
    } catch {
        Write-Error 'dotnet CLI is required to run the Godot MCP server.'
        exit 1
    }
} else {
    try {
        $nodeVersion = node --version
        Write-Host "Node.js version: $nodeVersion"
    } catch {
        Write-Error "Node.js is not installed or not in PATH. Please install Node.js to run the MCP server."
        exit 1
    }
}

# Validate server path
if (-not (Test-Path $config.serverPath)) {
    if ($config.runtime -eq 'dotnet') {
        Write-Error "Godot MCP Server path not found: $($config.serverPath)"
    } else {
        Write-Error "MCP Server path not found: $($config.serverPath)`nMake sure Unity is open and the MCP package is imported."
    }
    exit 1
}

# Function to check if port is in use
function Test-PortInUse {
    param (
        [int]$Port
    )

    try {
        if (Get-Command Test-NetConnection -ErrorAction SilentlyContinue) {
            return Test-NetConnection -ComputerName 'localhost' -Port $Port -InformationLevel Quiet
        }
        else {
            $client = [System.Net.Sockets.TcpClient]::new()
            $task = $client.ConnectAsync('localhost', $Port)
            $connected = $task.Wait(500)
            $client.Dispose()
            return $connected
        }
    }
    catch {
        return $false
    }
}

# Check if port is already in use
if (Test-PortInUse -Port $config.port) {
    Write-Warning "Port $($config.port) is already in use. This could be another instance of the MCP server."
    Write-Warning "If you're having connection issues, try stopping other servers or using a different port."
}

# Function to start the MCP server
function Start-McpServer {
    Write-Host "Starting MCP Persistent Server using $($config.engine) engine ($($config.runtime))..."
    Write-Host "Starting MCP server on port $($config.port)..."

    try {
        Push-Location $config.serverPath

        if ($config.runtime -eq 'dotnet') {
            $env:PORT = $config.port.ToString()
            $processInfo = Start-Process -FilePath "dotnet" -ArgumentList "run --project GodotServer.csproj --no-build" -RedirectStandardOutput $config.logFile -RedirectStandardError $config.logFile -NoNewWindow -PassThru
        }
        else {
            $env:UNITY_PORT = $config.port.ToString()
            $env:NODE_ENV = $config.nodeEnv
            $processInfo = Start-Process -FilePath "node" -ArgumentList "build/index.js" -RedirectStandardOutput $config.logFile -RedirectStandardError $config.logFile -NoNewWindow -PassThru
        }

        Pop-Location

        Write-Host "MCP server started on port $($config.port) with process ID: $($processInfo.Id)" -ForegroundColor Green
        Write-Host "Output is being logged to: $($config.logFile)"
        Write-Host "To test the connection, run: .\test-menu.ps1 -MenuPath `"GameObject/Create Empty`""

        return $processInfo.Id
    }
    catch {
        if ($pwd.Path -eq $config.serverPath) {
            Pop-Location
        }

        Write-Error "Failed to start MCP server: $_"
        return $null
    }
}

# Function to check if server is running
function Test-ServerRunning {
    param (
        [int]$ProcessId,
        [int]$Port
    )
    
    # Check process
    if ($ProcessId -and (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue)) {
        # Process exists, now check if it's listening on the port
        if (Test-PortInUse -Port $Port) {
            return $true
        }
    }
    
    return $false
}

# Start the server
$processId = Start-McpServer

# Verify server is running by checking if port is listening
Start-Sleep -Seconds 2
$serverRunning = $false
try {
    if (Test-PortInUse -Port $config.port) {
        $serverRunning = $true
        Write-Host "Server is now listening on port $($config.port)." -ForegroundColor Green
    }
}
catch {
    # Ignore errors in the verification check
}

if (-not $serverRunning) {
    Write-Warning "Unable to verify if server is running. Check the log file: $($config.logFile)"
}

# Monitor the server if requested
if ($Monitor -and $processId) {
    Write-Host "Starting server monitoring..." -ForegroundColor Yellow
    
    try {
        while ($true) {
            Start-Sleep -Seconds 30
            
            if (-not (Test-ServerRunning -ProcessId $processId -Port $config.port)) {
                Write-Warning "MCP server is not running. Attempting to restart..."
                
                # Try to cleanup old process if it's hung
                try {
                    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                }
                catch {
                    # Ignore cleanup errors
                }
                
                # Restart the server
                $processId = Start-McpServer
                if (-not $processId) {
                    Write-Error "Failed to restart MCP server."
                    exit 1
                }
                
                Write-Host "MCP server restarted with process ID: $processId" -ForegroundColor Green
            }
            else {
                Write-Host "MCP server is running." -ForegroundColor Green
            }
        }
    }
    catch {
        Write-Error "Error in server monitoring: $_"
        exit 1
    }
    finally {
        # Clean up if script is interrupted
        Write-Host "Server monitoring stopped." -ForegroundColor Yellow
    }
}

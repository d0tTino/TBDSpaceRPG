# Restart MCP After Unity Update
# This script handles MCP server reconnection after Unity updates

# Function to find and terminate existing Node.js processes running the MCP server
function Stop-McpNodeProcesses {
    Write-Host "Looking for existing MCP server processes..."
    
    # Find Node.js processes that are running MCP server
    $mcpProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue | 
        Where-Object { $_.CommandLine -like "*build/index.js*" -or $_.CommandLine -like "*mcp-persistent-server.js*" }
    
    if ($mcpProcesses -and $mcpProcesses.Count -gt 0) {
        Write-Host "Found $($mcpProcesses.Count) MCP server processes. Stopping them..."
        
        foreach ($process in $mcpProcesses) {
            try {
                Stop-Process -Id $process.Id -Force
                Write-Host "Stopped process with ID $($process.Id)"
            }
            catch {
                Write-Host "Error stopping process with ID $($process.Id): $($_.Exception.Message)"
            }
        }
        
        # Wait for processes to fully terminate
        Start-Sleep -Seconds 2
    }
    else {
        Write-Host "No existing MCP server processes found."
    }
}

# Function to check if port is in use
function Test-PortInUse {
    param (
        [int]$Port
    )
    
    try {
        $portString = [string]$Port
        $connections = netstat -ano | findstr ":$portString "
        return ($null -ne $connections -and $connections.Length -gt 0)
    }
    catch {
        return $false
    }
}

# Function to free up the port if it's in use
function Free-Port {
    param (
        [int]$Port
    )
    
    Write-Host "Checking if port $Port is in use..."
    
    if (Test-PortInUse -Port $Port) {
        Write-Host "Port $Port is in use. Attempting to free it..."
        
        try {
            # Get the process ID using the port
            $portString = [string]$Port
            $processInfo = netstat -ano | findstr ":$portString " | Select-Object -First 1
            if ($processInfo) {
                $processIdString = $processInfo.Trim().Split(' ')[-1]
                $processId = [int]$processIdString
                
                # Kill the process
                Stop-Process -Id $processId -Force
                Write-Host "Stopped process with ID $processId that was using port $Port"
                
                # Wait briefly to ensure the port is released
                Start-Sleep -Seconds 2
            }
        }
        catch {
            Write-Host "Error freeing port $Port`: $($_.Exception.Message)"
        }
    }
    else {
        Write-Host "Port $Port is free."
    }
}

# Function to start the MCP server
function Start-McpServer {
    param (
        [string]$ServerPath,
        [int]$Port
    )
    
    Write-Host "Starting MCP server on port $Port..."
    
    try {
        # Set environment variable for the port
        $env:UNITY_PORT = $Port
        
        # Start the MCP persistent server
        $serverScript = Join-Path $PSScriptRoot "mcp-persistent-server.js"
        Start-Process -FilePath "node" -ArgumentList $serverScript -NoNewWindow
        
        Write-Host "MCP server started successfully on port $Port"
        return $true
    }
    catch {
        Write-Host "Error starting MCP server: $($_.Exception.Message)"
        return $false
    }
}

# Function to test if MCP server is responding
function Test-McpServerConnection {
    param (
        [int]$Port,
        [int]$Retries = 5,
        [int]$RetryDelay = 1000
    )
    
    Write-Host "Testing connection to MCP server on port $Port..."
    
    $attempt = 0
    $connected = $false
    
    while ($attempt -lt $Retries -and -not $connected) {
        try {
            # Try to establish a socket connection to the server
            $client = New-Object System.Net.Sockets.TcpClient
            $client.ConnectAsync("localhost", $Port).Wait(1000)
            
            if ($client.Connected) {
                $client.Close()
                Write-Host "Successfully connected to MCP server on port $Port"
                $connected = $true
            }
            else {
                $attempt++
                Write-Host "Attempt $attempt to connect failed. Retrying in $($RetryDelay/1000) seconds..."
                Start-Sleep -Milliseconds $RetryDelay
            }
        }
        catch {
            $attempt++
            Write-Host "Attempt $attempt to connect failed with error: $($_.Exception.Message). Retrying in $($RetryDelay/1000) seconds..."
            Start-Sleep -Milliseconds $RetryDelay
        }
        finally {
            if ($null -ne $client) {
                $client.Dispose()
            }
        }
    }
    
    return $connected
}

# Function to validate the MCP configuration
function Test-McpConfig {
    Write-Host "Validating MCP configuration..."
    
    $configPath = "$env:USERPROFILE\.cursor\mcp.json"
    
    if (Test-Path $configPath) {
        try {
            $config = Get-Content $configPath -Raw | ConvertFrom-Json
            
            # Check if the necessary properties exist
            if ($config.mcpServers -and $config.mcpServers.'mcp-unity') {
                $primaryServer = $config.mcpServers.'mcp-unity'
                
                # Check workspace path
                if (-not $primaryServer.workspace -or -not (Test-Path $primaryServer.workspace)) {
                    Write-Host "Warning: Workspace path is missing or invalid in MCP configuration."
                    return $false
                }
                
                # Check transport
                if ($primaryServer.transport -ne "ws") {
                    Write-Host "Warning: Transport is not set to 'ws' in MCP configuration."
                    return $false
                }
                
                Write-Host "MCP configuration is valid."
                return $true
            }
            else {
                Write-Host "Error: MCP configuration is missing required server information."
                return $false
            }
        }
        catch {
            Write-Host "Error parsing MCP configuration: $($_.Exception.Message)"
            return $false
        }
    }
    else {
        Write-Host "Error: MCP configuration file not found at $configPath"
        return $false
    }
}

# Main script execution
Write-Host "===== MCP Server Restart After Unity Update ====="

# Step 1: Stop any existing MCP server processes
Stop-McpNodeProcesses

# Step 2: Free up the port
Free-Port -Port 8001

# Step 3: Validate MCP configuration
$configValid = Test-McpConfig

if (-not $configValid) {
    Write-Host "Warning: MCP configuration issues detected. Continuing anyway, but this might cause problems."
}

# Step 4: Determine server path (using package path for Unity)
$serverPath = "TBD SpaceGame/TBD SpaceGame/Library/PackageCache/com.gamelovers.mcp-unity@3acfb232f564/Server"

# Step 5: Start the MCP server
$serverStarted = Start-McpServer -ServerPath $serverPath -Port 8001

if ($serverStarted) {
    # Step 6: Test the connection
    $connected = Test-McpServerConnection -Port 8001 -Retries 10 -RetryDelay 2000
    
    if ($connected) {
        Write-Host "MCP server is now running and ready to accept connections from Unity."
        Write-Host ""
        Write-Host "IMPORTANT: Use the V2 menu items in Unity to avoid conflicts:"
        Write-Host "- MCP/Ship/V2/Create Test Ship"
        Write-Host "- MCP/Ship/V2/Upgrade Thrust"
        Write-Host "- MCP/Ship/V2/Upgrade Speed"
        Write-Host "- MCP/Ship/V2/Upgrade Rotation"
        Write-Host "- MCP/Ship/V2/Upgrade Braking"
        Write-Host "- MCP/Ship/V2/Log Current Upgrades"
        Write-Host ""
        Write-Host "These menu items use the new ShipUpgradeMenuHelperV2 class."
    }
    else {
        Write-Host "Failed to connect to MCP server after starting it. Check for errors in the output above."
    }
}
else {
    Write-Host "Failed to start MCP server. Check for errors in the output above."
} 
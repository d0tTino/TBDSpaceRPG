# test-menu.ps1
# Purpose: Test MCP menu items by sending commands to the MCP server
# Usage: .\test-menu.ps1 -MenuPath "Path/To/Menu/Item"
#        .\test-menu.ps1 -MenuPaths "Path1","Path2","Path3"

param (
    [Parameter(Mandatory=$true, ParameterSetName="SinglePath")]
    [string]$MenuPath,
    
    [Parameter(Mandatory=$true, ParameterSetName="MultiplePaths")]
    [string[]]$MenuPaths,
    
    [int]$Port = 8001,

    [switch]$Verbose
)

function Test-ValidPort {
    param([int]$Port)
    return ($Port -ge 1 -and $Port -le 65535)
}

if (-not (Test-ValidPort -Port $Port)) {
    Write-Error "Invalid port number $Port. Port must be between 1 and 65535." -ErrorAction Stop
}

# Import required modules
Add-Type -AssemblyName System.Net.WebSockets.Client

# Define constants and variables
$webSocketUrl = "ws://localhost:$Port/McpUnity"
$logFile = Join-Path $PSScriptRoot "mcp-client.log"
$timeoutMs = 10000 # 10 seconds

# Create log file if it doesn't exist
if (-not (Test-Path $logFile)) {
    New-Item -Path $logFile -ItemType File -Force | Out-Null
}

function Write-Log {
    param (
        [string]$Message,
        [string]$Level = "INFO"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    # Always add to log file
    Add-Content -Path $logFile -Value $logMessage
    
    # Output to console based on level
    switch ($Level) {
        "ERROR" { Write-Host $logMessage -ForegroundColor Red }
        "WARNING" { Write-Host $logMessage -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $logMessage -ForegroundColor Green }
        default { 
            if ($Verbose) {
                Write-Host $logMessage -ForegroundColor Gray
            }
        }
    }
}

function Test-Server {
    # Check if the server is running by testing if the port is in use
    try {
        $connection = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
        if ($connection) {
            return $true
        }
    }
    catch {
        # Ignore errors
    }
    
    return $false
}

function Send-WebSocketRequest {
    param (
        [string]$MenuPath
    )
    
    Write-Log "Connecting to WebSocket at $webSocketUrl..."
    
    try {
        # Create a cancellation token source for timeout
        $cts = New-Object System.Threading.CancellationTokenSource
        $cts.CancelAfter($timeoutMs)
        
        # Create client WebSocket
        $clientWs = New-Object System.Net.WebSockets.ClientWebSocket
        $connectTask = $clientWs.ConnectAsync($webSocketUrl, $cts.Token)
        
        # Wait for connection (with timeout)
        if (-not $connectTask.Wait($timeoutMs)) {
            Write-Log "Connection timed out after $($timeoutMs)ms" "ERROR"
            return $false
        }
        
        # Check connection state
        if ($clientWs.State -ne [System.Net.WebSockets.WebSocketState]::Open) {
            Write-Log "Failed to connect to MCP server. WebSocket state: $($clientWs.State)" "ERROR"
            return $false
        }
        
        Write-Log "Connected to MCP server" "SUCCESS"
        
        # Create payload for executing menu item
        $requestId = [guid]::NewGuid().ToString()
        $payload = @{
            "type" = "execute_menu_item"
            "id" = $requestId
            "parameters" = @{
                "menuPath" = $MenuPath
            }
        } | ConvertTo-Json
        
        # Convert payload to bytes
        $payloadBytes = [System.Text.Encoding]::UTF8.GetBytes($payload)
        $segment = New-Object System.ArraySegment[byte] -ArgumentList @(,$payloadBytes)
        
        # Send the message
        Write-Log "Sending request: $payload"
        $sendTask = $clientWs.SendAsync(
            $segment, 
            [System.Net.WebSockets.WebSocketMessageType]::Text,
            $true,
            $cts.Token
        )
        
        if (-not $sendTask.Wait($timeoutMs)) {
            Write-Log "Send operation timed out" "ERROR"
            return $false
        }
        
        Write-Log "Request sent successfully (ID: $requestId)" "SUCCESS"
        
        # Prepare to receive response
        $receiveBuffer = New-Object byte[] 8192
        $receiveSegment = New-Object System.ArraySegment[byte] -ArgumentList @(,$receiveBuffer)
        
        # Receive the response
        $response = ""
        $receiving = $true
        
        while ($receiving) {
            $receiveResult = $clientWs.ReceiveAsync($receiveSegment, $cts.Token).GetAwaiter().GetResult()
            
            if ($receiveResult.MessageType -eq [System.Net.WebSockets.WebSocketMessageType]::Close) {
                Write-Log "WebSocket closed by server" "INFO"
                $receiving = $false
            }
            else {
                $responseChunk = [System.Text.Encoding]::UTF8.GetString($receiveBuffer, 0, $receiveResult.Count)
                $response += $responseChunk
                
                if ($receiveResult.EndOfMessage) {
                    $receiving = $false
                }
            }
        }
        
        # Process response
        if ($response) {
            try {
                $responseObj = $response | ConvertFrom-Json
                
                if ($responseObj.error) {
                    Write-Log "Error from server: $($responseObj.error)" "ERROR"
                    return $false
                }
                else {
                    # Validate response ID matches request ID
                    if ($responseObj.id -ne $requestId) {
                        Write-Log "Warning: Response ID ($($responseObj.id)) does not match request ID ($requestId)" "WARNING"
                    }
                    
                    Write-Log "Command execution initiated in Unity (ID: $($responseObj.id))" "SUCCESS"
                    Write-Log "Response: $response" "INFO"
                    return $true
                }
            }
            catch {
                Write-Log "Failed to parse response: $response" "ERROR"
                Write-Log "Error: $_" "ERROR"
                return $false
            }
        }
        else {
            Write-Log "No response received from server" "WARNING"
            return $false
        }
    }
    catch {
        Write-Log "WebSocket error: $_" "ERROR"
        return $false
    }
    finally {
        # Clean up
        if ($clientWs -and $clientWs.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
            try {
                $closeTask = $clientWs.CloseAsync(
                    [System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure,
                    "Done",
                    [System.Threading.CancellationToken]::None
                )
                $closeTask.Wait(5000) | Out-Null
                Write-Log "WebSocket connection closed" "INFO"
            }
            catch {
                Write-Log "Error closing WebSocket: $_" "WARNING"
            }
        }
        
        if ($cts) {
            $cts.Dispose()
        }
    }
}

# Function to process multiple menu paths
function Process-MenuPaths {
    param (
        [string[]]$Paths
    )
    
    $totalPaths = $Paths.Count
    $successCount = 0
    $failCount = 0
    
    Write-Host "Processing $totalPaths menu items..." -ForegroundColor Cyan
    
    foreach ($path in $Paths) {
        Write-Host "Testing menu item: $path" -ForegroundColor Cyan
        
        # Execute menu item
        $result = Send-WebSocketRequest -MenuPath $path
        
        if ($result) {
            $successCount++
            Write-Host "✓ Success: $path" -ForegroundColor Green
        }
        else {
            $failCount++
            Write-Host "✗ Failed: $path" -ForegroundColor Red
        }
        
        # Add a small delay between requests
        Start-Sleep -Seconds 1
    }
    
    # Return results summary
    return @{
        Total = $totalPaths
        Success = $successCount
        Failed = $failCount
    }
}

# Main execution
Write-Log "Starting menu item test for MCP server on port $Port" "INFO"

# Determine execution mode
$pathsToTest = @()
if ($PSCmdlet.ParameterSetName -eq "SinglePath") {
    $pathsToTest = @($MenuPath)
    Write-Log "Testing single menu item: $MenuPath" "INFO"
}
else {
    $pathsToTest = $MenuPaths
    Write-Log "Testing multiple menu items: $($MenuPaths.Count) items" "INFO"
}

# Verify server is running
if (-not (Test-Server)) {
    Write-Log "MCP server does not appear to be running on port $Port." "WARNING"
    Write-Log "Start the server using .\run-mcp-server.ps1 before running this script." "WARNING"
    
    $startServer = Read-Host "Would you like to start the server now? (y/n)"
    if ($startServer -eq "y") {
        & "$PSScriptRoot\run-mcp-server.ps1" -Port $Port
        Start-Sleep -Seconds 2 # Give the server time to start
    }
    else {
        Write-Log "Exiting due to missing server." "ERROR"
        exit 1
    }
}

# Execute menu items
$results = Process-MenuPaths -Paths $pathsToTest

# Report results
if ($results.Failed -eq 0) {
    Write-Host "`n✅ All menu items executed successfully ($($results.Success)/$($results.Total))" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "`n❌ Some menu items failed: $($results.Failed)/$($results.Total) failed" -ForegroundColor Red
    exit 1
} 
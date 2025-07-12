# MCP Menu Command Tester
# A script to test MCP menu commands with better error handling

param(
    [Parameter(Mandatory=$true)]
    [string]$MenuPath,
    
    [Parameter(Mandatory=$false)]
    [string]$Port = "8001",
    
    [Parameter(Mandatory=$false)]
    [switch]$VerifyServerFirst = $true
)

$ErrorActionPreference = "Stop"

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

# Function to verify menu paths exist in Unity
function Test-UnityMenuPath {
    param([string]$menuPath)
    
    # These are common Unity menu paths that are likely to exist
    $knownMenuPaths = @(
        "GameObject/Create Empty",
        "GameObject/3D Object/Cube",
        "GameObject/3D Object/Sphere",
        "Edit/Duplicate",
        "Edit/Delete",
        "Window/General/Console"
    )
    
    # Built-in Unity menu paths are likely to work
    foreach ($known in $knownMenuPaths) {
        if ($menuPath -eq $known) {
            return $true
        }
    }
    
    # For custom menu paths, we can only warn
    if ($menuPath -notmatch "^(GameObject|Edit|Assets|Window|Help)/") {
        Write-Host "WARNING: '$menuPath' appears to be a custom menu path. Ensure it's correctly registered in Unity." -ForegroundColor Yellow
        Write-Host "TIP: Place menu scripts in Assets/Plugins folder and restart Unity to ensure registration." -ForegroundColor Yellow
    }
    
    return $true
}

# Function to execute MCP menu command
function Invoke-McpMenuCommand {
    param(
        [string]$menuPath,
        [string]$port
    )
    
    try {
        Write-Host "`n=== MCP MENU COMMAND TEST ===" -ForegroundColor Cyan
        Write-Host "Menu Path: $menuPath" -ForegroundColor Cyan
        Write-Host "Port: $port" -ForegroundColor Cyan
        Write-Host "-------------------------------" -ForegroundColor Cyan
        
        # Verify server is running if requested
        if ($VerifyServerFirst) {
            if (-not (Test-ServerRunning -port $port)) {
                Write-Host "ERROR: MCP server is not running on port $port!" -ForegroundColor Red
                Write-Host "Start the server with: .\mcp-server-manager.ps1 -Action start -Port $port" -ForegroundColor Yellow
                return
            }
            Write-Host "✓ MCP server is running on port $port" -ForegroundColor Green
        }
        
        # Basic menu path validation
        if (-not (Test-UnityMenuPath -menuPath $menuPath)) {
            return
        }
        
        # Connect to WebSocket and send command
        Write-Host "Connecting to MCP WebSocket..." -ForegroundColor Cyan
        
        # Load WebSocket client
        Add-Type -AssemblyName System.Net.WebSockets.Client
        
        # Create WebSocket client
        $client = New-Object System.Net.WebSockets.ClientWebSocket
        $uri = New-Object System.Uri "ws://localhost:$port/McpUnity"
        $cts = New-Object System.Threading.CancellationTokenSource
        $cts.CancelAfter(5000) # 5 second timeout
        
        # Connect to WebSocket
        Write-Host "Connecting to $uri..." -ForegroundColor Yellow
        $client.ConnectAsync($uri, $cts.Token).GetAwaiter().GetResult()
        
        if ($client.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
            Write-Host "✓ Connected successfully" -ForegroundColor Green
            
            # Create message
            $command = @{
                method = "execute_menu_item"
                params = @{
                    menuPath = $menuPath
                }
                id = [Guid]::NewGuid().ToString()
            } | ConvertTo-Json -Compress
            
            # Send command
            Write-Host "Sending command: $command" -ForegroundColor Yellow
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($command)
            $segment = New-Object System.ArraySegment[byte] -ArgumentList @(,$bytes)
            $client.SendAsync($segment, [System.Net.WebSockets.WebSocketMessageType]::Text, $true, $cts.Token).GetAwaiter().GetResult()
            
            # Create buffer for response
            Write-Host "Waiting for response..." -ForegroundColor Yellow
            $buffer = New-Object byte[] 8192
            $receiveSegment = New-Object System.ArraySegment[byte] -ArgumentList @(,$buffer)
            
            # Wait for response with timeout
            try {
                $receiveResult = $client.ReceiveAsync($receiveSegment, $cts.Token).GetAwaiter().GetResult()
                $response = [System.Text.Encoding]::UTF8.GetString($buffer, 0, $receiveResult.Count)
                Write-Host "Response received: $response" -ForegroundColor Green
                
                # Check for success/error in response
                $responseObj = $response | ConvertFrom-Json
                if ($responseObj.result.success -eq $false) {
                    Write-Host "ERROR: $($responseObj.result.message)" -ForegroundColor Red
                }
            }
            catch [System.Threading.Tasks.TaskCanceledException] {
                Write-Host "No response received within timeout period" -ForegroundColor Yellow
                Write-Host "TIP: This is normal - check Unity console for results" -ForegroundColor Yellow
            }
            catch {
                Write-Host "Error receiving response: $_" -ForegroundColor Red
            }
            
            # Close WebSocket connection
            if ($client.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
                $client.CloseAsync([System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure, "Done", $cts.Token).GetAwaiter().GetResult()
            }
            
            Write-Host "✓ Command sent successfully" -ForegroundColor Green
            Write-Host "Check Unity console for results!" -ForegroundColor Cyan
        }
        else {
            Write-Host "ERROR: Failed to connect to WebSocket. State: $($client.State)" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "ERROR: $($_)" -ForegroundColor Red
    }
}

# Main execution
Invoke-McpMenuCommand -menuPath $MenuPath -port $Port

# Show usage examples
Write-Host "`nUsage Examples:" -ForegroundColor Cyan
Write-Host "  .\test-mcp-menu.ps1 -MenuPath `"GameObject/Create Empty`"" -ForegroundColor Cyan
Write-Host "  .\test-mcp-menu.ps1 -MenuPath `"Help/About Unity`"" -ForegroundColor Cyan
Write-Host "  .\test-mcp-menu.ps1 -MenuPath `"Basic/Test`" -Port 8001" -ForegroundColor Cyan 
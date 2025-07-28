# Simplified PowerShell WebSocket client for the MCP server
# Example:
#   ./mcpWebSocketClient-simple.ps1 -menuPath "MCP/Test/Ping" -Engine unity
param (
    [Parameter(Mandatory=$false)]
    [string]$menuPath,

    [Parameter(Mandatory=$false)]
    [string]$message,

    [Parameter(Mandatory=$false)]
    [string]$logType = "Log"  # Can be: Log, Warning, Error,

    [ValidateSet('godot','unity')]
    [string]$Engine = 'godot',

    [string]$ConfigFile = 'engine-config.json'
)

function Test-ValidPort {
    param([int]$Port)
    return $Port -ge 1 -and $Port -le 65535
}

# Validate parameters
if (!$menuPath -and !$message) {
    Write-Host "Error: You must provide either -menuPath or -message parameter"
    exit 1
}

# Determine operation type
$operationType = if ($menuPath) { "execute_menu_item" } else { "notify_message" }
$targetName = if ($menuPath) { $menuPath } else { $message }

Write-Host "Connecting to MCP server and performing operation: $operationType with target: $targetName"

try {
    # Create client WebSocket
    $client = New-Object System.Net.WebSockets.ClientWebSocket

    $port = $null
    if (Test-Path $ConfigFile) {
        try {
            $cfg = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json
            if ($cfg.$Engine -and $cfg.$Engine.port) { $port = [int]$cfg.$Engine.port }
        } catch {
            Write-Warning "Failed to load engine config from $ConfigFile: $_"
        }
    }
    if (-not $port) {
        $portsPath = Join-Path $PSScriptRoot 'servers/ports.cjs'
        if ($Engine -eq 'godot') {
            $port = [int](node -e "const p=require(process.argv[1]);console.log(p.godot);" $portsPath)
        } else {
            $port = [int](node -e "const p=require(process.argv[1]);console.log(p.unity);" $portsPath)
        }
    }
    $url = if ($Engine -eq 'godot') {
        "ws://localhost:$port/"
    } else {
        "ws://localhost:$port/McpUnity"
    }
    $uri = New-Object System.Uri($url)

    if (-not (Test-ValidPort -Port $uri.Port)) {
        Write-Host "Invalid port in WebSocket URI: $($uri.Port)" -ForegroundColor Red
        return
    }
    
    # Connect to the server
    $connectTask = $client.ConnectAsync($uri, [System.Threading.CancellationToken]::None)
    Write-Host "Connecting to WebSocket at $uri..."
    $connectTask.Wait(5000) # Wait up to 5 seconds
    
    if ($client.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
        Write-Host "WebSocket connected successfully"
        
        # Create request message based on operation type
        if ($operationType -eq "execute_menu_item") {
            $requestObj = @{
                method = $operationType
                params = @{
                    menuPath = $menuPath
                }
                id = [guid]::NewGuid().ToString()
            }
        } else {
            $requestObj = @{
                method = $operationType
                params = @{
                    message = $message
                    logType = $logType
                }
                id = [guid]::NewGuid().ToString()
            }
        }
        
        $requestJson = $requestObj | ConvertTo-Json -Depth 3
        Write-Host "Sending request: $requestJson"
        
        # Convert to bytes and send
        $requestBytes = [System.Text.Encoding]::UTF8.GetBytes($requestJson)
        $segment = New-Object System.ArraySegment[byte] -ArgumentList @(,$requestBytes)
        
        $sendTask = $client.SendAsync($segment, [System.Net.WebSockets.WebSocketMessageType]::Text, $true, [System.Threading.CancellationToken]::None)
        $sendTask.Wait(5000)
        Write-Host "Request sent successfully"
        
        # Allow a small delay for the command to be processed
        Start-Sleep -Seconds 1
        
        # No waiting for response in this simplified version
        Write-Host "Command execution initiated" -ForegroundColor Green
        
        # Close the connection
        $closeTask = $client.CloseAsync([System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure, "Closing", [System.Threading.CancellationToken]::None)
        $closeTask.Wait(5000)
        Write-Host "WebSocket connection closed"
    } else {
        Write-Host "Failed to connect to WebSocket. State: $($client.State)"
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    
    # Get detailed exception info
    if ($_.Exception -is [AggregateException] -and $_.Exception.InnerExceptions.Count -gt 0) {
        Write-Host "Inner exception: $($_.Exception.InnerExceptions[0].Message)" -ForegroundColor Red
    }
}
finally {
    if ($client -ne $null) {
        if ($client.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
            $client.Dispose()
        }
    }
}

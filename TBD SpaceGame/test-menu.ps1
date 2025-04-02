# Simple script to test menu items
param(
    [Parameter(Mandatory=$false)]
    [string]$MenuPath = "Basic/Test",
    
    [Parameter(Mandatory=$false)]
    [string]$Port = "8001"
)

Write-Host "Testing menu item: $MenuPath on port $Port" -ForegroundColor Cyan

# Load WebSocket client
Add-Type -AssemblyName System.Net.WebSockets.Client

# Create WebSocket client
try {
    $client = New-Object System.Net.WebSockets.ClientWebSocket
    $uri = New-Object System.Uri "ws://localhost:$Port/McpUnity"
    $cts = New-Object System.Threading.CancellationTokenSource
    $cts.CancelAfter(5000) # 5 second timeout
    
    # Connect to WebSocket
    Write-Host "Connecting to WebSocket..." -ForegroundColor Yellow
    $client.ConnectAsync($uri, $cts.Token).GetAwaiter().GetResult()
    
    if ($client.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
        Write-Host "Connected!" -ForegroundColor Green
        
        # Create message
        $message = @{
            method = "execute_menu_item"
            params = @{
                menuPath = $MenuPath
            }
            id = [guid]::NewGuid().ToString()
        } | ConvertTo-Json
        
        # Send message
        Write-Host "Sending message: $message" -ForegroundColor Yellow
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($message)
        $segment = New-Object System.ArraySegment[byte] -ArgumentList @(,$bytes)
        $client.SendAsync($segment, [System.Net.WebSockets.WebSocketMessageType]::Text, $true, $cts.Token).GetAwaiter().GetResult()
        
        # Wait for a second
        Start-Sleep -Seconds 1
        
        # Close connection
        $client.CloseAsync([System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure, "Done", $cts.Token).GetAwaiter().GetResult()
        
        Write-Host "Command sent successfully!" -ForegroundColor Green
        Write-Host "Check Unity console for results" -ForegroundColor Cyan
    }
    else {
        Write-Host "Failed to connect to WebSocket. State: $($client.State)" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Show examples
Write-Host "`nExamples:" -ForegroundColor Cyan
Write-Host "  .\test-menu.ps1 -MenuPath `"Basic/Test`"" -ForegroundColor Cyan
Write-Host "  .\test-menu.ps1 -MenuPath `"GameObject/Create Empty`"" -ForegroundColor Cyan 
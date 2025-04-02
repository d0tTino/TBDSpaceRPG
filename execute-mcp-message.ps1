# PowerShell script to send a test message to Unity console
param (
    [Parameter(Mandatory=$true)]
    [string]$message
)

# Output what we're doing
Write-Host "Sending message to Unity: $message"

# Form the JSON payload
$payload = @{
    method = "notify_message"
    params = @{
        message = $message
        logType = "Log"  # Can be: Log, Warning, Error
    }
    id = [guid]::NewGuid().ToString()
} | ConvertTo-Json -Depth 3

# Execute the command using Invoke-WebRequest
try {
    $result = Invoke-WebRequest -Uri "http://localhost:8001/mcp" -Method Post -Body $payload -ContentType "application/json"
    Write-Host "Result: $($result.Content)"
}
catch {
    Write-Host "Error executing MCP command: $_"
} 
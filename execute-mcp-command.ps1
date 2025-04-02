# PowerShell script to execute MCP commands
param (
    [Parameter(Mandatory=$true)]
    [string]$menuPath
)

# Output what we're doing
Write-Host "Executing MCP menu item: $menuPath"

# Form the JSON payload
$payload = @{
    method = "execute_menu_item"
    params = @{
        menuPath = $menuPath
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
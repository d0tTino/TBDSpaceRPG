# PowerShell script to execute MCP commands
param (
    [Parameter(Mandatory=$true)]
    [string]$menuPath,
    [string]$Endpoint,
    [string]$ConfigFile = "engine-config.json"
)

# Output what we're doing
Write-Host "Executing MCP menu item: $menuPath"

function Test-ValidPort {
    param([int]$Port)
    return $Port -ge 1 -and $Port -le 65535
}

$uri = $null

if ($PSBoundParameters.ContainsKey('Endpoint')) {
    $uri = $Endpoint
} elseif (Test-Path $ConfigFile) {
    try {
        $cfg = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json
        if ($cfg.unity -and $cfg.unity.port) {
            $uri = "http://localhost:$($cfg.unity.port)/mcp"
        }
    } catch {
        Write-Warning "Failed to load engine config from $ConfigFile: $_"
    }
}

if (-not $uri) {
    $uri = "http://localhost:8001/mcp"
}

$port = ([System.Uri]$uri).Port
if (-not (Test-ValidPort -Port $port)) {
    Write-Error "Invalid port: $port"
    exit 1
}

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
    $result = Invoke-WebRequest -Uri $uri -Method Post -Body $payload -ContentType "application/json"
    Write-Host "Result: $($result.Content)"
}
catch {
    Write-Host "Error executing MCP command: $_"
}

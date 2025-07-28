# PowerShell script to execute MCP commands
# Example:
#   ./execute-mcp-command.ps1 -menuPath "GameObject/Create Empty" -Engine unity
param (
    [Parameter(Mandatory=$true)]
    [string]$menuPath,
    [string]$Endpoint,
    [ValidateSet('godot','unity')]
    [string]$Engine = 'godot',
    [string]$ConfigFile = "engine-config.json"
)

# Output what we're doing
Write-Host "Executing MCP menu item: $menuPath"

function Test-ValidPort {
    param([int]$Port)
    return $Port -ge 1 -and $Port -le 65535
}

$uri = $null
$port = $null

if ($PSBoundParameters.ContainsKey('Endpoint')) {
    $uri = $Endpoint
    $port = ([System.Uri]$uri).Port
} else {
    if (Test-Path $ConfigFile) {
        try {
            $cfg = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json
            if ($cfg.$Engine -and $cfg.$Engine.port) {
                $port = [int]$cfg.$Engine.port
            }
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
    $uri = "http://localhost:$port/mcp"
}

if (-not $port) {
    $port = ([System.Uri]$uri).Port
}
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

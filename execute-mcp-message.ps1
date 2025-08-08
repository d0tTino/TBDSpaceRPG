# PowerShell script to send a test message to the MCP server
# Example:
#   ./execute-mcp-message.ps1 -message "Hello" -Engine unity
param (
    [Parameter(Mandatory=$true)]
    [string]$message,
    [string]$Endpoint,
    [ValidateSet('godot','unity')]
    [string]$Engine = 'godot',
    [string]$ConfigFile = "engine-config.json"
)

if ([string]::IsNullOrWhiteSpace($message)) {
    Write-Error "message cannot be empty"
    exit 1
}

# Output what we're doing
Write-Host "Sending message to MCP server: $message"

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
$payload = [PSCustomObject]@{
    method = "notify_message"
    params = @{ message = [string]$message; logType = "Log" }
    id = [guid]::NewGuid().ToString()
} | ConvertTo-Json -Depth 3 -Compress

# Execute the command using Invoke-WebRequest
try {
    $result = Invoke-WebRequest -Uri $uri -Method Post -Body $payload -ContentType "application/json"
    Write-Host "Result: $($result.Content)"
}
catch {
    Write-Host "Error executing MCP command: $_"
}

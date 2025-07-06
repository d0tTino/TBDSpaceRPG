# update-mcp-config.ps1
# Purpose: Update the fallback server workspace path in the MCP config file
# Usage: .\update-mcp-config.ps1 [-ConfigPath <path>] [-Workspace <path>]

param(
    [string]$ConfigPath = (Join-Path $Env:USERPROFILE '.cursor/mcp.json'),
    [Parameter(Mandatory=$true)]
    [string]$Workspace
)

# Validate the configuration file exists
if (-not (Test-Path $ConfigPath)) {
    Write-Host "MCP configuration file not found at $ConfigPath"
    return
}

Write-Host "Found MCP configuration at $ConfigPath"

# Read the current configuration
$config = Get-Content -Path $ConfigPath -Raw | ConvertFrom-Json

# Update the fallback server workspace path
$config.mcpServers."mcp-unity-fallback".workspace = $Workspace

# Convert back to JSON and save
$config | ConvertTo-Json -Depth 10 | Set-Content -Path $ConfigPath

Write-Host "Updated mcp-unity-fallback workspace path successfully"

# Output the current configuration
Write-Host "Current MCP Configuration:"
Get-Content -Path $ConfigPath

# update-mcp-config.ps1
# Purpose: Update the fallback server workspace path in the MCP config file
# Usage: .\update-mcp-config.ps1 [-ConfigPath <path>]

param(
    [string]$ConfigPath = "C:\Users\w1n51\.cursor\mcp.json"
)

# Check if the file exists
if (Test-Path $ConfigPath) {
    Write-Host "Found MCP configuration at $ConfigPath"

    # Read the current configuration
    $config = Get-Content -Path $ConfigPath -Raw | ConvertFrom-Json

    # Update the fallback server workspace path
    $config.mcpServers."mcp-unity-fallback".workspace = "C:/Users/w1n51/OneDrive/Desktop/Gamedev/TBD Space Game/TBD SpaceGame/TBD SpaceGame"

    # Convert back to JSON and save
    $config | ConvertTo-Json -Depth 10 | Set-Content -Path $ConfigPath

    Write-Host "Updated mcp-unity-fallback workspace path successfully"
} else {
    Write-Host "MCP configuration file not found at $ConfigPath"
}

# Output the current configuration
Write-Host "Current MCP Configuration:"
Get-Content -Path $ConfigPath

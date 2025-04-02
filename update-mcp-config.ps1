# PowerShell script to update MCP configuration
$configPath = "C:\Users\w1n51\.cursor\mcp.json"

# Check if the file exists
if (Test-Path $configPath) {
    Write-Host "Found MCP configuration at $configPath"
    
    # Read the current configuration
    $config = Get-Content -Path $configPath -Raw | ConvertFrom-Json
    
    # Update the fallback server workspace path
    $config.mcpServers."mcp-unity-fallback".workspace = "C:/Users/w1n51/OneDrive/Desktop/Gamedev/TBD Space Game/TBD SpaceGame/TBD SpaceGame"
    
    # Convert back to JSON and save
    $config | ConvertTo-Json -Depth 10 | Set-Content -Path $configPath
    
    Write-Host "Updated mcp-unity-fallback workspace path successfully"
} else {
    Write-Host "MCP configuration file not found at $configPath"
}

# Output the current configuration
Write-Host "Current MCP Configuration:"
Get-Content -Path $configPath 
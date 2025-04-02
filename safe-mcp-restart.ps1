# Safe MCP Restart Script
# This script starts the MCP server without killing other processes

Write-Host "Starting MCP server safely..."

# Define the MCP server path
$serverPath = "TBD SpaceGame/TBD SpaceGame/Library/PackageCache/com.gamelovers.mcp-unity@3acfb232f564/Server"

# Set environment variable for the port
$env:UNITY_PORT = "8001"

# Check if we're in the correct directory
$currentDir = Get-Location
$serverFullPath = Join-Path $currentDir $serverPath

if (Test-Path $serverFullPath) {
    # We're in the project root, run the command with path
    Write-Host "Running MCP server from project root..."
    Start-Process -FilePath "node" -ArgumentList "$serverPath/build/index.js" -NoNewWindow
} else {
    # Try to run directly - might be already in the server directory
    Write-Host "Running MCP server directly..."
    Start-Process -FilePath "node" -ArgumentList "build/index.js" -NoNewWindow
}

Write-Host "MCP server started on port 8001. You can now reconnect from Unity."
Write-Host ""
Write-Host "IMPORTANT: Use the V2 menu items in Unity to avoid conflicts:"
Write-Host "- MCP/Ship/V2/Create Test Ship"
Write-Host "- MCP/Ship/V2/Upgrade Thrust"
Write-Host "- MCP/Ship/V2/Upgrade Speed"
Write-Host "- MCP/Ship/V2/Upgrade Rotation"
Write-Host "- MCP/Ship/V2/Upgrade Braking"
Write-Host "- MCP/Ship/V2/Log Current Upgrades"
Write-Host ""
Write-Host "These menu items use the new ShipUpgradeMenuHelperV2 class to avoid the duplicate menu errors." 
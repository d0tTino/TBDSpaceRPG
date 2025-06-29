# PowerShell script to start the MCP Proxy server
Write-Host "Starting MCP Proxy Server..."

# Check if Node.js is available
try {
    $nodeVersion = node --version
    Write-Host "Using Node.js $nodeVersion"
}
catch {
    Write-Error "Node.js not found. Please install Node.js 18+ to run the MCP Proxy server."
    exit 1
}

# Change to the mcpProxy server directory
if (-not (Test-Path "C:\Users\w1n51\mcpProxy")) {
    Write-Error "MCP Proxy directory not found: C:\Users\w1n51\mcpProxy"
    exit 1
}
Set-Location -Path "C:\Users\w1n51\mcpProxy"

# Start the mcpProxy server as a background job
Start-Process node -ArgumentList "dist/src/index.js" -NoNewWindow -PassThru

Write-Host "MCP Proxy Server started on port 8004"
Write-Host "Server will run in the background. To stop, close the Node.js process." 

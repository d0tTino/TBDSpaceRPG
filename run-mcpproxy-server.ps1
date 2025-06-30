# PowerShell script to start the MCP Proxy server
Write-Host "Starting MCP Proxy Server..."

function Test-ValidPort {
    param([int]$Port)
    return $Port -ge 1 -and $Port -le 65535
}

if (-not (Test-ValidPort -Port 8004)) {
    Write-Error "Invalid port: 8004. Must be between 1 and 65535."
    exit 1
}

# Check if Node.js is available
try {
    $nodeVersion = node --version
    Write-Host "Using Node.js $nodeVersion"
} catch {
    Write-Error "Node.js not found. Please install Node.js 18+ to run the MCP Proxy server."
    exit 1
}

# Change to the mcpProxy server directory
try {
    Set-Location -Path "C:\Users\w1n51\mcpProxy"
} catch {
    Write-Error "Unable to change directory"
    exit 1
}


# Start the mcpProxy server as a background job
Start-Process node -ArgumentList "dist/src/index.js" -NoNewWindow -PassThru

Write-Host "MCP Proxy Server started on port 8004"
Write-Host "Server will run in the background. To stop, close the Node.js process."


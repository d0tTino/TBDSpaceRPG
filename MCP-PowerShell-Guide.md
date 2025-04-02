# PowerShell Best Practices for MCP Integration

## Common PowerShell Pitfalls

Working with the MCP server in PowerShell requires careful attention to syntax. This guide addresses the most common issues encountered in our development process.

## Command Chaining

### ✅ DO: Use semicolons for command chaining
```powershell
cd "TBD SpaceGame\Server"; $env:UNITY_PORT="8001"; node build/index.js
```

### ❌ DON'T: Use && operators (common in bash/cmd)
```powershell
# This causes syntax errors in PowerShell
cd "TBD SpaceGame\Server" && $env:UNITY_PORT="8001" && node build/index.js
```

## Environment Variables

### ✅ DO: Use $env: prefix with quotes
```powershell
$env:UNITY_PORT="8001"
```

### ❌ DON'T: Use export or SET commands
```powershell
# This doesn't work in PowerShell
export UNITY_PORT=8001
SET UNITY_PORT=8001
```

## Path Navigation

### ✅ DO: Use double quotes for paths with spaces
```powershell
cd "TBD SpaceGame\TBD SpaceGame\Library\PackageCache"
```

### ❌ DON'T: Use single quotes or no quotes for paths with spaces
```powershell
# These will fail with paths containing spaces
cd 'TBD SpaceGame\Server'
cd TBD SpaceGame\Server
```

## Running Commands in Background

### ✅ DO: Use Start-Process for background tasks
```powershell
Start-Process -FilePath "powershell.exe" -ArgumentList "-File .\run-mcp-server.ps1" -WindowStyle Hidden
```

### ❌ DON'T: Use & operator without proper handling
```powershell
# This may not work as expected
.\run-mcp-server.ps1 &
```

## Working Directory Context

PowerShell maintains working directory context between commands when using semicolons:

```powershell
# This works as expected - each command runs in the resulting directory of the previous command
cd "TBD SpaceGame"; cd "Server"; node build/index.js
```

## Handling Errors

Add error handling to your scripts:

```powershell
try {
    cd "TBD SpaceGame\TBD SpaceGame\Library\PackageCache\com.gamelovers.mcp-unity@3acfb232f564\Server"
    $env:UNITY_PORT="8001"
    node build/index.js
} catch {
    Write-Error "Failed to start MCP server: $_"
    exit 1
}
```

### MCP-Specific Error Handling Example

Here's a practical example for handling MCP server startup:

```powershell
try {
    # Verify Node.js installation first
    $nodeVersion = node --version
    Write-Host "Using Node.js version: $nodeVersion"
    
    # Change to server directory
    Push-Location "TBD SpaceGame\TBD SpaceGame\Library\PackageCache\com.gamelovers.mcp-unity@3acfb232f564\Server"
    
    # Set environment variables
    $env:UNITY_PORT = "8001"
    $env:NODE_ENV = "production"
    
    # Start the server and capture any errors
    $process = Start-Process -FilePath "node" -ArgumentList "build/index.js" -NoNewWindow -PassThru
    
    # Check if process started successfully
    if ($process.ExitCode -ne $null) {
        throw "Node.js process exited immediately with code: $($process.ExitCode)"
    }
    
    Write-Host "MCP server started successfully with process ID: $($process.Id)"
} 
catch {
    Write-Error "Failed to start MCP server: $_"
    
    # Make sure we return to the original directory even on error
    if ($pwd.Path -like "*com.gamelovers.mcp-unity*") {
        Pop-Location
    }
    exit 1
}
finally {
    # Always return to original directory
    if ($pwd.Path -like "*com.gamelovers.mcp-unity*") {
        Pop-Location
    }
}
```

## Checking Port Availability

Before starting the MCP server, check if the required port is already in use:

```powershell
function Test-PortAvailable {
    param (
        [int]$Port = 8001
    )
    
    try {
        # Check if port is in use
        $inUse = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
        
        if ($inUse) {
            Write-Warning "Port $Port is already in use!"
            Write-Warning "Process using port: $(Get-Process -Id $inUse[0].OwningProcess -ErrorAction SilentlyContinue | Select-Object -ExpandProperty ProcessName)"
            return $false
        }
        
        return $true
    }
    catch {
        # If we can't check, assume it's available
        Write-Warning "Unable to check port availability: $_"
        return $true
    }
}

# Usage example
if (Test-PortAvailable -Port 8001) {
    Write-Host "Port is available, starting MCP server..."
    # Start server code here
}
else {
    Write-Host "Please close the application using port 8001 or use a different port."
    exit 1
}
```

## Script Template

Here's a template for creating reliable MCP-related PowerShell scripts:

```powershell
# Script Name: example-mcp-script.ps1
# Purpose: Example MCP integration script
# Usage: .\example-mcp-script.ps1 -Parameter "Value"

param (
    [Parameter(Mandatory=$true)]
    [string]$Parameter
)

# Define paths relative to project root
$projectRoot = "C:\Users\w1n51\OneDrive\Desktop\Gamedev\TBD Space Game"
$serverPath = Join-Path $projectRoot "TBD SpaceGame\TBD SpaceGame\Library\PackageCache\com.gamelovers.mcp-unity@3acfb232f564\Server"

# Validate paths
if (-not (Test-Path $serverPath)) {
    Write-Error "Server path not found: $serverPath"
    exit 1
}

# Change to server directory
try {
    Push-Location $serverPath
    
    # Set environment variables
    $env:UNITY_PORT = "8001"
    
    # Execute command
    Write-Output "Executing command with parameter: $Parameter"
    # Your command here
    
    # Return to original directory
    Pop-Location
} catch {
    Write-Error "Script execution failed: $_"
    # Ensure we return to original directory even on error
    if ($pwd.Path -eq $serverPath) {
        Pop-Location
    }
    exit 1
}

Write-Output "Script completed successfully"
```

## Troubleshooting PowerShell Issues

### Module Not Found
If you see errors like:
```
Error: Cannot find module 'C:\path\to\build\index.js'
```

Check:
1. Are you in the correct directory?
2. Does the file actually exist?
3. Are you using correct path separators (backslashes in Windows)?

### Command Not Recognized
If PowerShell doesn't recognize node, npm, or other commands:

1. Ensure the tool is installed
2. Check if the tool is in your PATH
3. Use full paths if necessary: `C:\Program Files\nodejs\node.exe`

### Permission Issues
If you encounter permission errors:

1. Run PowerShell as Administrator for system directories
2. Check file permissions on scripts and directories
3. Ensure execution policy allows running scripts: `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser` 
# PowerShell script to stop all MCP servers launched via run-all-mcp-servers.ps1

param(
    [string]$PidDirectory
)

Write-Host "Stopping MCP servers..." -ForegroundColor Cyan

# Determine script directory
$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (-not $scriptDir) {
    $scriptDir = $PWD.Path
}

if (-not $PidDirectory) {
    $PidDirectory = Join-Path $scriptDir 'tmp/pids'
}

if (-not (Test-Path $PidDirectory)) {
    Write-Host "PID directory not found: $PidDirectory" -ForegroundColor Yellow
    return
}

$pidFiles = Get-ChildItem -Path $PidDirectory -Filter '*.pid' -ErrorAction SilentlyContinue
if (-not $pidFiles) {
    Write-Host "No PID files found." -ForegroundColor Yellow
    return
}

foreach ($file in $pidFiles) {
    try {
        $pid = Get-Content -Path $file.FullName | Select-Object -First 1
        if ($pid) {
            $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Stopping $($file.BaseName) (PID: $pid)..." -ForegroundColor Yellow
                Stop-Process -Id $pid -ErrorAction SilentlyContinue
                if (Get-Process -Id $pid -ErrorAction SilentlyContinue) {
                    Start-Sleep -Seconds 3
                    if (Get-Process -Id $pid -ErrorAction SilentlyContinue) {
                        Write-Host "Forcing termination of PID $pid" -ForegroundColor Red
                        Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                    }
                }
                Write-Host "Stopped $($file.BaseName)" -ForegroundColor Green
            } else {
                Write-Host "Process with PID $pid not running" -ForegroundColor Yellow
            }
        }
    } catch {
        Write-Warning "Failed to stop process in $($file.Name): $_"
    } finally {
        Remove-Item $file.FullName -ErrorAction SilentlyContinue
    }
}

Write-Host "All servers stopped." -ForegroundColor Cyan


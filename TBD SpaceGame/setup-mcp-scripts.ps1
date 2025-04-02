# MCP Scripts Setup Helper
# Helps ensure scripts are in the right location for proper menu registration

$ErrorActionPreference = "Stop"
$unityProjectPath = Join-Path (Get-Location) "TBD SpaceGame\TBD SpaceGame"

# Check if Unity project exists
if (-not (Test-Path $unityProjectPath)) {
    Write-Host "ERROR: Unity project not found at: $unityProjectPath" -ForegroundColor Red
    exit 1
}

# Create necessary directories if they don't exist
$pluginsDir = Join-Path $unityProjectPath "Assets\Plugins"
$editorDir = Join-Path $unityProjectPath "Assets\Editor"

if (-not (Test-Path $pluginsDir)) {
    Write-Host "Creating Plugins directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $pluginsDir -Force | Out-Null
}

if (-not (Test-Path $editorDir)) {
    Write-Host "Creating Editor directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $editorDir -Force | Out-Null
}

# Function to ensure a script exists with proper content
function Ensure-ScriptExists {
    param(
        [string]$scriptPath,
        [string]$scriptContent,
        [string]$scriptName,
        [string]$scriptDescription
    )
    
    $fullPath = Join-Path $unityProjectPath $scriptPath
    $directoryPath = Split-Path $fullPath -Parent
    
    # Ensure directory exists
    if (-not (Test-Path $directoryPath)) {
        Write-Host "Creating directory: $directoryPath" -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $directoryPath -Force | Out-Null
    }
    
    $needsUpdate = $true
    
    # Check if file exists
    if (Test-Path $fullPath) {
        $existingContent = Get-Content $fullPath -Raw
        if ($existingContent -eq $scriptContent) {
            Write-Host "✓ $scriptName already exists and is up to date" -ForegroundColor Green
            $needsUpdate = $false
        }
        else {
            Write-Host "Updating $scriptName..." -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Creating $scriptName..." -ForegroundColor Yellow
    }
    
    # Update file if needed
    if ($needsUpdate) {
        Set-Content -Path $fullPath -Value $scriptContent
        Write-Host "✓ $scriptName has been created/updated" -ForegroundColor Green
    }
    
    return $fullPath
}

# Define script contents
$basicMenuItemScript = @'
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

// This class is deliberately placed in the Plugins folder (not Editor)
// to ensure it's compiled early in Unity's compilation process
public static class BasicMenuItem
{
    [MenuItem("Basic/Test")]
    public static void Test()
    {
        Debug.Log("Basic Test executed successfully!");
        EditorUtility.DisplayDialog("Basic Test", "Menu item executed successfully!", "OK");
    }
    
    [MenuItem("Basic/Create GameObject")]
    public static void CreateGameObject()
    {
        GameObject obj = new GameObject("Basic Test Object");
        Debug.Log("Created basic test object!");
        Selection.activeGameObject = obj;
        EditorUtility.DisplayDialog("Basic Test", "GameObject created successfully!", "OK");
    }
    
    [MenuItem("Basic/Log Unity Version")]
    public static void LogUnityVersion()
    {
        Debug.Log($"Unity Version: {Application.unityVersion}");
        Debug.Log($"Product Name: {Application.productName}");
        Debug.Log($"Platform: {Application.platform}");
        EditorUtility.DisplayDialog("Unity Info", $"Unity Version: {Application.unityVersion}\nPlatform: {Application.platform}", "OK");
    }
}
#endif
'@

$mcpBridgeScript = @'
using UnityEngine;
using UnityEditor;

// This class is deliberately placed in the Editor folder
// and provides direct menu items for MCP to call
public static class McpBridge
{
    [MenuItem("McpBridge/Test")]
    public static void Test()
    {
        Debug.Log("McpBridge Test executed successfully!");
        EditorUtility.DisplayDialog("McpBridge", "Test executed successfully!", "OK");
    }
    
    [MenuItem("McpBridge/CreateShip")]
    public static void CreateShip()
    {
        Debug.Log("Creating ship via McpBridge...");
        GameObject ship = new GameObject("McpBridge Ship");
        
        // Select the ship in the hierarchy
        Selection.activeGameObject = ship;
        
        EditorUtility.DisplayDialog("McpBridge", "Ship created successfully!", "OK");
    }
    
    [MenuItem("McpBridge/LogStats")]
    public static void LogStats()
    {
        Debug.Log("McpBridge LogStats executed.");
        Debug.Log($"Active Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Unity Version: {Application.unityVersion}");
        Debug.Log($"Company Name: {Application.companyName}");
        Debug.Log($"Product Name: {Application.productName}");
        
        EditorUtility.DisplayDialog("McpBridge Stats", 
            $"Active Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}\n" +
            $"Unity Version: {Application.unityVersion}", 
            "OK");
    }
}
'@

$defaultAssetScript = @'
using UnityEngine;
using UnityEditor;

public static class DefaultAsset
{
    [MenuItem("MCP/Default/Test")]
    public static void DefaultTest()
    {
        Debug.Log("Default test menu item executed!");
        EditorUtility.DisplayDialog("MCP Test", "Menu item executed successfully!", "OK");
    }

    [MenuItem("Help/About MCP Test")]
    public static void AboutTest()
    {
        Debug.Log("About MCP test menu item executed!");
        EditorUtility.DisplayDialog("About MCP Test", "MCP Test is working correctly!", "OK");
    }
}
'@

# Ensure scripts exist
Write-Host "`n=== Setting up MCP Scripts ===" -ForegroundColor Cyan

$basicMenuItemPath = Ensure-ScriptExists -scriptPath "Assets\Plugins\BasicMenuItem.cs" -scriptContent $basicMenuItemScript -scriptName "BasicMenuItem.cs" -scriptDescription "Basic menu items for MCP testing"
$mcpBridgePath = Ensure-ScriptExists -scriptPath "Assets\Editor\McpBridge.cs" -scriptContent $mcpBridgeScript -scriptName "McpBridge.cs" -scriptDescription "MCP bridge for menu execution"
$defaultAssetPath = Ensure-ScriptExists -scriptPath "Assets\Editor\DefaultAsset.cs" -scriptContent $defaultAssetScript -scriptName "DefaultAsset.cs" -scriptDescription "Default menu items for MCP testing"

# Summary
Write-Host "`n=== MCP Scripts Setup Complete ===" -ForegroundColor Cyan
Write-Host "The following scripts have been set up:" -ForegroundColor Cyan
Write-Host "1. $basicMenuItemPath" -ForegroundColor Green
Write-Host "2. $mcpBridgePath" -ForegroundColor Green
Write-Host "3. $defaultAssetPath" -ForegroundColor Green

Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "1. Restart Unity to ensure all scripts are compiled" -ForegroundColor Yellow
Write-Host "2. Start the MCP server: .\mcp-server-manager.ps1 -Action start" -ForegroundColor Yellow
Write-Host "3. Test a menu command: .\test-mcp-menu.ps1 -MenuPath `"Basic/Test`"" -ForegroundColor Yellow

Write-Host "`nTIP: If menu items still aren't recognized, try creating a new Unity project" -ForegroundColor Yellow
Write-Host "     and importing the MCP package from the Unity Asset Store." -ForegroundColor Yellow 
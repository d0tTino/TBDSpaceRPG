# Unity-MCP Integration Manual

## Introduction

This guide explains how to integrate the Machine Control Protocol (MCP) with Unity for the TBD Space Game project. MCP enables AI-assisted development by allowing external tools to control Unity's editor functionality.

## Setup Requirements

- Unity 2022.3 or newer
- Node.js 16+ installed
- MCP Unity package imported
- PowerShell 7.0+ recommended

## Creating Menu Items

Menu items are the primary way MCP interacts with Unity. Here's how to create them correctly:

### Basic Menu Item

Place this script in `Assets/Plugins` (not in Editor folder):

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Important: Don't use namespaces for menu scripts

public class BasicMenuItem
{
    [MenuItem("Basic/Test")]
    public static void TestMenuItem()
    {
        Debug.Log("Basic test menu item executed");
    }
}
#endif
```

### Ship Customization Menu

Here's an example for ship customization features:

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ShipUpgradeMenuHelper
{
    [MenuItem("MCP/Ship/V2/Create Test Ship")]
    public static void CreateTestShip()
    {
        GameObject ship = new GameObject("Test Ship");
        ship.AddComponent<CustomizableSpaceship>();
        Debug.Log("Test ship created with CustomizableSpaceship component");
    }

    [MenuItem("MCP/Ship/V2/Upgrade Thrust")]
    public static void UpgradeThrust()
    {
        CustomizableSpaceship[] ships = Object.FindObjectsOfType<CustomizableSpaceship>();
        if (ships.Length == 0)
        {
            Debug.LogError("No ships found to upgrade");
            return;
        }

        ships[0].UpgradeThrust(1);
        Debug.Log($"Upgraded thrust on ship: {ships[0].name}");
    }

    [MenuItem("MCP/Ship/V2/Log Current Upgrades")]
    public static void LogCurrentUpgrades()
    {
        CustomizableSpaceship[] ships = Object.FindObjectsOfType<CustomizableSpaceship>();
        if (ships.Length == 0)
        {
            Debug.LogError("No ships found to check upgrades");
            return;
        }

        ships[0].LogAllUpgrades();
    }
}
#endif
```

### Important Rules for Menu Items

1. **Place scripts in `Assets/Plugins` for proper compilation priority**
   - *Why:* Scripts in the Plugins folder compile before other scripts, ensuring menu items are registered before Unity needs them. Scripts in the Editor folder may compile too late.

2. **Wrap all editor code in `#if UNITY_EDITOR` guards**
   - *Why:* These guards prevent editor-only code from being included in runtime builds, avoiding errors when the UnityEditor namespace isn't available.

3. **Keep scripts outside of namespaces**
   - *Why:* Unity's menu system doesn't always correctly find menu items inside namespaces, causing "menu item not found" errors with MCP.

4. **Use simple, consistent menu paths**
   - *Why:* Complex paths with special characters or excessive nesting can cause recognition problems in the MCP system.

5. **Include informative debug logging**
   - *Why:* Proper logging helps diagnose whether a menu item executed correctly or where it failed, which is essential when debugging MCP integrations.

6. **Handle error cases (e.g., when objects aren't found)**
   - *Why:* Graceful error handling prevents Unity editor crashes that could disconnect the MCP server.

7. **Allow sufficient time for Unity to compile after adding scripts**
   - *Why:* Unity needs time to compile and register new menu items (typically 30+ seconds), especially after initial script creation.

## Executing Menu Items

### From PowerShell

Use the test-menu.ps1 script:

```powershell
.\test-menu.ps1 -MenuPath "MCP/Ship/V2/Create Test Ship"
```

### From C# Code

```csharp
EditorApplication.ExecuteMenuItem("MCP/Ship/V2/Create Test Ship");
```

## Common Workflows

### Ship Customization Workflow

1. Create a test ship:
   ```powershell
   .\test-menu.ps1 -MenuPath "MCP/Ship/V2/Create Test Ship"
   ```

2. Upgrade the ship's thrust:
   ```powershell
   .\test-menu.ps1 -MenuPath "MCP/Ship/V2/Upgrade Thrust"
   ```

3. Check current upgrades:
   ```powershell
   .\test-menu.ps1 -MenuPath "MCP/Ship/V2/Log Current Upgrades"
   ```

## CI/CD Integration

The MCP integration can be incorporated into your CI/CD pipeline to automate testing and validation of Unity menu items.

### Pipeline Integration Steps

1. **Setup MCP Server in CI Environment**:
   ```yaml
   # Example GitHub Actions workflow step
   - name: Start MCP Server
     run: |
       ./run-mcp-server.ps1 -Port 8001
       Start-Sleep -Seconds 5  # Allow server to initialize
   ```

2. **Execute Test Commands**:
   ```yaml
   - name: Test Menu Items
     run: |
       ./test-menu.ps1 -MenuPath "GameObject/Create Empty" -Port 8001
       if ($LASTEXITCODE -ne 0) { exit 1 }
       
       ./test-menu.ps1 -MenuPath "MCP/Ship/V2/Create Test Ship" -Port 8001
       if ($LASTEXITCODE -ne 0) { exit 1 }
   ```

3. **Validate Unity Logs**:
   ```yaml
   - name: Check Unity Logs
     run: |
       if (Select-String -Path "mcp-server.log" -Pattern "Error") {
         echo "Errors found in MCP server logs"
         exit 1
       }
   ```

### Batch Testing Script

Create a batch testing script for CI environments:

```powershell
# batch-test-menus.ps1
param (
    [string[]]$MenuPaths = @(
        "GameObject/Create Empty",
        "MCP/Ship/V2/Create Test Ship",
        "MCP/Ship/V2/Upgrade Thrust"
    ),
    [int]$Port = 8001
)

$failedTests = 0

foreach ($path in $MenuPaths) {
    Write-Host "Testing menu item: $path"
    ./test-menu.ps1 -MenuPath $path -Port $Port
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Test failed for menu item: $path"
        $failedTests++
    }
}

if ($failedTests -gt 0) {
    Write-Error "$failedTests tests failed"
    exit 1
}
else {
    Write-Host "All menu item tests passed!" -ForegroundColor Green
    exit 0
}
```

### Monitoring MCP Server Health in CI

Add this script to verify server health during CI runs:

```powershell
# monitor-mcp-server.ps1
param (
    [int]$Port = 8001,
    [int]$DurationMinutes = 10
)

$endTime = (Get-Date).AddMinutes($DurationMinutes)
$failures = 0

while ((Get-Date) -lt $endTime) {
    $connection = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    
    if (-not $connection) {
        $failures++
        Write-Warning "MCP server not detected on port $Port. Failure #$failures"
        
        if ($failures -ge 3) {
            Write-Error "MCP server has failed multiple times. Attempting restart..."
            ./run-mcp-server.ps1 -Port $Port
            $failures = 0
        }
    }
    else {
        Write-Host "MCP server running on port $Port" -ForegroundColor Green
    }
    
    Start-Sleep -Seconds 30
}
```

## Troubleshooting

### Menu Item Not Found

If you get "ExecuteMenuItem failed because there is no menu named 'X'":

1. Verify the exact menu path (case-sensitive)
2. Ensure script is located in Assets/Plugins (not Editor)
3. Wait for Unity to compile scripts (can take 30+ seconds)
4. Check Unity console for compilation errors
5. Try standard Unity menu paths as a fallback

### Script Compilation Issues

1. Ensure script has no syntax errors
2. Remove any namespace declarations
3. Verify `#if UNITY_EDITOR` guards are in place
4. Check that required components exist

### Connection Issues

1. Verify MCP server is running (`.\run-mcp-server.ps1`)
2. Check Unity is in Edit mode (not Play mode)
3. Confirm port 8001 is not blocked or in use
4. Try restarting Unity if connected previously

## Advanced Integration

### Custom Property Drawers

For advanced UI customization in the Inspector:

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShipUpgrade))]
public class ShipUpgradeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // Draw custom UI for ShipUpgrade property
        SerializedProperty levelProp = property.FindPropertyRelative("level");
        SerializedProperty nameProp = property.FindPropertyRelative("upgradeName");
        
        Rect nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect levelRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
        
        EditorGUI.PropertyField(nameRect, nameProp);
        EditorGUI.PropertyField(levelRect, levelProp);
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 2;
    }
}
#endif
```

### Editor Windows

Create custom editor windows accessible via MCP:

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ShipCustomizationWindow : EditorWindow
{
    [MenuItem("MCP/Ship/Open Customization Window")]
    public static void ShowWindow()
    {
        ShipCustomizationWindow window = EditorWindow.GetWindow<ShipCustomizationWindow>("Ship Customization");
        window.Show();
    }
    
    private void OnGUI()
    {
        // Draw window content
        GUILayout.Label("Ship Customization", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Test Ship"))
        {
            ShipUpgradeMenuHelper.CreateTestShip();
        }
        
        if (GUILayout.Button("Upgrade Thrust"))
        {
            ShipUpgradeMenuHelper.UpgradeThrust();
        }
        
        if (GUILayout.Button("Log Upgrades"))
        {
            ShipUpgradeMenuHelper.LogCurrentUpgrades();
        }
    }
}
#endif
```

## Best Practices

1. **Keep menu items specific**: Each menu item should perform one specific task
2. **Add validation**: Use [MenuItem validation](https://docs.unity3d.com/ScriptReference/MenuItem-ctor.html) to enable/disable menu items
3. **Log appropriately**: Include informative logs for tracking execution
4. **Handle errors gracefully**: Check for null references and provide useful error messages
5. **Test in Edit mode**: Ensure all functionality works in Edit mode, not just Play mode
6. **Maintain undo support**: Use Undo.RecordObject for changes to persist properly

## Server Utility Scripts

Use `safe-mcp-restart.ps1` to safely restart the MCP server without killing unrelated Node processes:
```powershell
./safe-mcp-restart.ps1
```
If Unity updates its packages, run `restart-mcp-after-unity-update.ps1`:
```powershell
./restart-mcp-after-unity-update.ps1
```


## Godot Workflow

### Exporting glTF Assets
Use the glTFast plugin or Blender\x27s exporter to convert Unity prefabs or Blender models into `.gltf` files. Place the exports in `Assets_glTF/`.

### Launching Servers for Godot
Start all MCP services with Godot-specific settings:
```powershell
./run-all-mcp-servers.ps1 -Engine godot
```

The Godot project under `Godot/` will automatically import these assets and generate reusable scenes.

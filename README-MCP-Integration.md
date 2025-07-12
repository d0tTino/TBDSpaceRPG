# MCP Integration for TBD Space Game

This document provides comprehensive information about the Model Context Protocol (MCP) integration for the TBD Space Game project, including server setup, configuration, and tools developed to enhance AI-assisted game development.

<!-- Project Comparison -->
Unlike the classic **Ringworld RPG**, which focuses primarily on tabletop-style exploration of a single megastructure, TBD Space Game centers on managing a starship crew over multiple generations. The two projects share a sci-fi setting, but this project integrates MCP tooling and Unity-based gameplay while **Ringworld RPG** relies on a more traditional ruleset with minimal automation. These notes clarify the unique scope and workflow of this project for anyone familiar with **Ringworld RPG**.

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [MCP Servers Setup](#mcp-servers-setup)
4. [Configuration](#configuration)
5. [Tools](#tools)
6. [Ship Customization System](#ship-customization-system)
7. [Troubleshooting](#troubleshooting)
8. [Future Enhancements](#future-enhancements)

## Overview

The TBD Space Game uses MCP (Model Context Protocol) to enable AI-assisted development workflows. The integration consists of multiple servers:

- **MCP Unity Server**: Primary server for Unity integration running on port 8001
- **MCP Unity Fallback**: Backup server running on port 8002
- **Server-Git**: Version control operations via MCP running on port 8080
- **Server-Postgres**: Database operations via MCP running on port 8003
- **MCPProxy**: Routes requests to appropriate MCP servers running on port 8004
- **KSA Adapter**: Placeholder server for the upcoming KSA engine on port 8005

These servers enable AI tools to interact with the Unity project, perform version control operations, manage database records, and more.

## Prerequisites

- Unity 2022.3 or later
- Node.js v18 or later
- npm v9 or later
- PowerShell 7.0 or later (required to run tests)
- Python 3.8 or later (for server-git)
- PostgreSQL (for server-postgres)

## MCP Servers Setup

### Primary MCP Unity Server

The primary MCP Unity server enables AI tools to interact with the Unity editor.

```powershell
# Start the primary MCP Unity server
./run-mcp-server.ps1
```

### Server-Git

Handles Git operations through MCP.

```powershell
# Start the Git server
./run-git-server.ps1
```

### Server-Postgres

Manages database operations through MCP.

```powershell
# Start the PostgreSQL server
./run-postgres-server.ps1
```

### MCPProxy

Routes requests to appropriate MCP servers.

```powershell
# Start the MCP Proxy server (defaults to a path relative to this repo)
./run-mcpproxy-server.ps1

# Provide a custom proxy path if needed
./run-mcpproxy-server.ps1 -ProxyPath "C:\path\to\mcpProxy"
```

### KSA Adapter

Placeholder adapter for the upcoming KSA engine. It simply echoes MCP requests.

```powershell
# Start the KSA adapter
./run-ksa-server.ps1
```

When the real engine API is available, replace the placeholder server with the
actual implementation and update configuration values accordingly.

### All Servers

For convenience, a script is provided to start all MCP servers simultaneously:

```powershell
# Start all MCP servers
./run-all-mcp-servers.ps1
```

## Configuration

### MCP Unity Server Configuration

The MCP Unity server is configured in the `~/.cursor/mcp.json` file:

```json
{
  "mcpServers": {
    "mcp-unity": {
      "serverType": "node",
      "workspace": "C:/Users/w1n51/OneDrive/Desktop/Gamedev/TBD Space Game/TBD SpaceGame/TBD SpaceGame",
      "transport": "ws",
      "port": 8001,
      "diagnostics": {
        "verboseLogging": true
      }
    },
    "mcp-unity-fallback": {
      "serverType": "node",
      "workspace": "C:/Users/w1n51/OneDrive/Desktop/Gamedev/TBD Space Game/TBD SpaceGame/TBD SpaceGame",
      "transport": "ws",
      "port": 8002,
      "diagnostics": {
        "verboseLogging": true
      }
    }
  }
}
```

You can update this configuration using the provided script. Provide the
workspace path for the Unity fallback server with the `-Workspace` parameter:

```powershell
./update-mcp-config.ps1 -Workspace "C:/path/to/TBD SpaceGame"
```

## Tools

### WebSocket Clients

Two WebSocket clients are available for interacting with MCP servers:

#### Simple WebSocket Client

Basic client for executing menu items in Unity:

```powershell
./mcpWebSocketClient-simple.ps1 -menuPath "MCP/Ship/Create Test Ship"
```

#### Advanced WebSocket Client

Enhanced client supporting multiple command types:

```powershell
# Execute menu item
./mcpWebSocketClient-advanced.ps1 -commandType "menu_item" -menuPath "MCP/Ship/Upgrade Thrust"

# Send a message to Unity console
./mcpWebSocketClient-advanced.ps1 -commandType "message" -message "Hello from MCP!" -logType "Log"

# Update a component property
./mcpWebSocketClient-advanced.ps1 -commandType "update_component" -gameObjectName "TestShip" -componentType "SpaceshipMovement" -propertyName "thrustForce" -propertyValue "30"

# Select a GameObject
./mcpWebSocketClient-advanced.ps1 -commandType "select_gameobject" -gameObjectName "TestShip"
```

### Persistent Server Script

A persistent server script (`mcp-persistent-server.js`) has been developed to auto-restart the MCP server if it crashes or Unity recompiles:

```powershell
# Start the persistent MCP server
./run-mcp-server.ps1
```

### Unity Editor Integration

The `McpServerAutoRestarter.cs` script integrates with the Unity Editor to restart the MCP server after compilation:

```csharp
[InitializeOnLoad]
public class McpServerAutoRestarter
{
    static McpServerAutoRestarter()
    {
        EditorApplication.delayCall += () =>
        {
            Debug.Log("MCP Server Auto Restarter initialized");
        };

        CompilationPipeline.compilationFinished += OnCompilationFinished;
    }

    private static void OnCompilationFinished(object obj)
    {
        // Wait briefly for Unity to settle after compilation
        EditorApplication.delayCall += () =>
        {
            RestartMcpServer();
        };
    }

    private static void RestartMcpServer()
    {
        // Implementation details in the script
    }
}
```

## Running Tests

All automated tests use PowerShell scripts. Ensure that the `pwsh` command is available before running tests.

```pwsh
pwsh -File tests/test-servers.ps1
```

For installation instructions, see the [PowerShell installation guide](https://learn.microsoft.com/powershell/scripting/install/installing-powershell).

## Ship Customization System

The Ship Customization System (implemented according to Section 4.16 of the technical documentation) consists of:

1. **SpaceshipMovement.cs**: Core component handling ship movement with customizable parameters
2. **ShipCustomizationManager.cs**: Manages upgrade categories and levels
3. **ShipUpgradeMenuHelper.cs**: Editor script that provides menu items for MCP integration

### Testing the Ship Customization System

The following MCP commands can be used to test the Ship Customization System:

```powershell
# Create a test ship
./mcpWebSocketClient-simple.ps1 -menuPath "MCP/Ship/Create Test Ship"

# Apply upgrades
./mcpWebSocketClient-simple.ps1 -menuPath "MCP/Ship/Upgrade Thrust"
./mcpWebSocketClient-simple.ps1 -menuPath "MCP/Ship/Upgrade Speed"
./mcpWebSocketClient-simple.ps1 -menuPath "MCP/Ship/Upgrade Rotation"
./mcpWebSocketClient-simple.ps1 -menuPath "MCP/Ship/Upgrade Braking"

# Log current upgrades
./mcpWebSocketClient-simple.ps1 -menuPath "MCP/Ship/Log Current Upgrades"

# Reset all upgrades
./mcpWebSocketClient-simple.ps1 -menuPath "MCP/Ship/Reset All Upgrades"
```

## Troubleshooting

### Common Issues

1. **MCP Server Not Connecting**:
   - Verify Unity is running
   - Check ports are not in use by other applications
   - Ensure correct workspace path in configuration

2. **WebSocket Client Errors**:
   - Ensure the MCP server is running
   - Check for correct port (default: 8001)
   - Verify command syntax

3. **Auto-restart Not Working**:
   - Ensure `mcp-persistent-server.js` is correctly referenced in the PowerShell script
   - Check Node.js is properly installed

### Logs

Log files are available in the following locations:

- MCP Unity Server: `TBD SpaceGame/TBD SpaceGame/Library/PackageCache/com.gamelovers.mcp-unity@*/Server/logs`
- Server-Git: Terminal output only
- Server-Postgres: `servers/postgres/logs`
- Telemetry Server: `servers/telemetry/logs`
- Telemetry reports: `Documentation/Technical/Telemetry_Reports.md`
- MCPProxy: Terminal output only
- KSA Adapter: Terminal output only

## Future Enhancements

Planned enhancements for the MCP integration:

1. **AI Director Integration**: Connect AI Director system to MCP for dynamic storytelling
2. **Crew Management Tools**: Additional MCP tools for managing crew dynamics
3. **Telemetry Collection**: Implemented telemetry server under `servers/telemetry`
4. **Enhanced Documentation**: Interactive API documentation for all MCP servers
5. **Visual MCP Tool**: GUI interface for interacting with MCP servers
6. **KSA Engine Adapter**: Replace the placeholder with the real engine API when available

---

For more information about the TBD Space Game project, refer to the Gameplay System documentation.

# MCP Unity Integration Guide

## Quick Start

```powershell
# Start the MCP server
.\run-mcp-server.ps1

# Test a menu command
.\test-menu.ps1 -MenuPath "GameObject/Create Empty"
```

## Server Management

### Starting the Server

The recommended way to start the MCP server is using our custom script:

```powershell
.\run-mcp-server.ps1
```

This script handles:
- Path validation
- Environment variable configuration
- Server process management

### Manual Server Start (If Needed)

If you need to start the server manually, use semicolons for command chaining:

```powershell
cd "TBD SpaceGame\TBD SpaceGame\Library\PackageCache\com.gamelovers.mcp-unity@3acfb232f564\Server"; 
$env:UNITY_PORT="8001"; 
node build/index.js
```

❌ DO NOT use the `&&` operator in PowerShell as it causes syntax errors:
```powershell
# This will NOT work
cd "path\to\server" && $env:UNITY_PORT="8001" && node build/index.js
```

## PowerShell Best Practices

1. **Command Chaining**: Use semicolons to chain commands, not `&&`
   ```powershell
   cd "path\to\directory"; Get-ChildItem; node script.js
   ```

2. **Environment Variables**: Use the `$env:` prefix and quotes
   ```powershell
   $env:UNITY_PORT="8001"
   ```

3. **Path Navigation**: Always use double quotes for paths with spaces
   ```powershell
   cd "TBD SpaceGame"
   ```

## Testing Menu Items

Use the test-menu.ps1 script to verify menu integration:

```powershell
# Test standard Unity menu items first
.\test-menu.ps1 -MenuPath "GameObject/Create Empty"

# Then test custom menu items
.\test-menu.ps1 -MenuPath "Basic/Test"
```

## Troubleshooting

### Common Issues

1. **"Module Not Found" Error**:
   - Ensure you're in the correct directory
   - Verify the path in run-mcp-server.ps1

2. **Connection Refused**:
   - Check that port 8001 is not in use
   - Restart Unity if connected previously

3. **Menu Item Not Found**:
   - Ensure script is in Plugins folder (not Editor)
   - Wait for Unity to compile scripts
   - Verify menu item path is correct

### Common Error Messages

Here are specific error messages you might encounter and how to resolve them:

1. **"Error: Cannot find module 'C:\path\to\build\index.js'"**:
   - You're not in the correct directory
   - Solution: Use `Push-Location` and `Pop-Location` to manage directory context
   - Example: `Push-Location "TBD SpaceGame\TBD SpaceGame\Library\PackageCache\com.gamelovers.mcp-unity@3acfb232f564\Server"`

2. **"ParserError: Unexpected token '="8001"' in expression or statement"**:
   - You're using `&&` instead of semicolons for command chaining
   - Solution: Replace `&&` with `;` between commands

3. **"System.Net.WebSockets.WebSocketException: Unable to connect to the remote server"**:
   - MCP server is not running or wrong port
   - Solution: Check if server is running with `netstat -ano | findstr :8001`
   - Start server with `.\run-mcp-server.ps1`

4. **"ExecuteMenuItem failed because there is no menu named 'X'"**:
   - Menu item doesn't exist or Unity hasn't compiled scripts
   - Solution: Wait 30+ seconds after script changes and check for compilation errors
   - Verify exact menu path (case-sensitive)

### GUI Layout Errors

The following errors in the MCP Unity Editor Window are known and non-critical:
```
GUI Error: Invalid GUILayout state in McpUnityEditorWindow view
ArgumentException: Getting control 8's position in a group with only 8 controls
```

These do not affect functionality and can be ignored.

## Setup for New Projects

1. Place menu item scripts in `Assets/Plugins` for proper compilation priority
2. Add `#if UNITY_EDITOR` guards around editor-only code
3. Avoid namespaces in menu item scripts
4. Allow Unity to fully compile after adding new scripts

## Next Steps

After setting up the MCP integration, test with a real workflow:

1. Start the MCP server:
   ```powershell
   .\run-mcp-server.ps1
   ```

2. Create a test ship:
   ```powershell
   .\test-menu.ps1 -MenuPath "MCP/Ship/V2/Create Test Ship"
   ```

3. Apply upgrades to the ship:
   ```powershell
   .\test-menu.ps1 -MenuPath "MCP/Ship/V2/Upgrade Thrust"
   ```

4. Check the results:
   ```powershell
   .\test-menu.ps1 -MenuPath "MCP/Ship/V2/Log Current Upgrades"
   ```

This workflow verifies that menu items are found, commands are executed, and the integration is working properly from end-to-end.

## Further Reading

- [MCP Unity Package Documentation](https://github.com/gamelovers/mcp-unity)
- [Unity Editor Scripting Manual](https://docs.unity3d.com/Manual/ExtendingTheEditor.html) 
## Godot Integration

To import exported assets into Godot:
1. Copy or move the contents of `Assets_glTF/` into the `Godot` project folder.
2. Open the Godot project in the Godot 4 editor.
3. Each `.gltf` file will automatically generate a scene that can be instanced in your C# scripts.



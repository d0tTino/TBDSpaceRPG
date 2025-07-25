# MCP Integration for TBD Space Game

This document provides comprehensive information about the Model Context Protocol (MCP) integration for the TBD Space Game project, including server setup, configuration, and tools developed to enhance AI-assisted game development.

> **Note**
> The project has moved to a **Godot-first** workflow. A vertical slice now runs in **Godot 4 (.NET)**. Unity is only an optional authoring tool for exporting glTF assets to `Assets_glTF/`. All Unity runtime scripts were removed and archived on the `unity-archive` branch.
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

- The TBD Space Game uses MCP (Model Context Protocol) to enable AI-assisted development workflows. Godot is the primary runtime engine, while Unity is used only for optional asset exports. The integration consists of multiple servers:

- **MCP Unity Export Server (optional)**: Provides Unity-based asset exports on port 8001
- **MCP Unity Fallback (optional)**: Backup server for asset exports on port 8002
- **Server-Git**: Version control operations via MCP running on port 8080
- **Server-Postgres**: Database operations via MCP running on port 8003
- **Telemetry Server**: Collects runtime events on port 8090
- **MCPProxy**: Routes requests to appropriate MCP servers running on port 8004
- **KSA Adapter**: Placeholder server for the upcoming KSA engine on port 8005

These servers enable AI tools to interact with the Godot project, which is the default runtime environment. The optional Unity asset pipeline provides glTF exports when needed. They also handle version control operations, manage database records, and more.

## Prerequisites

- Godot 4 or later
- Unity 2022.3 or later (optional, for asset authoring)
- Node.js v18 or later
- npm v9 or later
- PowerShell 7.0 or later (required to run tests)
- Python 3.8 or later (for server-git)
- PostgreSQL (for server-postgres)

## MCP Servers Setup

### Optional MCP Unity Export Server

This server enables asset creation and export through the Unity editor when you choose to use Unity in your pipeline.

```powershell
# Start the optional MCP Unity export server
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
### Telemetry Server

Collects runtime analytics and sends them to the telemetry dashboard.
```powershell
# Start the telemetry server
./run-telemetry-server.ps1
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

The KSA adapter forwards MCP commands to a running KSA engine instance. By
default it looks for an engine on `localhost:9000`, but you can configure a
custom endpoint using environment variables:

- `KSA_ENGINE_HOST` and `KSA_ENGINE_PORT` – specify the host and port
- `KSA_ENGINE_ENDPOINT` – full URL including path (overrides host/port)

```powershell
# Forward MCP commands to http://my-engine.local:9100/engine
$env:KSA_ENGINE_ENDPOINT = 'http://my-engine.local:9100/engine'
./run-ksa-server.ps1
```

The adapter returns the engine's raw HTTP response to the caller.

### All Servers

For convenience, a script is provided to start all MCP servers simultaneously:

```powershell
# Start all MCP servers
./run-all-mcp-servers.ps1
```

## Configuration

### Optional Unity Server Configuration

The optional Unity export server is configured in the `~/.cursor/mcp.json` file:

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

If you use Unity for asset exports, update this configuration with the provided script. Provide the workspace path for the Unity fallback server with the `-Workspace` parameter:

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

Install dependencies with `npm ci` before executing the test scripts. The test suite includes Node-based tests, optional Unity tests, and a Godot test project that requires the `dotnet` CLI.

Run all tests with:

```sh
npm test
```

To run only the Godot tests:

```sh
npm run test:godot
```
This command builds the Godot test project using `dotnet` and then executes the tests in a headless Godot instance.

You can still invoke the original PowerShell script if needed:

```pwsh
pwsh -File tests/test-servers.ps1
```

For installation instructions, see the [PowerShell installation guide](https://learn.microsoft.com/powershell/scripting/install/installing-powershell).

## Linting

Run the linters to ensure consistent code style for both the C# and Node.js projects.

### .NET Formatting

Use `dotnet format` to verify that the C# projects follow the repository's style guidelines:

```sh
dotnet format Godot/TBDSpaceRPG.csproj
dotnet format csharp/OrbitalMechanics.sln
```

### ESLint

The Node.js servers use ESLint for static analysis. Install dependencies and run:

```sh
npm run lint
```

## Ship Customization System

The Ship Customization System (implemented according to Section 4.16 of the technical documentation) consists of:

1. **SpaceshipMovement.cs**: Core component handling ship movement with customizable parameters
2. **ShipCustomizationManager.cs**: Manages upgrade categories and levels
3. **ShipUpgradeMenuHelper.cs**: Editor script that provides menu items for MCP integration

> **Note**
> `ShipCustomizationManager` must belong to the `"ShipCustomizationManager"` group so editor menu items can locate it. The provided Godot plugin automatically adds this group when creating a test ship.

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

- MCP Unity Export Server: `TBD SpaceGame/TBD SpaceGame/Library/PackageCache/com.gamelovers.mcp-unity@*/Server/logs`
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

# MCP Unity Integration Guide (Optional)

## Quick Start

```powershell
# Start the optional MCP Unity export server
.\run-mcp-server.ps1

# Test a menu command (Unity only)
.\test-menu.ps1 -MenuPath "GameObject/Create Empty"
```

## Server Management

### Starting the Server

The recommended way to start the optional Unity server is using our custom script:

```powershell
.\run-mcp-server.ps1
```

This script handles:
- Path validation
- Environment variable configuration
- Server process management
### Safe MCP Server Restart
Use `safe-mcp-restart.ps1` to restart the server without terminating unrelated Node processes:
```powershell
./safe-mcp-restart.ps1
```

### Restart After Unity Update
If Unity updates its packages, run `restart-mcp-after-unity-update.ps1` to reconnect:
```powershell
./restart-mcp-after-unity-update.ps1
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
   - Solution: Check if server is running with `Test-NetConnection -ComputerName localhost -Port 8001 -InformationLevel Quiet`
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

If you plan to use Unity for asset authoring, follow these steps in the Unity project:
1. Place menu item scripts in `Assets/Plugins` for proper compilation priority
2. Add `#if UNITY_EDITOR` guards around editor-only code
3. Avoid namespaces in menu item scripts
4. Allow Unity to fully compile after adding new scripts

## Next Steps

After setting up the MCP integration, test with a real workflow:

1. If using Unity, start the optional MCP Unity export server:
   ```powershell
   .\run-mcp-server.ps1
   ```

2. Create a test ship (Unity only):
   ```powershell
   .\test-menu.ps1 -MenuPath "MCP/Ship/V2/Create Test Ship"
   ```

3. Apply upgrades to the ship (Unity only):
   ```powershell
   .\test-menu.ps1 -MenuPath "MCP/Ship/V2/Upgrade Thrust"
   ```

4. Check the results (Unity only):
   ```powershell
   .\test-menu.ps1 -MenuPath "MCP/Ship/V2/Log Current Upgrades"
   ```

This workflow verifies that Unity menu items are found, commands are executed, and the integration is working properly from end-to-end.

## Further Reading

- [MCP Unity Package Documentation](https://github.com/gamelovers/mcp-unity)
- [Unity Editor Scripting Manual](https://docs.unity3d.com/Manual/ExtendingTheEditor.html) 
## Godot Integration

Godot is now the main runtime engine for TBD Space Game. Assets exported from Unity or Blender should be placed in `Assets_glTF/` so the Godot project can import them automatically.
A playable vertical slice built with **Godot 4 (.NET)** demonstrates the core systems. Unity runtime scripts were removed to focus on this workflow.

### Exporting glTF Assets
Use a glTF exporter (such as Unity's glTFast or Blender's exporter) to convert prefabs and models. Export them into `Assets_glTF/` so both Unity and Godot consume the same files.

### Command Line Asset Exporters
The Unity project included lightweight exporters that ran without opening the editor. These files are now archived on the `unity-archive` branch. If you need them, check out that branch and run:

```sh
./TBD\ SpaceGame/Assets/Editor/run-exporters.sh
```

The exporter builds `Exporters.csproj` and writes a sample `.gltf` file to `Assets_glTF/` and a JSON data file to `Gameplay_Data/`. Integrate it into your CI pipeline to regenerate assets automatically.

### Unified Export Tool

Run the following PowerShell script to copy exported assets into the Godot project. It will run the Unity exporters when they are available:

```powershell
./run-exporters-to-godot.ps1
```

The script copies glTF files into `Godot/Assets_glTF/` and gameplay data into `Godot/Gameplay_Data/` so the assets are ready for import when you open the Godot editor. If the Unity exporters are missing, checkout the `unity-archive` branch to access them.

### Starting Servers for Godot
Run all MCP servers configured for Godot with:
```powershell
./run-all-mcp-servers.ps1 -Engine godot
```

To import exported assets into Godot:
1. Run `./run-exporters-to-godot.ps1`.
2. Open the Godot project in the Godot 4 editor.
3. Each `.gltf` file will automatically generate a scene that can be instanced in your C# scripts.
### Running the Godot 4 (.NET) Vertical Slice
After the servers are up, open the project in `Godot/` with the Godot 4 editor and press **Play**. The vertical slice connects to the MCP servers using the endpoint defined in `project.godot`.


### Configuring the MCP Endpoint

The Godot project reads the MCP endpoint from the `mcp_endpoint` setting in
`project.godot`. The default is `http://localhost:8001/mcp`. Update this value to
point at a different server without modifying any scripts.

```ini
[application]
mcp_endpoint="http://localhost:8001/mcp"
```

`McpClient.cs` automatically picks up this value during startup, so changing it
in the configuration or via the Godot editor is enough to redirect requests.



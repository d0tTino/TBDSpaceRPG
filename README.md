# TBD Space RPG

This project houses the development files for a space‑faring role‑playing game focused on managing a multi‑generation starship crew. The codebase is centered on **Godot 4 (.NET)**, which is the default engine used for gameplay and testing. Unity remains optional and is primarily used for exporting glTF assets.

For details on how the MCP (Model Context Protocol) servers integrate with the game, see [README-MCP-Integration.md](README-MCP-Integration.md). Additional design notes and technical references are located in the [Documentation](Documentation/) directory.

## Getting Started

Install dependencies and run tests:

```bash
npm ci
npm run lint
npm test
```

Start all MCP servers with Godot:

```powershell
./run-all-mcp-servers.ps1 -Engine godot
```

Export glTF assets from Unity or Blender:
```powershell
./export-assets.ps1
```
For Unix systems use `./export-assets.sh` instead.

Export USD assets for O3DE:
```powershell
./export-to-usd.ps1
```
For Unix systems use `./export-to-usd.sh`.

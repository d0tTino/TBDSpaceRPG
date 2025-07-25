# Migrating from Unity to Godot

These steps outline how to move existing assets and workflows to the Godot pipeline.

1. Export Unity prefabs or Blender models to the glTF format using a compatible exporter.
2. Place all `.gltf` files in the `Assets_glTF/` directory at the project root so the Godot project can import them.
3. Open the project located in the `Godot/` folder. The vertical slice is built with **Godot 4 (.NET)** and automatically imports the glTF files as scenes.
4. Start the MCP servers configured for Godot:
   ```powershell
   ./run-all-mcp-servers.ps1 -Engine godot
   ```
5. Unity runtime scripts have been removed. Use the C# scripts in the Godot project to handle gameplay logic.
6. Run the Godot project from the editor and verify that it connects to the MCP servers at startup.

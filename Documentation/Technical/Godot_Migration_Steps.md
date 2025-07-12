# Migrating from Unity to Godot

These steps outline how to move existing assets and workflows to the Godot pipeline.

1. Export Unity prefabs or Blender models to the glTF format using a compatible exporter.
2. Place all `.gltf` files in the `Assets_glTF/` directory at the project root.
3. Open the project located in the `Godot/` folder. Godot will automatically import the glTF files and create scenes.
4. Start the MCP servers configured for Godot:
   ```powershell
   ./run-all-mcp-servers.ps1 -Engine godot
   ```
5. Replace Unity-specific scripts with Godot equivalents and validate that gameplay features behave the same.

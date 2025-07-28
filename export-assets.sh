#!/usr/bin/env bash
set -e
repo_root="$(cd "$(dirname "$0")" && pwd)"
project_path="$repo_root/TBD SpaceGame/TBD SpaceGame"
blender_script="$repo_root/blender/export-assets.py"
blender_project="$repo_root/blender/project.blend"

if command -v unity-editor >/dev/null 2>&1; then
    echo "Running Unity glTF exporter..."
    unity-editor -batchmode -nographics -quit -projectPath "$project_path" -executeMethod Exporters.ExportGLTF
elif command -v blender >/dev/null 2>&1; then
    if [[ -f "$blender_script" && -f "$blender_project" ]]; then
        echo "Running Blender glTF exporter..."
        blender -b "$blender_project" -P "$blender_script"
    else
        echo "Blender exporter not found. Skipping glTF export."
    fi
else
    echo "Neither unity-editor nor blender found. Skipping glTF export."
fi

dest_root="$repo_root/Assets_glTF"
godot_dest="$repo_root/Godot/Assets_glTF"
mkdir -p "$dest_root" "$godot_dest"

find "$project_path" \( -name '*.gltf' -o -name '*.glb' \) -type f 2>/dev/null | while read -r file; do
    cp "$file" "$dest_root" || true
    cp "$file" "$godot_dest" || true
done

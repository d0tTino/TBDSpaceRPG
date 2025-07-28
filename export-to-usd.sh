#!/usr/bin/env bash
set -e
repo_root="$(cd "$(dirname "$0")" && pwd)"
src_dir="$repo_root/Assets_glTF"
dest_dir="$repo_root/Assets_USD"
mkdir -p "$dest_dir"

if ! command -v usd_from_gltf >/dev/null 2>&1; then
    echo "usd_from_gltf not found. Install USD tools." >&2
    exit 1
fi

shopt -s nullglob
for file in "$src_dir"/*.gltf "$src_dir"/*.glb; do
    base="$(basename "${file%.*}")"
    usd_file="$dest_dir/$base.usda"
    usd_from_gltf "$file" "$usd_file"
    if command -v usdzip >/dev/null 2>&1; then
        usdzip "$usd_file" "$dest_dir/$base.usdz" -f
    fi
    echo "Converted $file -> $usd_file"
done
shopt -u nullglob

echo "USD files exported to $dest_dir"

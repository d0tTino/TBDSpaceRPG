$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$unityCmd = Get-Command unity-editor -ErrorAction SilentlyContinue
$projectPath = Join-Path $repoRoot 'TBD SpaceGame/TBD SpaceGame'
if ($unityCmd) {
    Write-Host "Running Unity glTF exporter..."
    & $unityCmd.Source -batchmode -nographics -quit -projectPath $projectPath -executeMethod Exporters.ExportGLTF
} else {
    Write-Warning "unity-editor not found. Skipping glTF export."
}

$destRoot = Join-Path $repoRoot 'Assets_glTF'
$godotDest = Join-Path $repoRoot 'Godot/Assets_glTF'
New-Item -ItemType Directory -Force -Path $destRoot | Out-Null
New-Item -ItemType Directory -Force -Path $godotDest | Out-Null

Get-ChildItem $projectPath -Recurse -Include *.gltf,*.glb,*.png,*.jpg -ErrorAction SilentlyContinue | ForEach-Object {
    Copy-Item $_.FullName -Destination $destRoot -Force
    Copy-Item $_.FullName -Destination $godotDest -Force
}

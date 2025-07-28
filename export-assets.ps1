$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$unityCmd = Get-Command unity-editor -ErrorAction SilentlyContinue
$blenderCmd = Get-Command blender -ErrorAction SilentlyContinue
$projectPath = Join-Path $repoRoot 'TBD SpaceGame/TBD SpaceGame'
$blenderScript = Join-Path $repoRoot 'blender/export-assets.py'
$blenderProject = Join-Path $repoRoot 'blender/project.blend'

if ($unityCmd) {
    Write-Host 'Running Unity glTF exporter...'
    & $unityCmd.Source -batchmode -nographics -quit -projectPath $projectPath -executeMethod Exporters.ExportGLTF
} elseif ($blenderCmd) {
    if (Test-Path $blenderScript -and Test-Path $blenderProject) {
        Write-Host 'Running Blender glTF exporter...'
        & $blenderCmd.Source -b $blenderProject -P $blenderScript
    } else {
        Write-Warning 'Blender exporter not found. Skipping glTF export.'
    }
} else {
    Write-Warning 'Neither unity-editor nor blender found. Skipping glTF export.'
}

$destRoot = Join-Path $repoRoot 'Assets_glTF'
$godotDest = Join-Path $repoRoot 'Godot/Assets_glTF'
New-Item -ItemType Directory -Force -Path $destRoot | Out-Null
New-Item -ItemType Directory -Force -Path $godotDest | Out-Null

Get-ChildItem $projectPath -Recurse -Include *.gltf,*.glb -ErrorAction SilentlyContinue | ForEach-Object {
    Copy-Item $_.FullName -Destination $destRoot -Force
    Copy-Item $_.FullName -Destination $godotDest -Force
}

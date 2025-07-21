param(
    [string]$GodotPath = "Godot"
)

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $repoRoot
try {
    # Run the Unity exporters
    & './TBD SpaceGame/Assets/Editor/run-exporters.sh'

    $gltfSource = Join-Path $repoRoot 'Assets_glTF'
    $dataSource = Join-Path $repoRoot 'Gameplay_Data'

    $gltfDest = Join-Path $GodotPath 'Assets_glTF'
    $dataDest = Join-Path $GodotPath 'Gameplay_Data'

    New-Item -ItemType Directory -Force -Path $gltfDest | Out-Null
    New-Item -ItemType Directory -Force -Path $dataDest | Out-Null

    Copy-Item -Path (Join-Path $gltfSource '*') -Destination $gltfDest -Recurse -Force
    Copy-Item -Path (Join-Path $dataSource '*') -Destination $dataDest -Recurse -Force
}
finally {
    Pop-Location
}

Write-Host "Exported assets copied to $GodotPath"

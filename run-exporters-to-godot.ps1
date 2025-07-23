param(
    [string]$GodotPath = "Godot"
)

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $repoRoot
try {
    # Run the Unity exporters if available
    $exporterScript = Join-Path $repoRoot 'TBD SpaceGame/Assets/Editor/run-exporters.sh'
    if (Test-Path $exporterScript) {
        & $exporterScript
    } else {
        Write-Host 'Export script not found. Switch to the unity-archive branch for the legacy Unity exporters.'
    }

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

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$srcDir = Join-Path $repoRoot 'Assets_glTF'
$destDir = Join-Path $repoRoot 'Assets_USD'
New-Item -ItemType Directory -Force -Path $destDir | Out-Null

$usdFromGltf = Get-Command usd_from_gltf -ErrorAction SilentlyContinue
if (-not $usdFromGltf) {
    Write-Warning 'usd_from_gltf not found. Install USD tools.'
    exit 1
}
$usdzip = Get-Command usdzip -ErrorAction SilentlyContinue

Get-ChildItem $srcDir -Include *.gltf,*.glb -Recurse | ForEach-Object {
    $base = [System.IO.Path]::GetFileNameWithoutExtension($_.FullName)
    $usdPath = Join-Path $destDir ("$base.usda")
    & $usdFromGltf.Source $_.FullName $usdPath
    if ($usdzip) {
        & $usdzip.Source $usdPath (Join-Path $destDir ("$base.usdz")) -f
    }
    Write-Host "Converted $($_.Name) -> $usdPath"
}
Write-Host "USD files exported to $destDir"

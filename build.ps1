param([string]$Version = "0.0.0")

$ErrorActionPreference = "Stop"
$Version = $Version.TrimStart("v")
$dist = Join-Path $PSScriptRoot "dist"

if (Test-Path $dist) {
    Remove-Item $dist -Recurse -Force
}

dotnet publish "$PSScriptRoot/src/mushroomarizer.csproj" -c Release -o "$dist/mushroomarizer" -p:Version=$Version -p:DebugType=none
if ($LASTEXITCODE) {
    exit $LASTEXITCODE
}

Compress-Archive "$dist/mushroomarizer/*" "$dist/mushroomarizer.zip"
Write-Host "Built $dist/mushroomarizer.zip (version $Version)"

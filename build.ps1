#Requires -Version 5.1
# Builds Mouse Effects for every Windows architecture:
# self-contained publishes, portable zips and Inno Setup installers.
# Output goes to .\installer

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$archs = @('x64', 'x86', 'arm64')
$rids = @{ x64 = 'win-x64'; x86 = 'win-x86'; arm64 = 'win-arm64' }

$version = (Select-String -Path 'installer.iss' -Pattern '#define MyAppVersion "(.+)"').Matches.Groups[1].Value
if (-not $version) { throw 'Could not read MyAppVersion from installer.iss' }

$iscc = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw 'Inno Setup 6 (ISCC.exe) not found.' }

$outDir = 'installer'
$stageRoot = Join-Path $env:TEMP 'mefx-build'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

foreach ($arch in $archs) {
    $rid = $rids[$arch]
    $pub = "publish-$arch"
    Write-Host "==> $arch ($rid)" -ForegroundColor Cyan

    Remove-Item -Recurse -Force $pub -ErrorAction SilentlyContinue
    dotnet publish MouseEffects.csproj -c Release -r $rid --self-contained true -o $pub
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $rid" }

    $zipName = "MouseEffects_win_${arch}_${version}.zip"
    $stage = Join-Path $stageRoot "MouseEffects_win_${arch}_${version}"
    Remove-Item -Recurse -Force $stageRoot -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $stage | Out-Null
    Copy-Item "$pub\*" $stage -Recurse
    Get-ChildItem $stage -Recurse -Filter *.pdb | Remove-Item -Force
    $zipPath = Join-Path $outDir $zipName
    Remove-Item $zipPath -ErrorAction SilentlyContinue
    Compress-Archive -Path $stage -DestinationPath $zipPath -Force
    Write-Host "    $zipName"

    $setupName = "MouseEffects_win_${arch}_${version}_setup.exe"
    & $iscc "/DArch=$arch" installer.iss | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "ISCC failed for $arch" }
    Write-Host "    $setupName"
}

Remove-Item -Recurse -Force $stageRoot -ErrorAction SilentlyContinue
Write-Host 'Done. Artifacts are in .\installer' -ForegroundColor Green

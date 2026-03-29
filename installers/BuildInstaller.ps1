[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$Trimmed,
    [switch]$FrameworkDependent,
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "Bombermaaan\Bombermaaan.csproj"
$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

[xml]$projectXml = Get-Content -Path $projectPath
$propertyGroup = $projectXml.Project.PropertyGroup | Select-Object -First 1
$appVersion = $propertyGroup.AssemblyVersion

if ([string]::IsNullOrWhiteSpace($appVersion)) {
    throw "AssemblyVersion could not be read from $projectPath"
}

$publishDir = Join-Path $repoRoot "publish\$Runtime"
$releaseRoot = Join-Path $repoRoot "releases\$Runtime"
$releaseDir = Join-Path $releaseRoot "Bombermaaan_$appVersion"
$outputDir = Join-Path $PSScriptRoot "output"

function Remove-SatelliteResourceDirectories {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath
    )

    Get-ChildItem -Path $RootPath -Directory | ForEach-Object {
        $files = Get-ChildItem -Path $_.FullName -File
        $dirs = Get-ChildItem -Path $_.FullName -Directory
        $containsOnlySatelliteAssemblies =
            $dirs.Count -eq 0 -and
            $files.Count -gt 0 -and
            ($files | Where-Object { $_.Name -notlike "*.resources.dll" }).Count -eq 0

        if ($containsOnlySatelliteAssemblies) {
            Remove-Item -LiteralPath $_.FullName -Recurse -Force
        }
    }

    Get-ChildItem -Path $RootPath -Directory | ForEach-Object {
        if ((Get-ChildItem -Path $_.FullName -Force | Measure-Object).Count -eq 0) {
            Remove-Item -LiteralPath $_.FullName -Force
        }
    }
}

Write-Host "Publishing Bombermaaan $appVersion for $Runtime..."

$dotnetArgs = @(
    "publish", $projectPath,
    "-c", $Configuration,
    "-r", $Runtime,
    "-p:PublishSingleFile=false",
    "-o", $publishDir
)

if ($FrameworkDependent) {
    $dotnetArgs += @("--self-contained", "false")
}
else {
    $dotnetArgs += @("--self-contained", "true")
}

if ($Trimmed) {
    $dotnetArgs += @(
        "-p:PublishTrimmed=true",
        "-p:TrimMode=partial"
    )
}

& dotnet @dotnetArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Remove-SatelliteResourceDirectories -RootPath $publishDir

New-Item -ItemType Directory -Path $releaseRoot -Force | Out-Null
if (Test-Path $releaseDir) {
    Remove-Item -LiteralPath $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

Copy-Item -Path (Join-Path $publishDir "*") -Destination $releaseDir -Recurse -Force
Copy-Item -LiteralPath (Join-Path $repoRoot "COPYING.txt") -Destination (Join-Path $releaseDir "COPYING.txt") -Force
Copy-Item -LiteralPath (Join-Path $repoRoot "README.md") -Destination (Join-Path $releaseDir "README.txt") -Force

Write-Host "Release folder prepared at: $releaseDir"

if ($SkipInstaller) {
    Write-Host "Skipping installer generation by request."
    exit 0
}

$isccCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
)

$isccPath = $isccCandidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if (-not $isccPath) {
    Write-Warning "Inno Setup 6 was not found. Release folder is ready, but no setup.exe was created."
    exit 0
}

New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$issPath = Join-Path $PSScriptRoot "InstallScriptWin64.iss"
$isccArgs = @(
    "/DAppVersion=$appVersion",
    "/DSourceDir=$releaseDir",
    "/DRepoRoot=$repoRoot",
    $issPath
)

Write-Host "Building installer with Inno Setup..."
& $isccPath @isccArgs
if ($LASTEXITCODE -ne 0) {
    throw "ISCC failed with exit code $LASTEXITCODE"
}

Write-Host "Installer output directory: $outputDir"

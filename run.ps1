param(
    [Parameter(Position = 0, Mandatory = $true)]
    [ValidateSet("winform", "wpf")]
    [string]$Target,

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
if (Test-Path "$env:SystemRoot\System32\chcp.com") {
    & "$env:SystemRoot\System32\chcp.com" 65001 | Out-Null
}

$Target = $Target.ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    throw "OCCT_ROOT is not configured. Pass -OcctRoot <path> or set the OCCT_ROOT environment variable."
}

$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

function Add-PathEntry {
    param([Parameter(Mandatory = $true)][string]$Directory)

    if (-not (Test-Path $Directory -PathType Container)) {
        return
    }

    $fullDirectory = [System.IO.Path]::GetFullPath($Directory).TrimEnd('\')
    $currentPath = [Environment]::GetEnvironmentVariable("PATH")
    if ($null -eq $currentPath) {
        $currentPath = ""
    }

    $alreadyPresent = $false
    foreach ($entry in $currentPath.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        try {
            $normalizedEntry = [System.IO.Path]::GetFullPath($entry).TrimEnd('\')
            if ($normalizedEntry.Equals($fullDirectory, [System.StringComparison]::OrdinalIgnoreCase)) {
                $alreadyPresent = $true
                break
            }
        }
        catch {
            # Ignore malformed PATH entries owned by other applications.
        }
    }

    if (-not $alreadyPresent) {
        $env:PATH = if ([string]::IsNullOrEmpty($currentPath)) { $fullDirectory } else { "$fullDirectory;$currentPath" }
    }
}

if (-not (Test-Path $OcctBinDir -PathType Container)) {
    throw "OCCT runtime directory was not found: $OcctBinDir"
}

$apps = @{
    winform = "src\CadWinForms\bin\x64\$Configuration\net8.0-windows\CAD-Winform.exe"
    wpf = "src\CadWpf\bin\x64\$Configuration\net8.0-windows\CAD-WPF.exe"
}

$executable = Join-Path $RepoRoot $apps[$Target]
if (-not (Test-Path $executable -PathType Leaf)) {
    throw "Executable was not found: $executable`nRun: .\build.ps1 $Target $Configuration -OcctRoot `"$OcctRoot`""
}

$applicationDirectory = Split-Path -Parent $executable
$nativeBridge = Join-Path $applicationDirectory "OcctNative.dll"
if (-not (Test-Path $nativeBridge -PathType Leaf)) {
    throw "OcctNative.dll was not found beside the application: $nativeBridge"
}

$env:OCCT_ROOT = $OcctRoot
$env:CASROOT = $OcctRoot
$env:OCCT_BRIDGE_NATIVE_DIR = $applicationDirectory
Add-PathEntry $applicationDirectory
Add-PathEntry $OcctBinDir

if (Test-Path $OcctThirdPartyDir -PathType Container) {
    Get-ChildItem $OcctThirdPartyDir -Directory | Sort-Object Name | ForEach-Object {
        Add-PathEntry (Join-Path $_.FullName "bin")
        Add-PathEntry (Join-Path $_.FullName "bin\win64")
        Add-PathEntry (Join-Path $_.FullName "bin\x64")
    }
}

Write-Host "Application: $executable"
Write-Host "OCCT root:  $OcctRoot" -ForegroundColor DarkGray

Push-Location $applicationDirectory
try {
    & $executable
    if ($LASTEXITCODE -ne 0) {
        throw "$Target exited with code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

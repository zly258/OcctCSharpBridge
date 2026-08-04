param(
    [Parameter(Position = 0, Mandatory = $true)]
    [ValidateSet("winform", "wpf")]
    [string]$Target,

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release"
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
$OcctRoot = "D:\tools\occt-vc144-64"
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

function Add-PathEntry {
    param([Parameter(Mandatory = $true)][string]$Directory)

    if (-not (Test-Path $Directory -PathType Container)) {
        return
    }

    $currentPath = [Environment]::GetEnvironmentVariable("PATH")
    if ($null -eq $currentPath) {
        $currentPath = ""
    }

    $entries = $currentPath.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)
    if ($entries -contains $Directory) {
        return
    }

    $env:PATH = if ([string]::IsNullOrEmpty($currentPath)) {
        $Directory
    }
    else {
        "$Directory;$currentPath"
    }
}

if (-not (Test-Path $OcctBinDir -PathType Container)) {
    throw "OCCT runtime directory was not found: $OcctBinDir"
}
if (-not (Test-Path $OcctThirdPartyDir -PathType Container)) {
    throw "OCCT third-party directory was not found: $OcctThirdPartyDir"
}

$env:CASROOT = $OcctRoot
Add-PathEntry $OcctBinDir

# Only inspect direct component directories under the fixed OCCT third-party root.
Get-ChildItem $OcctThirdPartyDir -Directory | Sort-Object Name | ForEach-Object {
    Add-PathEntry (Join-Path $_.FullName "bin")
    Add-PathEntry (Join-Path $_.FullName "bin\win64")
    Add-PathEntry (Join-Path $_.FullName "bin\x64")
}

$apps = @{
    winform = @{
        Path = "src\CadWinForms\bin\x64\$Configuration\net8.0-windows\CAD-Winform.exe"
    }
    wpf = @{
        Path = "src\CadWpf\bin\x64\$Configuration\net8.0-windows\CAD-WPF.exe"
    }
}

$executable = Join-Path $RepoRoot $apps[$Target].Path
if (-not (Test-Path $executable -PathType Leaf)) {
    throw "Executable was not found: $executable`nRun: .\build.ps1 $Target $Configuration"
}

$env:OCCT_BRIDGE_NATIVE_DIR = Split-Path -Parent $executable
Push-Location (Split-Path -Parent $executable)
try {
    & $executable
}
finally {
    Pop-Location
}

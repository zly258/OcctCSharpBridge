param(
    [Parameter(Position = 0)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT,

    [switch]$Build
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$EditorOutput = Join-Path $RepoRoot "src\OcctScript.Editor\bin\x64\$Configuration\net8.0-windows"
$EditorExe = Join-Path $EditorOutput "OcctScript.Editor.exe"
$NativeDll = Join-Path $EditorOutput "OcctNative.dll"

if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) {
    $OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
    if (-not (Test-Path $OcctRoot -PathType Container)) {
        throw "OCCT root was not found: $OcctRoot"
    }

    $env:OCCT_ROOT = $OcctRoot
    $OcctBin = Join-Path $OcctRoot "win64\vc14\bin"
    if (-not (Test-Path $OcctBin -PathType Container)) {
        throw "OCCT runtime directory was not found: $OcctBin"
    }

    $pathEntries = $env:PATH -split ';'
    if ($pathEntries -notcontains $OcctBin) {
        $env:PATH = "$OcctBin;$env:PATH"
    }
}

if ($Build) {
    $buildScript = Join-Path $RepoRoot "build.ps1"
    $arguments = @("script", $Configuration)
    if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) {
        $arguments += @("-OcctRoot", $OcctRoot)
    }

    & $buildScript @arguments
    if (-not $?) {
        throw "OcctScript build failed."
    }
}

if (-not (Test-Path $EditorExe -PathType Leaf)) {
    throw "OcctScript.Editor was not found. Build it first with: .\build.ps1 script $Configuration"
}

if (-not (Test-Path $NativeDll -PathType Leaf)) {
    throw "OcctNative.dll was not found beside OcctScript.Editor.exe. Rebuild with: .\build.ps1 script $Configuration"
}

Write-Host "Starting OcctScript Editor..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration"
Write-Host "Editor:        $EditorExe" -ForegroundColor DarkGray
if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) {
    Write-Host "OCCT root:     $OcctRoot" -ForegroundColor DarkGray
}

Start-Process -FilePath $EditorExe -WorkingDirectory $EditorOutput

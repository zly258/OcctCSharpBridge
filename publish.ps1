param(
    [Parameter(Position = 0)]
    [ValidateSet("all", "winform", "wpf", "avalonia")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$OutputDirectory = "",
    [switch]$SelfContained,
    [switch]$FrameworkDependent,
    [switch]$Zip,
    [switch]$KeepExisting
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ($SelfContained.IsPresent -and $FrameworkDependent.IsPresent) {
    throw "Use either -SelfContained or -FrameworkDependent, not both."
}

if ($Target -eq "all" -and $SelfContained.IsPresent) {
    throw "Unified 'all' publish cannot be self-contained because WinForms/WPF/Avalonia Windows Desktop publish closures contain conflicting framework DLLs. Use '.\publish.ps1 all ...' for the shared framework-dependent package, or publish winform/wpf/avalonia separately with -SelfContained."
}

$implementation = Join-Path (Split-Path -Parent $PSCommandPath) "publish.impl.ps1"
if (-not (Test-Path -LiteralPath $implementation -PathType Leaf)) {
    throw "Windows publish implementation was not found: $implementation"
}

$forward = @{
    Target = $Target
    Configuration = $Configuration
    OcctRoot = $OcctRoot
    OutputDirectory = $OutputDirectory
}

if ($Target -eq "all") {
    $forward.FrameworkDependent = $true
    Write-Host "[publish] Unified Windows package uses framework-dependent .NET 10 Desktop Runtime to avoid duplicate/conflicting framework DLLs." -ForegroundColor DarkGray
}
elseif ($FrameworkDependent.IsPresent) {
    $forward.FrameworkDependent = $true
}
elseif ($SelfContained.IsPresent) {
    $forward.SelfContained = $true
}

if ($Zip.IsPresent) { $forward.Zip = $true }
if ($KeepExisting.IsPresent) { $forward.KeepExisting = $true }

& $implementation @forward
if (-not $?) { throw "Windows Demo publish failed." }

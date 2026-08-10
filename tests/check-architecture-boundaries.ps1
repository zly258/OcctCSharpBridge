param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Project {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Project file was not found: $RelativePath"
    }
    [xml](Get-Content $path -Raw -Encoding UTF8)
}

function Get-ProjectReferences {
    param([Parameter(Mandatory = $true)][xml]$Project)
    return @($Project.Project.ItemGroup | ForEach-Object {
        @($_.ProjectReference) | ForEach-Object {
            if ($_ -and $_.Include) { [string]$_.Include }
        }
    })
}

function Get-PackageReferences {
    param([Parameter(Mandatory = $true)][xml]$Project)
    return @($Project.Project.ItemGroup | ForEach-Object {
        @($_.PackageReference) | ForEach-Object {
            if ($_ -and $_.Include) { [string]$_.Include }
        }
    })
}

function Assert-Reference {
    param([string[]]$References, [string]$Expected, [string]$ProjectName)
    $normalizedExpected = $Expected.Replace('/', '\')
    $matches = @($References | Where-Object { $_.Replace('/', '\') -eq $normalizedExpected })
    if ($matches.Count -ne 1) {
        throw "$ProjectName must reference exactly once: $Expected"
    }
}

$core = Read-Project "src/OcctNet/OcctNet.csproj"
$winForms = Read-Project "src/OcctNet.WinForms/OcctNet.WinForms.csproj"
$wpf = Read-Project "src/OcctNet.Wpf/OcctNet.Wpf.csproj"
$avalonia = Read-Project "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj"

$coreProjectReferences = Get-ProjectReferences $core
$corePackageReferences = Get-PackageReferences $core
if ($coreProjectReferences.Count -ne 0) {
    throw "OcctNet core must not depend on UI host projects."
}
foreach ($uiDependency in @("Avalonia", "PresentationFramework", "System.Windows.Forms")) {
    if ($uiDependency -in $corePackageReferences) {
        throw "OcctNet core must remain UI-framework independent: $uiDependency"
    }
}

Assert-Reference (Get-ProjectReferences $winForms) "..\OcctNet\OcctNet.csproj" "OcctNet.WinForms"
Assert-Reference (Get-ProjectReferences $wpf) "..\OcctNet.WinForms\OcctNet.WinForms.csproj" "OcctNet.Wpf"
Assert-Reference (Get-ProjectReferences $avalonia) "..\OcctNet\OcctNet.csproj" "OcctNet.Avalonia"

$winFormsEnabled = @($winForms.Project.PropertyGroup | ForEach-Object { [string]$_.UseWindowsForms } | Where-Object { $_ }) | Select-Object -First 1
if ($winFormsEnabled -ne "true") {
    throw "OcctNet.WinForms must enable Windows Forms."
}

$wpfEnabled = @($wpf.Project.PropertyGroup | ForEach-Object { [string]$_.UseWPF } | Where-Object { $_ }) | Select-Object -First 1
$wpfWinFormsEnabled = @($wpf.Project.PropertyGroup | ForEach-Object { [string]$_.UseWindowsForms } | Where-Object { $_ }) | Select-Object -First 1
if ($wpfEnabled -ne "true" -or $wpfWinFormsEnabled -ne "true") {
    throw "OcctNet.Wpf must enable WPF and Windows Forms hosting."
}

$avaloniaPackages = Get-PackageReferences $avalonia
if ("Avalonia" -notin $avaloniaPackages) {
    throw "OcctNet.Avalonia must reference the Avalonia package."
}

foreach ($demoPath in @(
    "src/OcctDemo.Common",
    "src/OcctDemo.WinForms",
    "src/OcctDemo.Wpf",
    "src/OcctDemo.Avalonia",
    "src/CadCommon",
    "src/CadWinForms",
    "src/CadWpf",
    "src/CadAvalonia"
)) {
    if (Test-Path (Join-Path $RepositoryRoot $demoPath)) {
        throw "Reusable main SDK must not contain application/demo project: $demoPath"
    }
}

$managedRoot = Join-Path $RepositoryRoot "src\OcctNet"
$managedText = (Get-ChildItem $managedRoot -Filter '*.cs' -File -Recurse |
    ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"

foreach ($forbidden in @(
    "DocumentManager",
    "CommandBus",
    "CommandRegistry",
    "ToolManager",
    "OcafDocument"
)) {
    if ($managedText -match "\b$([regex]::Escape($forbidden))\b") {
        throw "Application-layer type must not enter OcctNet core: $forbidden"
    }
}

foreach ($legacyFile in @(
    "src/OcctNet/OcctObject.Legacy.cs",
    "src/OcctNet/OcctGeometryExtensions.Compatibility.cs",
    "src/OcctNet/OcctEngine.ApiAliases.cs",
    "src/OcctNet/NativeMethods.Modeling.cs"
)) {
    if (Test-Path (Join-Path $RepositoryRoot $legacyFile)) {
        throw "Legacy/compatibility source must not be reintroduced: $legacyFile"
    }
}

Write-Host "[architecture] Core/UI dependency direction, main/demo boundary, and no-compatibility application boundary validated." -ForegroundColor Green

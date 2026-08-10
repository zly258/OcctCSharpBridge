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

    return [xml](Get-Content $path -Raw -Encoding UTF8)
}

function Get-ProjectReferences {
    param([Parameter(Mandatory = $true)][xml]$Project)

    $nodes = @($Project.SelectNodes('/Project/ItemGroup/ProjectReference'))
    return @($nodes | ForEach-Object {
        [string]$_.GetAttribute('Include')
    } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-PackageReferences {
    param([Parameter(Mandatory = $true)][xml]$Project)

    $nodes = @($Project.SelectNodes('/Project/ItemGroup/PackageReference'))
    return @($nodes | ForEach-Object {
        [string]$_.GetAttribute('Include')
    } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-ProjectProperty {
    param(
        [Parameter(Mandatory = $true)][xml]$Project,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $node = $Project.SelectSingleNode("/Project/PropertyGroup/$Name[normalize-space(.) != '']")
    if ($null -eq $node) { return $null }
    return [string]$node.InnerText
}

function Assert-Reference {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$References,
        [Parameter(Mandatory = $true)][string]$Expected,
        [Parameter(Mandatory = $true)][string]$ProjectName
    )

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

$coreProjectReferences = @(Get-ProjectReferences $core)
$corePackageReferences = @(Get-PackageReferences $core)
if ($coreProjectReferences.Count -ne 0) {
    throw "OcctNet core must not depend on UI host projects."
}
foreach ($uiDependency in @("Avalonia", "PresentationFramework", "System.Windows.Forms")) {
    if ($uiDependency -in $corePackageReferences) {
        throw "OcctNet core must remain UI-framework independent: $uiDependency"
    }
}

$winFormsReferences = @(Get-ProjectReferences $winForms)
$wpfReferences = @(Get-ProjectReferences $wpf)
$avaloniaReferences = @(Get-ProjectReferences $avalonia)
Assert-Reference $winFormsReferences "..\OcctNet\OcctNet.csproj" "OcctNet.WinForms"
Assert-Reference $wpfReferences "..\OcctNet.WinForms\OcctNet.WinForms.csproj" "OcctNet.Wpf"
Assert-Reference $avaloniaReferences "..\OcctNet\OcctNet.csproj" "OcctNet.Avalonia"

if ((Get-ProjectProperty $winForms "UseWindowsForms") -ne "true") {
    throw "OcctNet.WinForms must enable Windows Forms."
}
if ((Get-ProjectProperty $wpf "UseWPF") -ne "true" -or (Get-ProjectProperty $wpf "UseWindowsForms") -ne "true") {
    throw "OcctNet.Wpf must enable WPF and Windows Forms hosting."
}
$avaloniaPackages = @(Get-PackageReferences $avalonia)
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

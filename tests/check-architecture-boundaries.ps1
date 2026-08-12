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

function Assert-NoUiSiblingReferences {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$References,
        [Parameter(Mandatory = $true)][string]$ProjectName
    )

    $siblings = @("OcctNet.WinForms", "OcctNet.Wpf", "OcctNet.Avalonia")
    foreach ($reference in $References) {
        foreach ($sibling in $siblings) {
            if ($reference -match "(?i)\\$([regex]::Escape($sibling))\\") {
                throw "$ProjectName must not depend on sibling UI host project $sibling."
            }
        }
    }
}

function Test-TrackedPath {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $normalized = $RelativePath.Replace('\', '/')
    $tracked = @(& git -C $RepositoryRoot ls-files -- $normalized "$normalized/**" 2>$null)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect tracked repository paths with git ls-files."
    }
    return $tracked.Count -gt 0
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
Assert-Reference $wpfReferences "..\OcctNet\OcctNet.csproj" "OcctNet.Wpf"
Assert-Reference $avaloniaReferences "..\OcctNet\OcctNet.csproj" "OcctNet.Avalonia"
Assert-NoUiSiblingReferences $winFormsReferences "OcctNet.WinForms"
Assert-NoUiSiblingReferences $wpfReferences "OcctNet.Wpf"
Assert-NoUiSiblingReferences $avaloniaReferences "OcctNet.Avalonia"

if ((Get-ProjectProperty $winForms "UseWindowsForms") -ne "true") {
    throw "OcctNet.WinForms must enable Windows Forms."
}
if ((Get-ProjectProperty $wpf "UseWPF") -ne "true") {
    throw "OcctNet.Wpf must enable WPF."
}
if (-not [string]::IsNullOrWhiteSpace((Get-ProjectProperty $wpf "UseWindowsForms"))) {
    throw "OcctNet.Wpf must remain independent from Windows Forms."
}
$avaloniaPackages = @(Get-PackageReferences $avalonia)
if ("Avalonia" -notin $avaloniaPackages) {
    throw "OcctNet.Avalonia must reference the Avalonia package."
}

$demoProjects = @(
    "src/OcctDemo.Common",
    "src/OcctDemo.WinForms",
    "src/OcctDemo.Wpf",
    "src/OcctDemo.Avalonia"
)
$trackedDemoProjects = @($demoProjects | Where-Object { Test-TrackedPath $_ })
if ($trackedDemoProjects.Count -ne 0 -and $trackedDemoProjects.Count -ne $demoProjects.Count) {
    throw "Demo projects must be either fully absent on main or fully present on demo."
}
$isDemoBranchLayout = $trackedDemoProjects.Count -eq $demoProjects.Count

foreach ($legacyProject in @(
    "src/CadCommon",
    "src/CadWinForms",
    "src/CadWpf",
    "src/CadAvalonia"
)) {
    if (Test-TrackedPath $legacyProject) {
        throw "Legacy application project must not be tracked: $legacyProject"
    }
}

if ($isDemoBranchLayout) {
    $demoCommon = Read-Project "src/OcctDemo.Common/OcctDemo.Common.csproj"
    $demoWinForms = Read-Project "src/OcctDemo.WinForms/OcctDemo.WinForms.csproj"
    $demoWpf = Read-Project "src/OcctDemo.Wpf/OcctDemo.Wpf.csproj"
    $demoAvalonia = Read-Project "src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"

    Assert-Reference @(Get-ProjectReferences $demoCommon) "..\OcctNet\OcctNet.csproj" "OcctDemo.Common"
    Assert-Reference @(Get-ProjectReferences $demoWinForms) "..\OcctDemo.Common\OcctDemo.Common.csproj" "OcctDemo.WinForms"
    Assert-Reference @(Get-ProjectReferences $demoWinForms) "..\OcctNet.WinForms\OcctNet.WinForms.csproj" "OcctDemo.WinForms"
    Assert-Reference @(Get-ProjectReferences $demoWpf) "..\OcctDemo.Common\OcctDemo.Common.csproj" "OcctDemo.Wpf"
    Assert-Reference @(Get-ProjectReferences $demoWpf) "..\OcctNet.Wpf\OcctNet.Wpf.csproj" "OcctDemo.Wpf"
    Assert-Reference @(Get-ProjectReferences $demoAvalonia) "..\OcctDemo.Common\OcctDemo.Common.csproj" "OcctDemo.Avalonia"
    Assert-Reference @(Get-ProjectReferences $demoAvalonia) "..\OcctNet.Avalonia\OcctNet.Avalonia.csproj" "OcctDemo.Avalonia"
}

$managedRoot = Join-Path $RepositoryRoot "src\OcctNet"
$managedText = (Get-ChildItem $managedRoot -Filter '*.cs' -File -Recurse |
    ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"

foreach ($forbidden in @(
    "DocumentManager",
    "CommandBus",
    "CommandRegistry",
    "ToolManager"
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
    if (Test-TrackedPath $legacyFile) {
        throw "Legacy/compatibility source must not be reintroduced: $legacyFile"
    }
}

$layoutName = if ($isDemoBranchLayout) { "demo" } else { "main" }
Write-Host "[architecture] Core/UI dependency direction, independent UI hosts, $layoutName branch layout, and no-compatibility boundary validated." -ForegroundColor Green

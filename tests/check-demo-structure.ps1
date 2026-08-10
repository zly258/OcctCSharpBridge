param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path $contractPath -PathType Leaf)) { throw "bridge-contract.json was not found." }
$contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$targetFramework = [string]$contract.dotnet.targetFramework
if ([string]::IsNullOrWhiteSpace($targetFramework)) { throw "Bridge target framework is missing." }

function Read-Project {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) { throw "Project file was not found: $RelativePath" }
    return [xml](Get-Content $path -Raw -Encoding UTF8)
}

function Get-Property {
    param([Parameter(Mandatory = $true)][xml]$Project, [Parameter(Mandatory = $true)][string]$Name)
    $node = $Project.SelectSingleNode("/Project/PropertyGroup/$Name[normalize-space(.) != '']")
    if ($null -eq $node) { return $null }
    return [string]$node.InnerText
}

function Get-References {
    param([Parameter(Mandatory = $true)][xml]$Project)
    return @($Project.SelectNodes('/Project/ItemGroup/ProjectReference') | ForEach-Object {
        ([string]$_.GetAttribute('Include')).Replace('/', '\')
    })
}

function Assert-Reference {
    param(
        [Parameter(Mandatory = $true)][string[]]$References,
        [Parameter(Mandatory = $true)][string]$Expected,
        [Parameter(Mandatory = $true)][string]$ProjectName
    )
    $normalized = $Expected.Replace('/', '\')
    if (@($References | Where-Object { $_ -eq $normalized }).Count -ne 1) {
        throw "$ProjectName must reference exactly once: $Expected"
    }
}

function Get-TrackedPaths {
    param([Parameter(Mandatory = $true)][string]$PathSpec)
    $items = @(& git -C $RepositoryRoot ls-files -- $PathSpec 2>$null)
    if ($LASTEXITCODE -ne 0) { throw "Unable to inspect tracked paths with git ls-files." }
    return $items
}

$sharedProjects = @(
    "src/OcctNet/OcctNet.csproj",
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj",
    "src/OcctNet.Wpf/OcctNet.Wpf.csproj",
    "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj"
)
$demoProjects = @(
    "src/OcctDemo.Common/OcctDemo.Common.csproj",
    "src/OcctDemo.WinForms/OcctDemo.WinForms.csproj",
    "src/OcctDemo.Wpf/OcctDemo.Wpf.csproj",
    "src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"
)

foreach ($relativePath in @($sharedProjects + $demoProjects)) {
    $project = Read-Project $relativePath
    if ((Get-Property $project "TargetFramework") -ne $targetFramework) {
        throw "$relativePath must target $targetFramework."
    }
    if ((Get-Property $project "IsPackable") -ne "false") {
        throw "$relativePath must remain non-packable on the demo branch."
    }
}

$common = Read-Project "src/OcctDemo.Common/OcctDemo.Common.csproj"
$commonRefs = @(Get-References $common)
Assert-Reference $commonRefs "..\OcctNet\OcctNet.csproj" "OcctDemo.Common"
if ($commonRefs.Count -ne 1) { throw "OcctDemo.Common must depend only on OcctNet." }

$winForms = Read-Project "src/OcctDemo.WinForms/OcctDemo.WinForms.csproj"
$winFormsRefs = @(Get-References $winForms)
foreach ($reference in @(
    "..\OcctDemo.Common\OcctDemo.Common.csproj",
    "..\OcctNet\OcctNet.csproj",
    "..\OcctNet.WinForms\OcctNet.WinForms.csproj"
)) { Assert-Reference $winFormsRefs $reference "OcctDemo.WinForms" }
if ((Get-Property $winForms "UseWindowsForms") -ne "true" -or (Get-Property $winForms "PlatformTarget") -ne "x64") {
    throw "OcctDemo.WinForms must remain a Windows Forms x64 application."
}
if ((Get-Property $winForms "AssemblyName") -ne "CAD-Winform") { throw "OcctDemo.WinForms assembly name changed unexpectedly." }

$wpf = Read-Project "src/OcctDemo.Wpf/OcctDemo.Wpf.csproj"
$wpfRefs = @(Get-References $wpf)
foreach ($reference in @(
    "..\OcctDemo.Common\OcctDemo.Common.csproj",
    "..\OcctNet\OcctNet.csproj",
    "..\OcctNet.Wpf\OcctNet.Wpf.csproj"
)) { Assert-Reference $wpfRefs $reference "OcctDemo.Wpf" }
if ((Get-Property $wpf "UseWPF") -ne "true" -or (Get-Property $wpf "PlatformTarget") -ne "x64") {
    throw "OcctDemo.Wpf must remain a WPF x64 application."
}
if ((Get-Property $wpf "AssemblyName") -ne "CAD-WPF") { throw "OcctDemo.Wpf assembly name changed unexpectedly." }

$avalonia = Read-Project "src/OcctDemo.Avalonia/OcctDemo.Avalonia.csproj"
$avaloniaRefs = @(Get-References $avalonia)
foreach ($reference in @(
    "..\OcctDemo.Common\OcctDemo.Common.csproj",
    "..\OcctNet\OcctNet.csproj",
    "..\OcctNet.Avalonia\OcctNet.Avalonia.csproj"
)) { Assert-Reference $avaloniaRefs $reference "OcctDemo.Avalonia" }
if ((Get-Property $avalonia "PlatformTarget") -ne "x64") {
    throw "OcctDemo.Avalonia must remain an x64 application."
}
if ((Get-Property $avalonia "AssemblyName") -ne "CAD-Avalonia") { throw "OcctDemo.Avalonia assembly name changed unexpectedly." }

foreach ($relativePath in @("run.ps1", "publish.ps1", "docs/README.md")) {
    if (-not (Test-Path (Join-Path $RepositoryRoot $relativePath) -PathType Leaf)) {
        throw "Required demo maintenance entry point is missing: $relativePath"
    }
}

$trackedDocs = @(Get-TrackedPaths "docs")
$unexpectedDocs = @($trackedDocs | Where-Object { $_ -ne "docs/README.md" })
if ($unexpectedDocs.Count -gt 0) {
    throw "Demo must not duplicate main SDK documentation: $($unexpectedDocs -join ', ')"
}

$workflowFiles = @(Get-TrackedPaths ".github/workflows")
if ($workflowFiles.Count -gt 0) {
    throw "GitHub Actions workflows are not used by this repository: $($workflowFiles -join ', ')"
}

foreach ($legacyPath in @(
    "src/CadCommon",
    "src/CadWinForms",
    "src/CadWpf",
    "src/CadAvalonia",
    "src/OcctNet/OcctObject.Legacy.cs",
    "src/OcctNet/OcctGeometryExtensions.Compatibility.cs",
    "src/OcctNet/OcctEngine.ApiAliases.cs",
    "src/OcctNet/NativeMethods.Modeling.cs",
    "src/OcctNative/OcctModelingInternal.hxx"
)) {
    $tracked = @(Get-TrackedPaths $legacyPath)
    if ($tracked.Count -gt 0) { throw "Legacy/compatibility path must not be reintroduced: $legacyPath" }
}

Write-Host "[demo-structure] Demo projects, branch boundaries, local tooling, and no-compatibility structure validated." -ForegroundColor Green

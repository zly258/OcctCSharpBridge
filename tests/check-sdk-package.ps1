param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path $contractPath -PathType Leaf)) {
    throw "Bridge contract file is missing: bridge-contract.json"
}
$contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$targetFramework = [string]$contract.dotnet.targetFramework
if ([string]::IsNullOrWhiteSpace($targetFramework)) {
    throw "Bridge contract target framework is missing."
}

function Read-Project {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Managed package project is missing: $RelativePath"
    }
    return [xml](Get-Content $path -Raw -Encoding UTF8)
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

$projects = [ordered]@{
    "OcctNet" = "src/OcctNet/OcctNet.csproj"
    "OcctNet.WinForms" = "src/OcctNet.WinForms/OcctNet.WinForms.csproj"
    "OcctNet.Wpf" = "src/OcctNet.Wpf/OcctNet.Wpf.csproj"
    "OcctNet.Avalonia" = "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj"
}

foreach ($entry in $projects.GetEnumerator()) {
    $project = Read-Project $entry.Value

    $actualTargetFramework = Get-ProjectProperty $project "TargetFramework"
    $platformTarget = Get-ProjectProperty $project "PlatformTarget"
    $isPackable = Get-ProjectProperty $project "IsPackable"
    $generateDocumentation = Get-ProjectProperty $project "GenerateDocumentationFile"
    $packageReadme = Get-ProjectProperty $project "PackageReadmeFile"
    $packageLicense = Get-ProjectProperty $project "PackageLicenseFile"
    $repositoryUrl = Get-ProjectProperty $project "RepositoryUrl"
    $includeSymbols = Get-ProjectProperty $project "IncludeSymbols"
    $symbolPackageFormat = Get-ProjectProperty $project "SymbolPackageFormat"

    if ($actualTargetFramework -ne $targetFramework) {
        throw "$($entry.Key) target framework is '$actualTargetFramework'; expected '$targetFramework'."
    }
    if ($platformTarget -ne "x64") {
        throw "$($entry.Key) PlatformTarget is '$platformTarget'; expected 'x64'."
    }
    if ($isPackable -ne "true") {
        throw "$($entry.Key) must remain packable."
    }
    if ($generateDocumentation -ne "true") {
        throw "$($entry.Key) must generate XML documentation."
    }
    if ($packageReadme -ne "README.md") {
        throw "$($entry.Key) PackageReadmeFile must be README.md."
    }
    if ($packageLicense -ne "LICENSE") {
        throw "$($entry.Key) PackageLicenseFile must be LICENSE."
    }
    if ($repositoryUrl -ne "https://github.com/zly258/OcctCSharpBridge") {
        throw "$($entry.Key) RepositoryUrl is incorrect."
    }
    if ($includeSymbols -ne "true" -or $symbolPackageFormat -ne "snupkg") {
        throw "$($entry.Key) symbol package metadata is incomplete."
    }
}

foreach ($requiredFile in @("README.md", "LICENSE")) {
    if (-not (Test-Path (Join-Path $RepositoryRoot $requiredFile) -PathType Leaf)) {
        throw "Package content file is missing: $requiredFile"
    }
}

Write-Host "[package] Four reusable $targetFramework managed SDK projects have consistent package metadata; package contents are validated after dotnet pack." -ForegroundColor Green

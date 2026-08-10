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
    [xml](Get-Content $path -Raw -Encoding UTF8)
}

function Get-PropertyValue {
    param(
        [Parameter(Mandatory = $true)][xml]$Project,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $values = @($Project.Project.PropertyGroup | ForEach-Object {
        $node = $_.$Name
        if ($node -ne $null) { [string]$node }
    } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })

    return $values | Select-Object -First 1
}

$projects = [ordered]@{
    "OcctNet" = "src/OcctNet/OcctNet.csproj"
    "OcctNet.WinForms" = "src/OcctNet.WinForms/OcctNet.WinForms.csproj"
    "OcctNet.Wpf" = "src/OcctNet.Wpf/OcctNet.Wpf.csproj"
    "OcctNet.Avalonia" = "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj"
}

foreach ($entry in $projects.GetEnumerator()) {
    $project = Read-Project $entry.Value

    $actualTargetFramework = Get-PropertyValue $project "TargetFramework"
    $platformTarget = Get-PropertyValue $project "PlatformTarget"
    $isPackable = Get-PropertyValue $project "IsPackable"
    $generateDocumentation = Get-PropertyValue $project "GenerateDocumentationFile"
    $packageReadme = Get-PropertyValue $project "PackageReadmeFile"
    $packageLicense = Get-PropertyValue $project "PackageLicenseFile"
    $repositoryUrl = Get-PropertyValue $project "RepositoryUrl"
    $includeSymbols = Get-PropertyValue $project "IncludeSymbols"
    $symbolPackageFormat = Get-PropertyValue $project "SymbolPackageFormat"

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

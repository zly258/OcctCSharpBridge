param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projects = @(
    "src/OcctNet/OcctNet.csproj",
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj",
    "src/OcctNet.Wpf/OcctNet.Wpf.csproj"
)

foreach ($relativePath in $projects) {
    $path = Join-Path $RepositoryRoot $relativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Managed package project is missing: $relativePath"
    }

    $text = [System.IO.File]::ReadAllText($path)
    foreach ($token in @(
        "<IsPackable>true</IsPackable>",
        "<GenerateDocumentationFile>true</GenerateDocumentationFile>",
        "<PackageReadmeFile>README.md</PackageReadmeFile>",
        "<PackageLicenseFile>LICENSE</PackageLicenseFile>",
        "<RepositoryUrl>https://github.com/zly258/OcctCSharpBridge</RepositoryUrl>",
        "<IncludeSymbols>true</IncludeSymbols>",
        "<SymbolPackageFormat>snupkg</SymbolPackageFormat>"
    )) {
        if (-not $text.Contains($token)) {
            throw "Managed SDK package metadata is missing from ${relativePath}: $token"
        }
    }
}

foreach ($relativePath in @(
    "docs/GETTING_STARTED.md",
    "docs/GETTING_STARTED.zh-CN.md",
    "docs/PACKAGING.md",
    "docs/PACKAGING.zh-CN.md"
)) {
    if (-not (Test-Path (Join-Path $RepositoryRoot $relativePath) -PathType Leaf)) {
        throw "Main SDK documentation is missing: $relativePath"
    }
}

$buildScript = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "build.ps1"))
foreach ($token in @(
    'ValidateSet("validate", "native", "managed", "pack", "smoke", "ci", "all")',
    '"pack" { Pack-ManagedSdk }',
    'Pack-ManagedSdk -SkipBuild',
    'artifacts\packages'
)) {
    if (-not $buildScript.Contains($token)) {
        throw "Main build script is missing SDK packaging contract: $token"
    }
}

Write-Host "[package] Main-branch managed SDK packaging metadata, documentation and pack target validated." -ForegroundColor Green

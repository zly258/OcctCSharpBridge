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
$UseSelfContained = -not $FrameworkDependent.IsPresent
if ($SelfContained.IsPresent) { $UseSelfContained = $true }

$RepoRoot = Split-Path -Parent $PSCommandPath
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$ManifestPath = Join-Path $DistRoot "bridge-manifest.json"
$NativeDll = Join-Path $DistRoot "OcctNative.dll"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"

if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $RepoRoot "artifacts\publish"
}
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)

$Projects = [ordered]@{
    winform = @{
        Name = "WinForms"
        Project = "src\OcctDemo.WinForms\OcctDemo.WinForms.csproj"
        Executable = "CAD-Winform.exe"
    }
    wpf = @{
        Name = "WPF"
        Project = "src\OcctDemo.Wpf\OcctDemo.Wpf.csproj"
        Executable = "CAD-WPF.exe"
    }
    avalonia = @{
        Name = "Avalonia"
        Project = "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"
        Executable = "CAD-Avalonia.exe"
    }
}

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required path was not found: $Path" }
}

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    if ($null -eq (Get-Command -Name $Name -ErrorAction SilentlyContinue)) {
        throw "$Name was not found in PATH."
    }
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) { throw $ErrorMessage }
}

function Get-SelectedKeys {
    if ($Target -eq "all") { return @("winform", "wpf", "avalonia") }
    return @($Target)
}

function Copy-RuntimeDlls {
    param([Parameter(Mandatory = $true)][string]$Destination)

    Copy-Item -LiteralPath $NativeDll -Destination (Join-Path $Destination "OcctNative.dll") -Force

    Get-ChildItem -LiteralPath $OcctBinDir -Filter "*.dll" -File | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $Destination $_.Name) -Force
    }

    if (Test-Path -LiteralPath $OcctThirdPartyDir -PathType Container) {
        Get-ChildItem -LiteralPath $OcctThirdPartyDir -Filter "*.dll" -File -Recurse | Where-Object {
            $_.DirectoryName -match '[\\/]bin([\\/]|$)'
        } | ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $Destination $_.Name) -Force
        }
    }

    Copy-Item -LiteralPath $ContractPath -Destination (Join-Path $Destination "bridge-contract.json") -Force
    Copy-Item -LiteralPath $ManifestPath -Destination (Join-Path $Destination "bridge-manifest.json") -Force
}

Assert-Command "dotnet"
Assert-Path $BuildScript
Assert-Path $ContractPath
Assert-Path $ManifestPath
Assert-Path $NativeDll
Assert-Path $OcctBinDir

& $BuildScript validate $Configuration

$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$packageRoot = Join-Path $OutputDirectory ("OcctCSharpBridge-Demo-{0}-win-x64" -f $Target)

if ((Test-Path -LiteralPath $packageRoot) -and -not $KeepExisting.IsPresent) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

foreach ($key in Get-SelectedKeys) {
    $definition = $Projects[$key]
    $projectPath = Join-Path $RepoRoot $definition.Project
    $destination = Join-Path $packageRoot $key
    Assert-Path $projectPath

    if ((Test-Path -LiteralPath $destination) -and -not $KeepExisting.IsPresent) {
        Remove-Item -LiteralPath $destination -Recurse -Force
    }
    New-Item -ItemType Directory -Path $destination -Force | Out-Null

    Write-Host "[publish] $($definition.Name) from Bridge $($contract.bridgeVersion), ABI $($contract.nativeAbiVersion)..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "publish", $projectPath,
        "-c", $Configuration,
        "-r", "win-x64",
        "-p:Platform=x64",
        "--self-contained", $UseSelfContained.ToString().ToLowerInvariant(),
        "--nologo",
        "-o", $destination
    ) "$($definition.Name) publish failed."

    Assert-Path (Join-Path $destination $definition.Executable)
    Copy-RuntimeDlls -Destination $destination
}

if ($Zip.IsPresent) {
    $zipPath = "$packageRoot.zip"
    Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
    Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "Package: $zipPath" -ForegroundColor Green
}
else {
    Write-Host "Package: $packageRoot" -ForegroundColor Green
}

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

# Direct OcctNative toolkits plus the OCCT 7.9 runtime closure required by
# TKV3d and the STEP/IGES/STL exchange toolkits. CAF/XCAF modules here are
# internal OCCT runtime dependencies of exchange support; the Bridge still does
# not expose or use OCAF/XDE as its document architecture.
$RequiredOcctRuntimeModules = @(
    "TKernel", "TKMath", "TKG2d", "TKG3d", "TKGeomBase", "TKBRep",
    "TKGeomAlgo", "TKTopAlgo", "TKPrim", "TKBO", "TKBool", "TKFillet",
    "TKOffset", "TKMesh", "TKShHealing", "TKService", "TKV3d", "TKOpenGl",
    "TKHLR", "TKXSBase", "TKDE", "TKDESTEP", "TKDEIGES", "TKDESTL",
    "TKCDF", "TKLCAF", "TKCAF", "TKVCAF", "TKXCAF"
)

# Native third-party runtime candidates used by the selected OCCT toolkits.
# Qt, VTK, Tcl/Tk, Draw/Test tooling and debug binaries are intentionally not
# copied. FFmpeg/OpenVR are retained only when present because TKService may be
# built with those optional features in the installed OCCT package.
$ThirdPartyRuntimeCandidates = @(
    "tbb12.dll",
    "tbbmalloc.dll",
    "tbbmalloc_proxy.dll",
    "freetype.dll",
    "FreeImage.dll",
    "FreeImagePlus.dll",
    "avcodec-57.dll",
    "avdevice-57.dll",
    "avfilter-6.dll",
    "avformat-57.dll",
    "avutil-55.dll",
    "swscale-4.dll",
    "zlib.dll",
    "zlib1.dll",
    "liblzma.dll",
    "openvr_api.dll"
)

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

function Copy-MergedFile {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination
    )

    $destinationDirectory = Split-Path -Parent $Destination
    if (-not (Test-Path -LiteralPath $destinationDirectory -PathType Container)) {
        New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
    }

    if (Test-Path -LiteralPath $Destination -PathType Leaf) {
        $sourceHash = (Get-FileHash -LiteralPath $Source -Algorithm SHA256).Hash
        $destinationHash = (Get-FileHash -LiteralPath $Destination -Algorithm SHA256).Hash
        if ($sourceHash -eq $destinationHash) { return }
        throw "Publish output collision contains different files: $Destination"
    }

    Copy-Item -LiteralPath $Source -Destination $Destination -Force
}

function Merge-PublishTree {
    param(
        [Parameter(Mandatory = $true)][string]$SourceRoot,
        [Parameter(Mandatory = $true)][string]$DestinationRoot
    )

    $normalizedSourceRoot = [System.IO.Path]::GetFullPath($SourceRoot).TrimEnd('\', '/')
    Get-ChildItem -LiteralPath $normalizedSourceRoot -File -Recurse | ForEach-Object {
        $relativePath = $_.FullName.Substring($normalizedSourceRoot.Length).TrimStart('\', '/')
        Copy-MergedFile -Source $_.FullName -Destination (Join-Path $DestinationRoot $relativePath)
    }
}

function Copy-OcctRuntime {
    param([Parameter(Mandatory = $true)][string]$Destination)

    Copy-Item -LiteralPath $NativeDll -Destination (Join-Path $Destination "OcctNative.dll") -Force

    foreach ($module in $RequiredOcctRuntimeModules) {
        $source = Join-Path $OcctBinDir "$module.dll"
        Assert-Path $source
        Copy-Item -LiteralPath $source -Destination (Join-Path $Destination "$module.dll") -Force
    }

    if (Test-Path -LiteralPath $OcctThirdPartyDir -PathType Container) {
        foreach ($fileName in $ThirdPartyRuntimeCandidates) {
            $matches = @(Get-ChildItem -LiteralPath $OcctThirdPartyDir -Filter $fileName -File -Recurse -ErrorAction SilentlyContinue)
            if ($matches.Count -gt 0) {
                Copy-Item -LiteralPath $matches[0].FullName -Destination (Join-Path $Destination $fileName) -Force
            }
        }
    }

    Copy-Item -LiteralPath $ContractPath -Destination (Join-Path $Destination "bridge-contract.json") -Force
    Copy-Item -LiteralPath $ManifestPath -Destination (Join-Path $Destination "bridge-manifest.json") -Force
}

Assert-Command "dotnet"
Assert-Path $BuildScript

# Validate the Binary SDK once before touching publish output.
& $BuildScript validate $Configuration

Assert-Path $OcctBinDir
$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$packageRoot = Join-Path $OutputDirectory ("OcctCSharpBridge-Demo-{0}-win-x64" -f $Target)
$stagingRoot = Join-Path $OutputDirectory (".OcctCSharpBridge-Demo-{0}-staging-{1}" -f $Target, $PID)

if ((Test-Path -LiteralPath $packageRoot) -and -not $KeepExisting.IsPresent) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null
New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null

try {
    foreach ($key in Get-SelectedKeys) {
        $definition = $Projects[$key]
        $projectPath = Join-Path $RepoRoot $definition.Project
        $stagingDestination = Join-Path $stagingRoot $key
        Assert-Path $projectPath
        New-Item -ItemType Directory -Path $stagingDestination -Force | Out-Null

        Write-Host "[publish] $($definition.Name) from Bridge $($contract.bridgeVersion), ABI $($contract.nativeAbiVersion)..." -ForegroundColor Cyan
        Invoke-Checked "dotnet" @(
            "publish", $projectPath,
            "-c", $Configuration,
            "-r", "win-x64",
            "-p:Platform=x64",
            "-p:Version=$($contract.bridgeVersion)",
            "-p:DebugType=None",
            "-p:DebugSymbols=false",
            "--self-contained", $UseSelfContained.ToString().ToLowerInvariant(),
            "--nologo",
            "-o", $stagingDestination
        ) "$($definition.Name) publish failed."

        Assert-Path (Join-Path $stagingDestination $definition.Executable)
        Merge-PublishTree -SourceRoot $stagingDestination -DestinationRoot $packageRoot
    }

    # Native runtime is shared by all three applications and copied only once.
    Copy-OcctRuntime -Destination $packageRoot

    foreach ($key in Get-SelectedKeys) {
        Assert-Path (Join-Path $packageRoot $Projects[$key].Executable)
    }
}
finally {
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
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

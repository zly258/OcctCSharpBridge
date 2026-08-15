param(
    [string]$Remote = "origin",
    [string]$SourceBranch = "main",
    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$SdkRoot = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$Destination = Join-Path $RepoRoot "dist\win-x64"
$TempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("OcctCSharpBridge-main-sdk-" + [Guid]::NewGuid().ToString("N"))
$WorktreeAdded = $false

function Assert-File {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Required SDK file was not found: $Path" }
}

function Test-SdkRoot {
    param([Parameter(Mandatory = $true)][string]$Root)
    foreach ($name in @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "OcctNet.Avalonia.dll",
        "bridge-contract.json",
        "bridge-manifest.json")) {
        Assert-File (Join-Path $Root $name)
    }
    $contract = Get-Content -LiteralPath (Join-Path $Root "bridge-contract.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([int]$contract.schemaVersion -ne 3 -or
        [int]$contract.nativeAbi.current -ne 5 -or
        [int]$contract.nativeAbi.minimumSupported -ne 5 -or
        [string]$contract.api.policy -ne "abi5-only" -or
        [string]$contract.platform -ne "win-x64" -or
        [string]$contract.dotnet.sdkVersion -ne "10.0.303") {
        throw "The source SDK is not the expected Bridge 3 ABI5-only win-x64 SDK."
    }
    return $contract
}

function Copy-Sdk {
    param([Parameter(Mandatory = $true)][string]$Source)
    $contract = Test-SdkRoot $Source
    if (Test-Path -LiteralPath $Destination -PathType Container) { Remove-Item -LiteralPath $Destination -Recurse -Force }
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    Copy-Item -Path (Join-Path $Source "*") -Destination $Destination -Recurse -Force
    Write-Host "Binary SDK synchronized." -ForegroundColor Green
    Write-Host "Bridge: $($contract.bridgeVersion), ABI 5 only, OCCT $($contract.occtVersion), .NET SDK $($contract.dotnet.sdkVersion)" -ForegroundColor DarkGray
    Write-Host "Path:   $Destination" -ForegroundColor DarkGray
}

if (-not [string]::IsNullOrWhiteSpace($SdkRoot)) {
    Copy-Sdk ([System.IO.Path]::GetFullPath($SdkRoot))
    exit 0
}

if ($null -eq (Get-Command git -ErrorAction SilentlyContinue)) { throw "git was not found in PATH." }

try {
    Write-Host "[sync] Fetching $Remote/$SourceBranch..." -ForegroundColor Cyan
    & git -C $RepoRoot fetch --quiet $Remote $SourceBranch
    if ($LASTEXITCODE -ne 0) { throw "Unable to fetch $Remote/$SourceBranch." }

    Write-Host "[sync] Creating temporary clean SDK worktree..." -ForegroundColor DarkGray
    & git -C $RepoRoot worktree add --detach $TempRoot "$Remote/$SourceBranch"
    if ($LASTEXITCODE -ne 0) { throw "Unable to create worktree for $Remote/$SourceBranch." }
    $WorktreeAdded = $true

    $buildScript = Join-Path $TempRoot "build.ps1"
    if (-not (Test-Path -LiteralPath $buildScript -PathType Leaf)) { throw "$Remote/$SourceBranch does not contain build.ps1." }

    Write-Host "[sync] Building validated win-x64 Binary SDK from $Remote/$SourceBranch..." -ForegroundColor Cyan
    $buildArgs = @("dist", "Release")
    if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) { $buildArgs += @("-OcctRoot", $OcctRoot) }
    & $buildScript @buildArgs
    if ($LASTEXITCODE -ne 0) { throw "Binary SDK build failed on $Remote/$SourceBranch." }

    Copy-Sdk (Join-Path $TempRoot "dist\win-x64")
}
finally {
    if ($WorktreeAdded) {
        & git -C $RepoRoot worktree remove --force $TempRoot *> $null
    }
    elseif (Test-Path -LiteralPath $TempRoot) {
        Remove-Item -LiteralPath $TempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

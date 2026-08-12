param(
    [string]$Remote = "origin",
    [string]$SourceBranch = "main"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$LocalSdkRoot = Join-Path $RepoRoot "dist\win-x64"
$TempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("OcctCSharpBridge-sdk-" + [Guid]::NewGuid().ToString("N"))
$WorktreeAdded = $false

function Invoke-Git {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    & git -C $RepoRoot @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

if ($null -eq (Get-Command git -ErrorAction SilentlyContinue)) {
    throw "git was not found in PATH."
}

try {
    Write-Host "[sync] Fetching $Remote/$SourceBranch..." -ForegroundColor Cyan
    Invoke-Git @("fetch", "--quiet", $Remote, $SourceBranch)

    $SourceRef = "$Remote/$SourceBranch"
    Write-Host "[sync] Opening temporary worktree at $SourceRef..." -ForegroundColor DarkGray
    Invoke-Git @("worktree", "add", "--detach", $TempRoot, $SourceRef)
    $WorktreeAdded = $true

    $SourceSdkRoot = Join-Path $TempRoot "dist\win-x64"
    $RequiredFiles = @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "bridge-contract.json",
        "bridge-manifest.json"
    )

    foreach ($Required in $RequiredFiles) {
        if (-not (Test-Path (Join-Path $SourceSdkRoot $Required) -PathType Leaf)) {
            throw ("Published Binary SDK is incomplete on {0}: missing {1}" -f $SourceRef, $Required)
        }
    }

    if (Test-Path $LocalSdkRoot) {
        Remove-Item $LocalSdkRoot -Recurse -Force
    }
    New-Item $LocalSdkRoot -ItemType Directory -Force | Out-Null
    Copy-Item (Join-Path $SourceSdkRoot "*") $LocalSdkRoot -Recurse -Force

    $Contract = Get-Content (Join-Path $LocalSdkRoot "bridge-contract.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    Write-Host "Binary SDK synchronized." -ForegroundColor Green
    Write-Host ("Bridge: {0}, ABI {1}, OCCT {2}, .NET SDK {3}" -f $Contract.bridgeVersion, $Contract.nativeAbiVersion, $Contract.occtVersion, $Contract.dotnet.sdkVersion) -ForegroundColor DarkGray
    Write-Host "Path:   $LocalSdkRoot" -ForegroundColor DarkGray
}
finally {
    if ($WorktreeAdded) {
        & git -C $RepoRoot worktree remove --force $TempRoot *> $null
    }
    elseif (Test-Path $TempRoot) {
        Remove-Item $TempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
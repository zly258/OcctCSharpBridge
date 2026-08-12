param(
    [string]$Remote = "origin",
    [string]$MainBranch = "main",
    [switch]$Commit,
    [switch]$Push
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$BuildScript = Join-Path $RepoRoot "build.ps1"
$TempWorktree = Join-Path ([System.IO.Path]::GetTempPath()) ("OcctCSharpBridge-main-sync-" + $PID)

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    if ($null -eq (Get-Command -Name $Name -ErrorAction SilentlyContinue)) {
        throw "$Name was not found in PATH."
    }
}

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [Parameter(Mandatory = $true)][string[]]$Arguments
    )
    & git -C $WorkingDirectory @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Get-CurrentBranch {
    $branch = @(& git -C $RepoRoot rev-parse --abbrev-ref HEAD 2>$null)
    if ($LASTEXITCODE -ne 0 -or $branch.Count -ne 1 -or [string]$branch[0] -eq "HEAD") {
        throw "sync.ps1 must be run from a named branch."
    }
    return ([string]$branch[0]).Trim()
}

function Test-BinarySdk {
    param([Parameter(Mandatory = $true)][string]$Path)

    $requiredFiles = @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "OcctNet.Avalonia.dll",
        "bridge-contract.json",
        "bridge-manifest.json"
    )
    foreach ($name in $requiredFiles) {
        $file = Join-Path $Path $name
        if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
            throw "Published Bridge Binary SDK is incomplete: $name"
        }
    }

    $contract = Get-Content -LiteralPath (Join-Path $Path "bridge-contract.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $manifest = Get-Content -LiteralPath (Join-Path $Path "bridge-manifest.json") -Raw -Encoding UTF8 | ConvertFrom-Json

    if ([int]$manifest.schemaVersion -ne 1 -or
        [string]$manifest.author -ne [string]$contract.author -or
        [string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [int]$manifest.nativeAbiVersion -ne [int]$contract.nativeAbiVersion -or
        [string]$manifest.occtVersion -ne [string]$contract.occtVersion -or
        [string]$manifest.configuration -ne "Release") {
        throw "Published Bridge manifest does not match bridge-contract.json or is not a Release SDK."
    }

    if ([string]::IsNullOrWhiteSpace([string]$manifest.sourceCommit)) {
        throw "Published Bridge manifest sourceCommit is missing."
    }

    foreach ($entry in @($manifest.files)) {
        $name = [string]$entry.name
        if ([string]::IsNullOrWhiteSpace($name) -or $name.Contains('/') -or $name.Contains('\')) {
            throw "Invalid Bridge manifest file name: $name"
        }
        $file = Join-Path $Path $name
        if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
            throw "Bridge manifest file is missing: $name"
        }
        $actual = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne ([string]$entry.sha256).ToLowerInvariant()) {
            throw "Bridge Binary SDK hash mismatch: $name"
        }
    }

    return $manifest
}

Assert-Command "git"
$currentBranch = Get-CurrentBranch
if ($currentBranch -ne "demo") {
    throw "sync.ps1 must be run from the demo branch. Current branch: $currentBranch"
}
if ($Push.IsPresent -and -not $Commit.IsPresent) {
    throw "-Push requires -Commit."
}

$changesOutsideDist = @(& git -C $RepoRoot status --porcelain --untracked-files=all | Where-Object {
    $line = [string]$_
    $path = if ($line.Length -gt 3) { $line.Substring(3).Trim('"') } else { "" }
    $path -notlike "dist/win-x64/*" -and $path -notlike "dist\win-x64\*"
})
if ($LASTEXITCODE -ne 0) { throw "Failed to inspect the demo working tree." }
if ($changesOutsideDist.Count -gt 0) {
    throw "The demo working tree has changes outside dist/win-x64. Commit or stash them before syncing the Bridge SDK."
}

Write-Host "[sync] Fetching $Remote/$MainBranch..." -ForegroundColor Cyan
Invoke-Git $RepoRoot @("fetch", $Remote, $MainBranch)

Remove-Item -LiteralPath $TempWorktree -Recurse -Force -ErrorAction SilentlyContinue
try {
    Invoke-Git $RepoRoot @("worktree", "add", "--detach", $TempWorktree, "$Remote/$MainBranch")
    $sourceDist = Join-Path $TempWorktree "dist\win-x64"
    $manifest = Test-BinarySdk -Path $sourceDist

    Remove-Item -LiteralPath $DistRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $DistRoot -Force | Out-Null
    Copy-Item -Path (Join-Path $sourceDist "*") -Destination $DistRoot -Recurse -Force

    $syncedManifest = Test-BinarySdk -Path $DistRoot
    if ([string]$syncedManifest.sourceCommit -ne [string]$manifest.sourceCommit) {
        throw "Synced Bridge manifest does not match the published main SDK."
    }
}
finally {
    & git -C $RepoRoot worktree remove --force $TempWorktree 2>$null
    Remove-Item -LiteralPath $TempWorktree -Recurse -Force -ErrorAction SilentlyContinue
    & git -C $RepoRoot worktree prune 2>$null
}

if (Test-Path -LiteralPath $BuildScript -PathType Leaf) {
    & $BuildScript validate Release
    if ($LASTEXITCODE -ne 0) { throw "Demo validation failed after Bridge SDK synchronization." }
}

$manifestPath = Join-Path $DistRoot "bridge-manifest.json"
$finalManifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
Write-Host "[sync] Bridge $($finalManifest.bridgeVersion), ABI $($finalManifest.nativeAbiVersion), source $($finalManifest.sourceCommit)" -ForegroundColor Green

if ($Commit.IsPresent) {
    Invoke-Git $RepoRoot @("add", "--", "dist/win-x64")
    $staged = @(& git -C $RepoRoot diff --cached --name-only -- "dist/win-x64")
    if ($LASTEXITCODE -ne 0) { throw "Failed to inspect staged Binary SDK changes." }
    if ($staged.Count -gt 0) {
        Invoke-Git $RepoRoot @("commit", "-m", "Sync Bridge Binary SDK from main")
    }
    else {
        Write-Host "[sync] demo/dist/win-x64 is already current." -ForegroundColor DarkGray
    }
}

if ($Push.IsPresent) {
    Invoke-Git $RepoRoot @("push", $Remote, "demo")
}

Write-Host "Bridge Binary SDK synchronized. Demo publishing remains a separate step." -ForegroundColor Green

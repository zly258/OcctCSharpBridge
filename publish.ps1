param(
    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$Remote = "origin",
    [string]$DemoBranch = "demo",
    [switch]$NoPush
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$DemoWorktree = Join-Path ([System.IO.Path]::GetTempPath()) ("OcctCSharpBridge-demo-publish-" + $PID)

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

function Test-BinarySdk {
    param([Parameter(Mandatory = $true)][string]$Path)

    $contractPath = Join-Path $Path "bridge-contract.json"
    $manifestPath = Join-Path $Path "bridge-manifest.json"
    foreach ($required in @(
        "OcctNative.dll", "OcctNet.dll", "OcctNet.WinForms.dll", "OcctNet.Wpf.dll", "OcctNet.Avalonia.dll",
        "bridge-contract.json", "bridge-manifest.json")) {
        if (-not (Test-Path (Join-Path $Path $required) -PathType Leaf)) {
            throw "Binary SDK file is missing: $required"
        }
    }

    $contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $manifest = Get-Content $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([int]$manifest.schemaVersion -ne 1 -or
        [string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [int]$manifest.nativeAbiVersion -ne [int]$contract.nativeAbiVersion -or
        [string]$manifest.platform -ne [string]$contract.platform -or
        [string]$manifest.targetFramework -ne [string]$contract.dotnet.targetFramework) {
        throw "Binary SDK manifest does not match bridge-contract.json."
    }

    foreach ($entry in @($manifest.files)) {
        $file = Join-Path $Path ([string]$entry.name)
        if (-not (Test-Path $file -PathType Leaf)) { throw "Manifest file is missing: $($entry.name)" }
        $hash = (Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($hash -ne ([string]$entry.sha256).ToLowerInvariant()) {
            throw "Binary SDK hash mismatch: $($entry.name)"
        }
    }
}

Assert-Command "git"
if (-not (Test-Path $BuildScript -PathType Leaf)) { throw "build.ps1 was not found." }

$currentBranch = (& git -C $RepoRoot branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $currentBranch -ne "main") {
    throw "publish.ps1 must be run from the main branch."
}

$initialChanges = @(& git -C $RepoRoot status --porcelain --untracked-files=all)
if ($LASTEXITCODE -ne 0) { throw "Failed to inspect the main working tree." }
if ($initialChanges.Count -gt 0) {
    throw "The main working tree must be clean before publishing."
}

Write-Host "[publish] Building and validating the Release Binary SDK..." -ForegroundColor Cyan
$buildArguments = @("dist", "Release")
if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) { $buildArguments += @("-OcctRoot", $OcctRoot) }
& $BuildScript @buildArguments
if ($LASTEXITCODE -ne 0) { throw "Binary SDK build failed." }
Test-BinarySdk -Path $DistRoot

Invoke-Git $RepoRoot @("add", "--", "dist/win-x64")
$staged = @(& git -C $RepoRoot diff --cached --name-only -- "dist/win-x64")
if ($LASTEXITCODE -ne 0) { throw "Failed to inspect staged Binary SDK changes." }
if ($staged.Count -gt 0) {
    Invoke-Git $RepoRoot @("commit", "-m", "Publish Bridge Binary SDK")
}
else {
    Write-Host "[publish] main/dist/win-x64 is already current." -ForegroundColor DarkGray
}

if (-not $NoPush) {
    Invoke-Git $RepoRoot @("push", $Remote, "main")
}

Remove-Item $DemoWorktree -Recurse -Force -ErrorAction SilentlyContinue
try {
    Invoke-Git $RepoRoot @("fetch", $Remote, $DemoBranch)
    Invoke-Git $RepoRoot @("worktree", "add", "--detach", $DemoWorktree, "$Remote/$DemoBranch")

    $demoDist = Join-Path $DemoWorktree "dist\win-x64"
    Remove-Item $demoDist -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $demoDist -Force | Out-Null
    Copy-Item (Join-Path $DistRoot "*") $demoDist -Recurse -Force
    Test-BinarySdk -Path $demoDist

    Invoke-Git $DemoWorktree @("add", "--", "dist/win-x64")
    $demoChanges = @(& git -C $DemoWorktree diff --cached --name-only -- "dist/win-x64")
    if ($LASTEXITCODE -ne 0) { throw "Failed to inspect staged demo Binary SDK changes." }
    if ($demoChanges.Count -gt 0) {
        Invoke-Git $DemoWorktree @("commit", "-m", "Sync Bridge Binary SDK from main")
        if (-not $NoPush) {
            Invoke-Git $DemoWorktree @("push", $Remote, "HEAD:$DemoBranch")
        }
    }
    else {
        Write-Host "[publish] demo/dist/win-x64 is already current." -ForegroundColor DarkGray
    }
}
finally {
    & git -C $RepoRoot worktree remove --force $DemoWorktree 2>$null
    Remove-Item $DemoWorktree -Recurse -Force -ErrorAction SilentlyContinue
    & git -C $RepoRoot worktree prune 2>$null
}

if ($NoPush) {
    Write-Host "Binary SDK prepared locally. -NoPush prevented remote updates." -ForegroundColor Green
}
else {
    Write-Host "Binary SDK published to main and synchronized to demo." -ForegroundColor Green
}

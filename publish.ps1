param(
    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$Remote = "origin",
    [switch]$Fast
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    $OcctRoot = $DefaultOcctRoot
}

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
    param([Parameter(Mandatory = $true)][string]$WorkingDirectory)

    $output = @(& git -C $WorkingDirectory rev-parse --abbrev-ref HEAD 2>$null)
    if ($LASTEXITCODE -ne 0 -or $output.Count -ne 1) {
        throw "Failed to resolve the current Git branch."
    }

    $branch = [string]$output[0]
    if ([string]::IsNullOrWhiteSpace($branch) -or $branch -eq "HEAD") {
        throw "publish.ps1 must be run from a named branch, not detached HEAD."
    }
    return $branch.Trim()
}

function Assert-RemoteMainAncestor {
    Invoke-Git $RepoRoot @("fetch", "--quiet", $Remote, "main")

    $remoteRef = "$Remote/main"
    & git -C $RepoRoot rev-parse --verify $remoteRef *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to resolve $remoteRef after fetching it."
    }

    & git -C $RepoRoot merge-base --is-ancestor $remoteRef HEAD
    $ancestorExitCode = $LASTEXITCODE
    if ($ancestorExitCode -eq 0) {
        return
    }
    if ($ancestorExitCode -ne 1) {
        throw "Failed to compare local main with $remoteRef."
    }

    $counts = @(& git -C $RepoRoot rev-list --left-right --count "$remoteRef...HEAD")
    if ($LASTEXITCODE -ne 0 -or $counts.Count -ne 1) {
        throw "Local main is not based on the latest $remoteRef. Fetch and synchronize before publishing."
    }

    $parts = @(([string]$counts[0]) -split '\s+' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($parts.Count -ne 2) {
        throw "Local main is not based on the latest $remoteRef. Fetch and synchronize before publishing."
    }

    $remoteOnly = [int]$parts[0]
    $localOnly = [int]$parts[1]
    throw "Local main is stale or diverged from $remoteRef (remote-only commits: $remoteOnly, local-only commits: $localOnly). Synchronize main before publishing; do not force-push generated SDK commits."
}

function Invoke-Build {
    param([Parameter(Mandatory = $true)][string]$Target)

    & $BuildScript -Target $Target -Configuration "Release" -OcctRoot $OcctRoot
    if ($LASTEXITCODE -ne 0) {
        throw "build.ps1 $Target failed with exit code $LASTEXITCODE."
    }
}

function Commit-IfChanged {
    param(
        [Parameter(Mandatory = $true)][string[]]$Paths,
        [Parameter(Mandatory = $true)][string]$Message
    )

    Invoke-Git $RepoRoot (@("add", "--") + $Paths)
    $staged = @(& git -C $RepoRoot diff --cached --name-only -- $Paths)
    if ($LASTEXITCODE -ne 0) { throw "Failed to inspect staged changes for: $($Paths -join ', ')" }
    if ($staged.Count -gt 0) {
        Invoke-Git $RepoRoot @("commit", "-m", $Message)
        return $true
    }
    return $false
}

function Test-BinarySdk {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [string]$ExpectedSourceCommit = ""
    )

    $requiredFiles = @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "OcctNet.Avalonia.dll",
        "bridge-contract.json",
        "bridge-manifest.json"
    )
    foreach ($required in $requiredFiles) {
        if (-not (Test-Path (Join-Path $Path $required) -PathType Leaf)) {
            throw "Binary SDK file is missing: $required"
        }
    }

    $contractPath = Join-Path $Path "bridge-contract.json"
    $manifestPath = Join-Path $Path "bridge-manifest.json"
    $contract = Get-Content $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $manifest = Get-Content $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

    if ([int]$contract.schemaVersion -ne 3 -or
        [int]$contract.nativeAbi.current -ne 5 -or
        [int]$contract.nativeAbi.minimumSupported -ne 5 -or
        [string]$contract.api.policy -ne "abi5-only") {
        throw "Binary SDK contract must remain Bridge 3 ABI5-only."
    }

    if ($manifest.PSObject.Properties.Name -contains "nativeAbiVersion") {
        throw "Binary SDK manifest must not contain retired flat nativeAbiVersion metadata."
    }

    if ([int]$manifest.schemaVersion -ne 2 -or
        [string]$manifest.author -ne [string]$contract.author -or
        [string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [int]$manifest.nativeAbi.current -ne [int]$contract.nativeAbi.current -or
        [int]$manifest.nativeAbi.minimumSupported -ne [int]$contract.nativeAbi.minimumSupported -or
        [string]$manifest.occtVersion -ne [string]$contract.occtVersion -or
        [string]$manifest.platform -ne [string]$contract.platform -or
        [string]$manifest.targetFramework -ne [string]$contract.dotnet.targetFramework -or
        [string]$manifest.sdkVersion -ne [string]$contract.dotnet.sdkVersion -or
        [string]$manifest.languageVersion -ne [string]$contract.dotnet.languageVersion -or
        [string]$manifest.configuration -ne "Release") {
        throw "Binary SDK manifest does not match bridge-contract.json or is not a Release ABI5 SDK."
    }

    if ([string]::IsNullOrWhiteSpace([string]$manifest.sourceCommit)) {
        throw "Binary SDK sourceCommit is missing."
    }
    if (-not [string]::IsNullOrWhiteSpace($ExpectedSourceCommit) -and
        [string]$manifest.sourceCommit -ne $ExpectedSourceCommit) {
        throw "Binary SDK sourceCommit does not match the source commit used for publishing."
    }

    $expectedHashedFiles = @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "OcctNet.Avalonia.dll",
        "bridge-contract.json"
    )
    $entries = @($manifest.files)
    $manifestNames = @($entries | ForEach-Object { [string]$_.name })
    if ($manifestNames.Count -ne $expectedHashedFiles.Count) {
        throw "Binary SDK manifest contains an unexpected number of hashed files."
    }
    foreach ($name in $expectedHashedFiles) {
        if ($name -notin $manifestNames) {
            throw "Binary SDK manifest does not hash required file: $name"
        }
    }
    if (@($manifestNames | Group-Object | Where-Object Count -ne 1).Count -gt 0) {
        throw "Binary SDK manifest contains duplicate file entries."
    }

    foreach ($entry in $entries) {
        $name = [string]$entry.name
        if ([string]::IsNullOrWhiteSpace($name) -or $name.Contains('/') -or $name.Contains('\')) {
            throw "Invalid Binary SDK manifest file name: $name"
        }
        $file = Join-Path $Path $name
        if (-not (Test-Path $file -PathType Leaf)) { throw "Manifest file is missing: $name" }
        $hash = (Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($hash -ne ([string]$entry.sha256).ToLowerInvariant()) {
            throw "Binary SDK hash mismatch: $name"
        }
    }
}

Assert-Command "git"
if (-not (Test-Path $BuildScript -PathType Leaf)) { throw "build.ps1 was not found." }

$currentBranch = Get-CurrentBranch -WorkingDirectory $RepoRoot
if ($currentBranch -ne "main") {
    throw "publish.ps1 must be run from the main branch. Current branch: $currentBranch"
}

$initialChanges = @(& git -C $RepoRoot status --porcelain --untracked-files=all)
if ($LASTEXITCODE -ne 0) { throw "Failed to inspect the main working tree." }
if ($initialChanges.Count -gt 0) {
    throw "The main working tree must be clean before publishing."
}

Assert-RemoteMainAncestor
Write-Host "[publish] Remote main ancestry validated." -ForegroundColor DarkGray

if ($Fast) {
    Write-Host "[publish] Fast mode: skipping API documentation generation." -ForegroundColor Yellow
}
else {
    Write-Host "[publish] Generating complete bilingual API reference..." -ForegroundColor Cyan
    Invoke-Build "docs"
    [void](Commit-IfChanged -Paths @("docs/zh-CN/api", "docs/en-US/api") -Message "Update generated API reference")

    $afterDocs = @(& git -C $RepoRoot status --porcelain --untracked-files=all)
    if ($LASTEXITCODE -ne 0 -or $afterDocs.Count -gt 0) {
        throw "The worktree is not clean after API documentation generation. Review unexpected generated files before publishing."
    }
}

$sourceCommit = (& git -C $RepoRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sourceCommit)) {
    throw "Failed to resolve the source commit used for Binary SDK publishing."
}

Write-Host "[publish] Building and validating the Release Binary SDK..." -ForegroundColor Cyan
Invoke-Build "dist"
Test-BinarySdk -Path $DistRoot -ExpectedSourceCommit $sourceCommit
[void](Commit-IfChanged -Paths @("dist/win-x64") -Message "Publish Bridge Binary SDK")

Invoke-Git $RepoRoot @("push", $Remote, "main")

$publishedCommit = (& git -C $RepoRoot rev-parse HEAD).Trim()
Write-Host "Bridge Binary SDK published to main." -ForegroundColor Green
Write-Host "Source:    $sourceCommit" -ForegroundColor DarkGray
Write-Host "Published: $publishedCommit" -ForegroundColor DarkGray
Write-Host "Demo consumers can now run .\sync.ps1 on the demo branch." -ForegroundColor Cyan
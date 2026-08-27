param(
    [string]$SourceBranch = 'main',
    [string]$OcctRoot = '',
    [switch]$ForceRebuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$bridgeRepository = 'https://github.com/zly258/OcctCSharpBridge.git'
$cacheRoot = Join-Path $root 'artifacts\dependencies'
$sourceRepo = Join-Path $cacheRoot 'OcctCSharpBridge'
$destination = Join-Path $root 'external\OcctCSharpBridge\win-x64'

function Assert-File {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required Bridge SDK file was not found: $Path"
    }
}

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )

    & git @Arguments
    if ($LASTEXITCODE -ne 0) { throw $ErrorMessage }
}

function Read-ValidatedSdk {
    param([Parameter(Mandatory = $true)][string]$SdkRoot)

    foreach ($name in @(
        'OcctNative.dll',
        'OcctNet.dll',
        'OcctNet.WinForms.dll',
        'OcctNet.Wpf.dll',
        'OcctNet.Avalonia.dll',
        'bridge-contract.json',
        'bridge-manifest.json')) {
        Assert-File (Join-Path $SdkRoot $name)
    }

    $contract = Get-Content -LiteralPath (Join-Path $SdkRoot 'bridge-contract.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    try { $sdkBaseline = [version][string]$contract.dotnet.sdkVersion }
    catch { throw 'The generated Bridge SDK contains an invalid .NET SDK baseline.' }

    $consumerFrameworks = @($contract.dotnet.supportedConsumerFrameworks | ForEach-Object { [string]$_ })
    $desktopConsumerFrameworks = @($contract.dotnet.supportedDesktopConsumerFrameworks | ForEach-Object { [string]$_ })
    if ([int]$contract.schemaVersion -ne 3 -or
        [int]$contract.nativeAbi.current -ne 5 -or
        [int]$contract.nativeAbi.minimumSupported -ne 5 -or
        [string]$contract.api.policy -ne 'abi5-only' -or
        [string]$contract.platform -ne 'win-x64' -or
        [string]::IsNullOrWhiteSpace([string]$contract.dotnet.targetFramework) -or
        [string]::IsNullOrWhiteSpace([string]$contract.dotnet.desktopTargetFramework) -or
        'net10.0' -notin $consumerFrameworks -or
        'net10.0-windows' -notin $desktopConsumerFrameworks -or
        $sdkBaseline.Major -ne 10 -or
        $sdkBaseline.Minor -ne 0 -or
        [string]$contract.dotnet.sdkRollForward -ne 'latestFeature') {
        throw 'The generated SDK is not a compatible Bridge 3 ABI5-only win-x64 SDK for a .NET 10 consumer.'
    }

    $manifest = Get-Content -LiteralPath (Join-Path $SdkRoot 'bridge-manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    try { $resolvedSdkVersion = [version][string]$manifest.resolvedSdkVersion }
    catch { throw 'The generated Bridge SDK manifest contains an invalid resolved .NET SDK version.' }

    if ([int]$manifest.schemaVersion -ne 2 -or
        [int]$manifest.nativeAbi.current -ne 5 -or
        [int]$manifest.nativeAbi.minimumSupported -ne 5 -or
        [string]$manifest.author -ne [string]$contract.author -or
        [string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [string]$manifest.occtVersion -ne [string]$contract.occtVersion -or
        [string]$manifest.platform -ne [string]$contract.platform -or
        [string]$manifest.targetFramework -ne [string]$contract.dotnet.targetFramework -or
        [string]$manifest.sdkVersion -ne [string]$contract.dotnet.sdkVersion -or
        $resolvedSdkVersion.Major -ne 10 -or
        $resolvedSdkVersion.Minor -ne 0 -or
        $resolvedSdkVersion -lt $sdkBaseline -or
        [string]$manifest.languageVersion -ne [string]$contract.dotnet.languageVersion -or
        [string]$manifest.configuration -ne 'Release' -or
        [string]::IsNullOrWhiteSpace([string]$manifest.sourceCommit)) {
        throw 'The generated Bridge SDK manifest is invalid or does not match its contract/build policy.'
    }

    $entries = @($manifest.files)
    if ($entries.Count -eq 0) { throw 'The Bridge SDK manifest does not contain file hashes.' }

    foreach ($entry in $entries) {
        $name = [string]$entry.name
        $expectedHash = ([string]$entry.sha256).ToLowerInvariant()
        if ([string]::IsNullOrWhiteSpace($name) -or [string]::IsNullOrWhiteSpace($expectedHash)) {
            throw 'The Bridge SDK manifest contains an invalid file hash entry.'
        }

        $path = Join-Path $SdkRoot $name
        Assert-File $path
        $actualHash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actualHash -ne $expectedHash) {
            throw "Bridge SDK hash mismatch: $name"
        }
    }

    foreach ($requiredHash in @(
        'OcctNative.dll',
        'OcctNet.dll',
        'OcctNet.Wpf.dll',
        'bridge-contract.json')) {
        if ($requiredHash -notin @($entries | ForEach-Object { [string]$_.name })) {
            throw "The Bridge SDK manifest does not hash required file: $requiredHash"
        }
    }

    return [pscustomobject]@{
        Contract = $contract
        Manifest = $manifest
    }
}

function Install-Sdk {
    param([Parameter(Mandatory = $true)][string]$Source)

    $sdk = Read-ValidatedSdk $Source
    $destinationParent = Split-Path -Parent $destination
    New-Item -ItemType Directory -Path $destinationParent -Force | Out-Null

    $staging = Join-Path $destinationParent ('.win-x64-staging-' + [Guid]::NewGuid().ToString('N'))
    $backup = Join-Path $destinationParent ('.win-x64-backup-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $staging -Force | Out-Null

    try {
        Copy-Item -Path (Join-Path $Source '*') -Destination $staging -Recurse -Force
        [void](Read-ValidatedSdk $staging)

        $hadPrevious = Test-Path -LiteralPath $destination -PathType Container
        if ($hadPrevious) { Move-Item -LiteralPath $destination -Destination $backup }
        try {
            Move-Item -LiteralPath $staging -Destination $destination
        }
        catch {
            if ($hadPrevious -and (Test-Path -LiteralPath $backup -PathType Container)) {
                Move-Item -LiteralPath $backup -Destination $destination
            }
            throw
        }
        Remove-Item -LiteralPath $backup -Recurse -Force -ErrorAction SilentlyContinue
    }
    finally {
        Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue
        if (-not (Test-Path -LiteralPath $destination -PathType Container) -and
            (Test-Path -LiteralPath $backup -PathType Container)) {
            Move-Item -LiteralPath $backup -Destination $destination
        }
    }

    Write-Host '[bridge] Binary SDK synchronized.' -ForegroundColor Green
    Write-Host "[bridge] Bridge: $($sdk.Contract.bridgeVersion), ABI 5, OCCT $($sdk.Contract.occtVersion)" -ForegroundColor DarkGray
    Write-Host "[bridge] Library TFM: $($sdk.Contract.dotnet.targetFramework) / $($sdk.Contract.dotnet.desktopTargetFramework)" -ForegroundColor DarkGray
    Write-Host "[bridge] Build SDK:   $($sdk.Manifest.resolvedSdkVersion) (baseline $($sdk.Contract.dotnet.sdkVersion))" -ForegroundColor DarkGray
    Write-Host "[bridge] Source:      $($sdk.Manifest.sourceCommit)" -ForegroundColor DarkGray
    Write-Host "[bridge] SDK:         $destination" -ForegroundColor DarkGray
}

function Remove-LegacyWorktrees {
    if (-not (Test-Path -LiteralPath $cacheRoot -PathType Container)) { return }

    if (Test-Path -LiteralPath (Join-Path $sourceRepo '.git')) {
        & git -C $sourceRepo worktree prune *> $null
    }

    foreach ($legacy in @(Get-ChildItem -LiteralPath $cacheRoot -Directory -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like '.OcctCSharpBridge*-sdk-*' })) {
        Write-Host "[bridge] Removing legacy temporary worktree: $($legacy.Name)" -ForegroundColor DarkGray
        Remove-Item -LiteralPath $legacy.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }

    if (Test-Path -LiteralPath (Join-Path $sourceRepo '.git')) {
        & git -C $sourceRepo worktree prune *> $null
    }
}

if ($null -eq (Get-Command git -ErrorAction SilentlyContinue)) {
    throw 'git was not found in PATH.'
}

New-Item -ItemType Directory -Path $cacheRoot -Force | Out-Null
Remove-LegacyWorktrees

$clonedSource = $false
if (-not (Test-Path -LiteralPath (Join-Path $sourceRepo '.git'))) {
    if (Test-Path -LiteralPath $sourceRepo) {
        Remove-Item -LiteralPath $sourceRepo -Recurse -Force
    }

    Write-Host '[bridge] Bridge source cache is missing; cloning OcctCSharpBridge...' -ForegroundColor Cyan
    Invoke-Git @('clone', '--quiet', '--filter=blob:none', $bridgeRepository, $sourceRepo) `
        'Unable to clone OcctCSharpBridge.'
    $clonedSource = $true
}
else {
    $remoteUrl = ([string](& git -C $sourceRepo remote get-url origin)).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Unable to read the cached Bridge origin URL.' }
    if ($remoteUrl -ne $bridgeRepository) {
        Invoke-Git @('-C', $sourceRepo, 'remote', 'set-url', 'origin', $bridgeRepository) `
            'Unable to repair the cached Bridge origin URL.'
    }
}

$sourceCommit = ''
if ($ForceRebuild) {
    Write-Host "[bridge] Fetching latest origin/$SourceBranch..." -ForegroundColor Cyan
    Invoke-Git @('-C', $sourceRepo, 'fetch', '--quiet', '--prune', 'origin', $SourceBranch) `
        "Unable to fetch OcctCSharpBridge origin/$SourceBranch."
    $sourceCommit = ([string](& git -C $sourceRepo rev-parse FETCH_HEAD)).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sourceCommit)) {
        throw "Unable to resolve OcctCSharpBridge origin/$SourceBranch."
    }
    Write-Host "[bridge] Latest source: origin/$SourceBranch @ $($sourceCommit.Substring(0, 7))" -ForegroundColor DarkGray
}
else {
    $sourceCommit = ([string](& git -C $sourceRepo rev-parse HEAD)).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sourceCommit)) {
        throw 'Unable to resolve the cached OcctCSharpBridge source commit.'
    }
    Write-Host "[bridge] Using cached Bridge source @ $($sourceCommit.Substring(0, 7)); network update skipped." -ForegroundColor DarkGray
}

if (-not $ForceRebuild -and (Test-Path -LiteralPath $destination -PathType Container)) {
    try {
        $existingSdk = Read-ValidatedSdk $destination
        if ([string]$existingSdk.Manifest.sourceCommit -eq $sourceCommit) {
            Write-Host '[bridge] Binary SDK matches the cached source; rebuild skipped.' -ForegroundColor Green
            Write-Host "[bridge] Source: $sourceCommit" -ForegroundColor DarkGray
            Write-Host "[bridge] Cache:  $sourceRepo" -ForegroundColor DarkGray
            Write-Host "[bridge] SDK:    $destination" -ForegroundColor DarkGray
            exit 0
        }
        Write-Host '[bridge] Cached source differs from the staged SDK; checking cached dist output.' -ForegroundColor DarkGray
    }
    catch {
        Write-Host '[bridge] Existing Binary SDK is incomplete or invalid; checking cached dist output.' -ForegroundColor DarkGray
    }
}
elseif ($ForceRebuild) {
    Write-Host '[bridge] Latest Bridge source update and Binary SDK rebuild requested.' -ForegroundColor DarkGray
}

$cachedDist = Join-Path $sourceRepo 'dist\win-x64'
if (-not $ForceRebuild -and (Test-Path -LiteralPath $cachedDist -PathType Container)) {
    try {
        $cachedSdk = Read-ValidatedSdk $cachedDist
        if ([string]$cachedSdk.Manifest.sourceCommit -eq $sourceCommit) {
            Write-Host '[bridge] Reusing validated cached Bridge dist output; rebuild skipped.' -ForegroundColor Green
            Install-Sdk $cachedDist
            Write-Host "[bridge] Cache:  $sourceRepo" -ForegroundColor DarkGray
            exit 0
        }
        Write-Host '[bridge] Cached dist output was built from a different source commit; rebuilding.' -ForegroundColor DarkGray
    }
    catch {
        Write-Host '[bridge] Cached dist output is incomplete or invalid; rebuilding.' -ForegroundColor DarkGray
    }
}

Write-Host '[bridge] Preparing fixed Bridge source cache...' -ForegroundColor DarkGray
Invoke-Git @('-C', $sourceRepo, 'checkout', '--quiet', '--detach', '--force', $sourceCommit) `
    'Unable to checkout the Bridge source commit.'
Invoke-Git @('-C', $sourceRepo, 'reset', '--hard', '--quiet', $sourceCommit) `
    'Unable to reset the Bridge source cache.'
Invoke-Git @('-C', $sourceRepo, 'clean', '-ffdx', '--quiet') `
    'Unable to clean the Bridge source cache.'

$buildScript = Join-Path $sourceRepo 'build.ps1'
Assert-File $buildScript

Write-Host '[bridge] Building validated win-x64 Binary SDK from cached source...' -ForegroundColor Cyan
$buildParameters = @{
    Target = 'dist'
    Configuration = 'Release'
}
if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) {
    $buildParameters.OcctRoot = $OcctRoot
}

& $buildScript @buildParameters
if (-not $?) { throw 'OcctCSharpBridge Binary SDK build failed.' }

$builtRoot = Join-Path $sourceRepo 'dist\win-x64'
$builtSdk = Read-ValidatedSdk $builtRoot
if ([string]$builtSdk.Manifest.sourceCommit -ne $sourceCommit) {
    throw "Built Bridge sourceCommit '$($builtSdk.Manifest.sourceCommit)' does not match '$sourceCommit'."
}

Install-Sdk $builtRoot
Write-Host "[bridge] Cache:  $sourceRepo" -ForegroundColor DarkGray

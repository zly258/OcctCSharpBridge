param(
    [string]$Remote = "origin",
    [string]$SourceBranch = "main",
    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$SdkRoot = "",
    [switch]$ForceRebuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$Destination = Join-Path $RepoRoot "dist\win-x64"
$WorkspaceRoot = Split-Path -Parent $RepoRoot
$WorktreeRoot = Join-Path $WorkspaceRoot (".OcctCSharpBridge-main-sdk-" + [Guid]::NewGuid().ToString("N"))
$WorktreeAdded = $false

function Assert-File {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Required SDK file was not found: $Path" }
}

function Read-ValidatedSdk {
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
        [string]$contract.dotnet.languageVersion -ne "14.0") {
        throw "The source SDK is not the expected Bridge 3 ABI5-only win-x64 SDK."
    }

    $coreFramework = [string]$contract.dotnet.targetFramework
    $desktopFramework = [string]$contract.dotnet.desktopTargetFramework
    if ($coreFramework -notin @("net8.0", "net9.0", "net10.0")) {
        throw "Unsupported Bridge Core target framework: $coreFramework"
    }
    if ($desktopFramework -ne "$coreFramework-windows") {
        throw "Bridge Desktop target framework '$desktopFramework' does not match Core target '$coreFramework'."
    }

    $sdkBaseline = [string]$contract.dotnet.sdkVersion
    try { $sdkVersion = [version]$sdkBaseline }
    catch { throw "The source SDK contains an invalid .NET SDK baseline: $sdkBaseline" }
    if ($sdkVersion.Major -ne 10 -or $sdkVersion.Minor -ne 0) {
        throw "The source SDK must be built from the stable .NET 10 SDK line."
    }
    if ($contract.dotnet.PSObject.Properties.Name -contains "sdkRollForward") {
        $rollForward = [string]$contract.dotnet.sdkRollForward
        if ($rollForward -ne "latestFeature") {
            throw "Unsupported source SDK roll-forward policy: $rollForward"
        }
    }

    $manifest = Get-Content -LiteralPath (Join-Path $Root "bridge-manifest.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([int]$manifest.schemaVersion -ne 2 -or
        [int]$manifest.nativeAbi.current -ne 5 -or
        [int]$manifest.nativeAbi.minimumSupported -ne 5 -or
        [string]$manifest.author -ne [string]$contract.author -or
        [string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [string]$manifest.occtVersion -ne [string]$contract.occtVersion -or
        [string]$manifest.platform -ne [string]$contract.platform -or
        [string]$manifest.targetFramework -ne $coreFramework -or
        [string]$manifest.sdkVersion -ne $sdkBaseline -or
        [string]$manifest.languageVersion -ne [string]$contract.dotnet.languageVersion -or
        [string]$manifest.configuration -ne "Release" -or
        [string]::IsNullOrWhiteSpace([string]$manifest.sourceCommit)) {
        throw "The source SDK manifest is not the expected Bridge 3 ABI5-only win-x64 manifest."
    }

    $requiredHashes = @(
        "OcctNative.dll",
        "OcctNet.dll",
        "OcctNet.WinForms.dll",
        "OcctNet.Wpf.dll",
        "OcctNet.Avalonia.dll",
        "bridge-contract.json")
    $entries = @($manifest.files)
    $names = @($entries | ForEach-Object { [string]$_.name })
    if ($names.Count -ne $requiredHashes.Count) { throw "The SDK manifest contains an unexpected number of hashed files." }
    foreach ($name in $requiredHashes) {
        if ($name -notin $names) { throw "The SDK manifest does not hash required file: $name" }
    }
    foreach ($entry in $entries) {
        $path = Join-Path $Root ([string]$entry.name)
        Assert-File $path
        $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne ([string]$entry.sha256).ToLowerInvariant()) {
            throw "The SDK manifest hash does not match: $($entry.name)"
        }
    }

    return [pscustomobject]@{
        Contract = $contract
        Manifest = $manifest
    }
}

function Copy-Sdk {
    param([Parameter(Mandatory = $true)][string]$Source)

    $sdk = Read-ValidatedSdk $Source
    if (Test-Path -LiteralPath $Destination -PathType Container) { Remove-Item -LiteralPath $Destination -Recurse -Force }
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    Copy-Item -Path (Join-Path $Source "*") -Destination $Destination -Recurse -Force
    Write-Host "Binary SDK synchronized." -ForegroundColor Green
    Write-Host "Bridge: $($sdk.Contract.bridgeVersion), ABI 5 only, OCCT $($sdk.Contract.occtVersion), target $($sdk.Contract.dotnet.targetFramework), SDK baseline $($sdk.Contract.dotnet.sdkVersion)" -ForegroundColor DarkGray
    Write-Host "Path:   $Destination" -ForegroundColor DarkGray
}

if (-not [string]::IsNullOrWhiteSpace($SdkRoot)) {
    if ($ForceRebuild) { throw "-ForceRebuild cannot be combined with -SdkRoot; the supplied SDK is copied as-is after validation." }
    Copy-Sdk ([System.IO.Path]::GetFullPath($SdkRoot))
    exit 0
}

if ($null -eq (Get-Command git -ErrorAction SilentlyContinue)) { throw "git was not found in PATH." }

Write-Host "[sync] Fetching $Remote/$SourceBranch..." -ForegroundColor Cyan
& git -C $RepoRoot fetch --quiet $Remote $SourceBranch
if ($LASTEXITCODE -ne 0) { throw "Unable to fetch $Remote/$SourceBranch." }

$sourceCommit = ([string](& git -C $RepoRoot rev-parse "$Remote/$SourceBranch")).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sourceCommit)) {
    throw "Unable to resolve $Remote/$SourceBranch."
}

if (-not $ForceRebuild -and (Test-Path -LiteralPath $Destination -PathType Container)) {
    try {
        $sdk = Read-ValidatedSdk $Destination
        if ([string]$sdk.Manifest.sourceCommit -eq $sourceCommit) {
            Write-Host "Binary SDK is already synchronized; rebuild skipped." -ForegroundColor Green
            Write-Host "Source: $Remote/$SourceBranch @ $($sourceCommit.Substring(0, 7))" -ForegroundColor DarkGray
            Write-Host "Bridge: $($sdk.Contract.bridgeVersion), ABI 5 only, OCCT $($sdk.Contract.occtVersion), target $($sdk.Contract.dotnet.targetFramework), SDK baseline $($sdk.Contract.dotnet.sdkVersion)" -ForegroundColor DarkGray
            Write-Host "Path:   $Destination" -ForegroundColor DarkGray
            exit 0
        }
        Write-Host "[sync] Existing SDK is from a different source commit; rebuilding." -ForegroundColor DarkGray
    }
    catch {
        Write-Host "[sync] Existing SDK is incomplete or invalid; rebuilding." -ForegroundColor DarkGray
    }
}
elseif ($ForceRebuild) {
    Write-Host "[sync] Forced Binary SDK rebuild requested." -ForegroundColor DarkGray
}

try {
    Write-Host "[sync] Creating clean SDK worktree beside the repository..." -ForegroundColor DarkGray
    & git -C $RepoRoot worktree prune
    if ($LASTEXITCODE -ne 0) { throw "Unable to prune stale git worktrees." }

    & git -C $RepoRoot worktree add --detach $WorktreeRoot "$Remote/$SourceBranch"
    if ($LASTEXITCODE -ne 0) { throw "Unable to create worktree for $Remote/$SourceBranch." }
    $WorktreeAdded = $true

    $buildScript = Join-Path $WorktreeRoot "build.ps1"
    if (-not (Test-Path -LiteralPath $buildScript -PathType Leaf)) { throw "$Remote/$SourceBranch does not contain build.ps1." }

    Write-Host "[sync] Building validated win-x64 Binary SDK from $Remote/$SourceBranch..." -ForegroundColor Cyan
    $buildParameters = @{
        Target = "dist"
        Configuration = "Release"
    }
    if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) { $buildParameters.OcctRoot = $OcctRoot }
    & $buildScript @buildParameters
    if ($LASTEXITCODE -ne 0) { throw "Binary SDK build failed on $Remote/$SourceBranch." }

    $builtSdk = Read-ValidatedSdk (Join-Path $WorktreeRoot "dist\win-x64")
    if ([string]$builtSdk.Manifest.sourceCommit -ne $sourceCommit) {
        throw "Built SDK sourceCommit '$($builtSdk.Manifest.sourceCommit)' does not match $Remote/$SourceBranch '$sourceCommit'."
    }

    Copy-Sdk (Join-Path $WorktreeRoot "dist\win-x64")
}
finally {
    if ($WorktreeAdded) {
        & git -C $RepoRoot worktree remove --force $WorktreeRoot *> $null
    }
    elseif (Test-Path -LiteralPath $WorktreeRoot) {
        Remove-Item -LiteralPath $WorktreeRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

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
$SourceRepoRoot = Join-Path $RepoRoot ".cache\main-sdk-source"
$DemoCoreTargetFramework = "net10.0"
$DemoDesktopTargetFramework = "net10.0-windows"
$SdkFileNames = @(
    "OcctNative.dll",
    "OcctNet.dll",
    "OcctNet.WinForms.dll",
    "OcctNet.Wpf.dll",
    "OcctNet.Avalonia.dll",
    "bridge-contract.json",
    "bridge-manifest.json"
)

function Assert-File {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Required SDK file was not found: $Path" }
}

function Invoke-GitChecked {
    param(
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )

    $output = @(& git -C $WorkingDirectory @Arguments 2>&1)
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        $detail = @($output | ForEach-Object { [string]$_ }) -join [Environment]::NewLine
        if ([string]::IsNullOrWhiteSpace($detail)) { throw $ErrorMessage }
        throw "$ErrorMessage`n$detail"
    }
}

function Read-ValidatedSdk {
    param([Parameter(Mandatory = $true)][string]$Root)

    foreach ($name in $SdkFileNames) { Assert-File (Join-Path $Root $name) }

    $unexpectedEntries = @(
        Get-ChildItem -LiteralPath $Root -Force | Where-Object { $_.Name -notin $SdkFileNames }
    )
    if ($unexpectedEntries.Count -gt 0) {
        throw "The SDK root contains files or directories outside the validated payload: $((@($unexpectedEntries.Name | Sort-Object)) -join ', '). Use a clean Binary SDK directory."
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
    $supportedCoreFrameworks = @($contract.dotnet.supportedConsumerFrameworks | ForEach-Object { [string]$_ })
    $supportedDesktopFrameworks = @($contract.dotnet.supportedDesktopConsumerFrameworks | ForEach-Object { [string]$_ })
    if ($coreFramework -notin $supportedCoreFrameworks) {
        throw "Bridge Core target framework '$coreFramework' is not declared in supportedConsumerFrameworks."
    }
    if ($desktopFramework -notin $supportedDesktopFrameworks) {
        throw "Bridge Desktop target framework '$desktopFramework' is not declared in supportedDesktopConsumerFrameworks."
    }
    if ($DemoCoreTargetFramework -notin $supportedCoreFrameworks) {
        throw "The Binary SDK does not declare support for Demo target $DemoCoreTargetFramework. Supported: $($supportedCoreFrameworks -join ', ')."
    }
    if ($DemoDesktopTargetFramework -notin $supportedDesktopFrameworks) {
        throw "The Binary SDK does not declare support for Demo desktop target $DemoDesktopTargetFramework. Supported: $($supportedDesktopFrameworks -join ', ')."
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
    foreach ($name in $SdkFileNames) {
        Copy-Item -LiteralPath (Join-Path $Source $name) -Destination (Join-Path $Destination $name) -Force
    }
    Write-Host "Binary SDK synchronized." -ForegroundColor Green
    Write-Host "Bridge: $($sdk.Contract.bridgeVersion), ABI 5 only, OCCT $($sdk.Contract.occtVersion), target $($sdk.Contract.dotnet.targetFramework), SDK baseline $($sdk.Contract.dotnet.sdkVersion)" -ForegroundColor DarkGray
    Write-Host "Path:   $Destination" -ForegroundColor DarkGray
}

function Get-OrCreateSourceClone {
    param(
        [Parameter(Mandatory = $true)][string]$RemoteName,
        [Parameter(Mandatory = $true)][string]$Branch,
        [Parameter(Mandatory = $true)][string]$ExpectedCommit
    )

    $remoteUrl = ([string](& git -C $RepoRoot remote get-url $RemoteName)).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($remoteUrl)) {
        throw "Unable to resolve URL for Git remote '$RemoteName'."
    }

    $cacheGit = Join-Path $SourceRepoRoot ".git"
    if (-not (Test-Path -LiteralPath $cacheGit)) {
        if (Test-Path -LiteralPath $SourceRepoRoot) { Remove-Item -LiteralPath $SourceRepoRoot -Recurse -Force }
        New-Item -ItemType Directory -Path (Split-Path -Parent $SourceRepoRoot) -Force | Out-Null
        Write-Host "[sync] Creating reusable Bridge source clone in .cache/main-sdk-source..." -ForegroundColor DarkGray
        & git clone --no-checkout --no-tags $remoteUrl $SourceRepoRoot
        if ($LASTEXITCODE -ne 0) { throw "Unable to create reusable Bridge source clone from $remoteUrl." }
    }
    else {
        Invoke-GitChecked $SourceRepoRoot @("remote", "set-url", "origin", $remoteUrl) "Unable to refresh cached source remote URL."
    }

    Write-Host "[sync] Updating reusable Bridge source clone..." -ForegroundColor DarkGray
    Invoke-GitChecked $SourceRepoRoot @("fetch", "--quiet", "--prune", "origin", $Branch) "Unable to fetch origin/$Branch in cached source clone."
    $fetchedCommit = ([string](& git -C $SourceRepoRoot rev-parse "FETCH_HEAD")).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($fetchedCommit)) { throw "Unable to resolve fetched source commit." }
    if ($fetchedCommit -ne $ExpectedCommit) {
        throw "Cached source commit '$fetchedCommit' does not match $RemoteName/$Branch '$ExpectedCommit'."
    }

    Invoke-GitChecked $SourceRepoRoot @("reset", "--hard") "Unable to reset cached Bridge source clone."
    Invoke-GitChecked $SourceRepoRoot @("clean", "-fd") "Unable to clean untracked files in cached Bridge source clone."
    Invoke-GitChecked $SourceRepoRoot @("checkout", "--detach", "--force", $ExpectedCommit) "Unable to checkout cached Bridge source commit $ExpectedCommit."

    return $SourceRepoRoot
}

function Invoke-BridgeBuildTarget {
    param(
        [Parameter(Mandatory = $true)][string]$BuildScript,
        [Parameter(Mandatory = $true)][string]$TargetName
    )

    $parameters = @{
        Target = $TargetName
        Configuration = "Release"
    }
    if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) { $parameters.OcctRoot = $OcctRoot }
    & $BuildScript @parameters
    if ($LASTEXITCODE -ne 0) { throw "Bridge build target '$TargetName' failed on $Remote/$SourceBranch." }
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

$sourceRoot = Get-OrCreateSourceClone $Remote $SourceBranch $sourceCommit
$buildScript = Join-Path $sourceRoot "build.ps1"
if (-not (Test-Path -LiteralPath $buildScript -PathType Leaf)) { throw "$Remote/$SourceBranch does not contain build.ps1." }

$buildScriptText = Get-Content -LiteralPath $buildScript -Raw -Encoding UTF8
if ($buildScriptText -match '(?s)ValidateSet\([^)]*"sdk"') {
    Write-Host "[sync] Running the validated win-x64 Binary SDK release gate in the reusable source clone..." -ForegroundColor Cyan
    Invoke-BridgeBuildTarget $buildScript "sdk"
}
else {
    Write-Host "[sync] Source revision predates the 'sdk' target; running the equivalent validated legacy sequence: all -> dist." -ForegroundColor Yellow
    Invoke-BridgeBuildTarget $buildScript "all"
    Invoke-BridgeBuildTarget $buildScript "dist"
}

$builtSdkRoot = Join-Path $sourceRoot "dist\win-x64"
$builtSdk = Read-ValidatedSdk $builtSdkRoot
if ([string]$builtSdk.Manifest.sourceCommit -ne $sourceCommit) {
    throw "Built SDK sourceCommit '$($builtSdk.Manifest.sourceCommit)' does not match $Remote/$SourceBranch '$sourceCommit'."
}

Copy-Sdk $builtSdkRoot

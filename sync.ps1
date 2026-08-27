param(
    [string]$Remote = "origin",
    [string]$SourceBranch = "main",
    [string]$SdkRoot = "",
    [string]$PortableRoot = "",
    [switch]$ForceRebuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$ExternalRoot = Join-Path $RepoRoot "external"
$ExternalCacheRoot = Join-Path $ExternalRoot ".cache\OcctCSharpBridge-source"
$BridgeRoot = Join-Path $ExternalRoot "OcctCSharpBridge"
$Destination = Join-Path $BridgeRoot "win-x64"
$PortableDestination = Join-Path $BridgeRoot "portable\win-x64"
$LegacyDestination = Join-Path $RepoRoot "dist\win-x64"
$LegacyPortableDestination = Join-Path $RepoRoot "dist\portable\win-x64"
New-Item -ItemType Directory -Path $ExternalCacheRoot -Force | Out-Null
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
$HashedSdkFileNames = @(
    "OcctNative.dll",
    "OcctNet.dll",
    "OcctNet.WinForms.dll",
    "OcctNet.Wpf.dll",
    "OcctNet.Avalonia.dll",
    "bridge-contract.json"
)

function Assert-File {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required file was not found: $Path"
    }
}

function Invoke-GitChecked {
    param(
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )

    $output = @(& git -C $WorkingDirectory @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) {
        $detail = @($output | ForEach-Object { [string]$_ }) -join [Environment]::NewLine
        if ([string]::IsNullOrWhiteSpace($detail)) { throw $ErrorMessage }
        throw "$ErrorMessage`n$detail"
    }
}

function Read-ValidatedSdk {
    param([Parameter(Mandatory = $true)][string]$Root)

    foreach ($name in $SdkFileNames) { Assert-File (Join-Path $Root $name) }

    $unexpected = @(Get-ChildItem -LiteralPath $Root -Force | Where-Object { $_.Name -notin $SdkFileNames })
    if ($unexpected.Count -gt 0) {
        throw "Binary SDK contains files outside the strict payload: $((@($unexpected.Name | Sort-Object)) -join ', ')."
    }

    $contract = Get-Content -LiteralPath (Join-Path $Root "bridge-contract.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([int]$contract.schemaVersion -ne 3 -or
        [int]$contract.nativeAbi.current -ne 5 -or
        [int]$contract.nativeAbi.minimumSupported -ne 5 -or
        [string]$contract.api.policy -ne "abi5-only" -or
        [string]$contract.platform -ne "win-x64" -or
        [string]$contract.dotnet.languageVersion -ne "14.0") {
        throw "Binary SDK is not the expected Bridge 3 ABI5-only win-x64 contract."
    }

    $coreFramework = [string]$contract.dotnet.targetFramework
    $desktopFramework = [string]$contract.dotnet.desktopTargetFramework
    $supportedCore = @($contract.dotnet.supportedConsumerFrameworks | ForEach-Object { [string]$_ })
    $supportedDesktop = @($contract.dotnet.supportedDesktopConsumerFrameworks | ForEach-Object { [string]$_ })
    if ($coreFramework -notin $supportedCore -or $desktopFramework -notin $supportedDesktop) {
        throw "Binary SDK target frameworks are inconsistent with the declared consumer framework lists."
    }
    if ($DemoCoreTargetFramework -notin $supportedCore) {
        throw "Binary SDK does not support Demo target $DemoCoreTargetFramework. Supported: $($supportedCore -join ', ')."
    }
    if ($DemoDesktopTargetFramework -notin $supportedDesktop) {
        throw "Binary SDK does not support Demo desktop target $DemoDesktopTargetFramework. Supported: $($supportedDesktop -join ', ')."
    }

    $sdkBaseline = [string]$contract.dotnet.sdkVersion
    try { $parsedSdk = [version]$sdkBaseline }
    catch { throw "Binary SDK contains an invalid build SDK baseline: $sdkBaseline" }
    if ($parsedSdk.Major -ne 10 -or $parsedSdk.Minor -ne 0) {
        throw "Binary SDK must use the stable .NET 10 build SDK line."
    }
    if ([string]$contract.dotnet.sdkRollForward -ne "latestFeature") {
        throw "Binary SDK must use sdkRollForward=latestFeature."
    }

    $manifest = Get-Content -LiteralPath (Join-Path $Root "bridge-manifest.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([int]$manifest.schemaVersion -ne 2 -or
        [int]$manifest.nativeAbi.current -ne 5 -or
        [int]$manifest.nativeAbi.minimumSupported -ne 5 -or
        [string]$manifest.bridgeVersion -ne [string]$contract.bridgeVersion -or
        [string]$manifest.occtVersion -ne [string]$contract.occtVersion -or
        [string]$manifest.platform -ne "win-x64" -or
        [string]$manifest.targetFramework -ne $coreFramework -or
        [string]$manifest.sdkVersion -ne $sdkBaseline -or
        [string]$manifest.languageVersion -ne "14.0" -or
        [string]$manifest.configuration -ne "Release" -or
        [string]::IsNullOrWhiteSpace([string]$manifest.sourceCommit)) {
        throw "Binary SDK manifest is invalid or inconsistent with its contract."
    }

    $entries = @($manifest.files)
    $names = @($entries | ForEach-Object { [string]$_.name })
    if ($names.Count -ne $HashedSdkFileNames.Count) {
        throw "Binary SDK manifest contains an unexpected number of hashed files."
    }
    foreach ($name in $HashedSdkFileNames) {
        if ($name -notin $names) { throw "Binary SDK manifest does not hash required file: $name" }
    }
    foreach ($entry in $entries) {
        $path = Join-Path $Root ([string]$entry.name)
        Assert-File $path
        $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne ([string]$entry.sha256).ToLowerInvariant()) {
            throw "Binary SDK hash mismatch: $($entry.name)"
        }
    }

    return [pscustomobject]@{ Contract = $contract; Manifest = $manifest }
}

function Read-ValidatedPortable {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$ExpectedSourceCommit,
        [Parameter(Mandatory = $true)][string]$ExpectedBridgeVersion
    )

    Assert-File (Join-Path $Root "package-manifest.json")
    Assert-File (Join-Path $Root "bridge-contract.json")
    Assert-File (Join-Path $Root "bridge-manifest.json")
    Assert-File (Join-Path $Root "runtime\OcctNative.dll")
    if (-not (Test-Path -LiteralPath (Join-Path $Root "occt\resources") -PathType Container)) {
        throw "Portable SDK OCCT resources are missing: $Root"
    }

    $package = Get-Content -LiteralPath (Join-Path $Root "package-manifest.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$package.product -ne "OcctCSharpBridge Portable SDK" -or
        [string]$package.platform -ne "win-x64" -or
        -not [bool]$package.portableRuntime -or
        [string]$package.bridgeSourceCommit -ne $ExpectedSourceCommit -or
        [string]$package.bridgeVersion -ne $ExpectedBridgeVersion) {
        throw "Portable SDK does not match the selected Binary SDK/source commit."
    }

    foreach ($entry in @($package.files)) {
        $relative = ([string]$entry.name).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
        $path = Join-Path $Root $relative
        Assert-File $path
        $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne ([string]$entry.sha256).ToLowerInvariant()) {
            throw "Portable SDK hash mismatch: $($entry.name)"
        }
    }

    return $package
}

function Copy-Sdk {
    param([Parameter(Mandatory = $true)][string]$Source)

    $sdk = Read-ValidatedSdk $Source
    Remove-Item -LiteralPath $Destination -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    foreach ($name in $SdkFileNames) {
        Copy-Item -LiteralPath (Join-Path $Source $name) -Destination (Join-Path $Destination $name) -Force
    }
    [void](Read-ValidatedSdk $Destination)

    Write-Host "Binary SDK synchronized." -ForegroundColor Green
    Write-Host "Source: $($sdk.Manifest.sourceCommit)" -ForegroundColor DarkGray
    Write-Host "Path:   $Destination" -ForegroundColor DarkGray
    return $sdk
}

function Copy-Portable {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$ExpectedSourceCommit,
        [Parameter(Mandatory = $true)][string]$ExpectedBridgeVersion
    )

    [void](Read-ValidatedPortable $Source $ExpectedSourceCommit $ExpectedBridgeVersion)
    Remove-Item -LiteralPath $PortableDestination -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $PortableDestination -Force | Out-Null
    Get-ChildItem -LiteralPath $Source -Force | Copy-Item -Destination $PortableDestination -Recurse -Force
    [void](Read-ValidatedPortable $PortableDestination $ExpectedSourceCommit $ExpectedBridgeVersion)

    Write-Host "Portable Bridge runtime synchronized." -ForegroundColor Green
    Write-Host "Path:   $PortableDestination" -ForegroundColor DarkGray
}

if (-not [string]::IsNullOrWhiteSpace($SdkRoot)) {
    if ($ForceRebuild) { throw "-ForceRebuild cannot be combined with -SdkRoot." }
    if ([string]::IsNullOrWhiteSpace($PortableRoot)) {
        throw "-PortableRoot is required with -SdkRoot so Binary and Portable SDKs remain one coherent Bridge build."
    }

    $sdk = Copy-Sdk ([System.IO.Path]::GetFullPath($SdkRoot))
    Copy-Portable ([System.IO.Path]::GetFullPath($PortableRoot)) ([string]$sdk.Manifest.sourceCommit) ([string]$sdk.Contract.bridgeVersion)
    exit 0
}
if (-not [string]::IsNullOrWhiteSpace($PortableRoot)) {
    throw "-PortableRoot is only valid together with -SdkRoot."
}

if (-not (Test-Path -LiteralPath $Destination -PathType Container) -and
    (Test-Path -LiteralPath $LegacyDestination -PathType Container)) {
    New-Item -ItemType Directory -Path $BridgeRoot -Force | Out-Null
    Move-Item -LiteralPath $LegacyDestination -Destination $Destination
    Write-Host "[sync] Migrated legacy dist/win-x64 to external/OcctCSharpBridge/win-x64." -ForegroundColor DarkGray
}
if (-not (Test-Path -LiteralPath $PortableDestination -PathType Container) -and
    (Test-Path -LiteralPath $LegacyPortableDestination -PathType Container)) {
    New-Item -ItemType Directory -Path (Split-Path -Parent $PortableDestination) -Force | Out-Null
    Move-Item -LiteralPath $LegacyPortableDestination -Destination $PortableDestination
    Write-Host "[sync] Migrated legacy dist/portable/win-x64 to external/OcctCSharpBridge/portable/win-x64." -ForegroundColor DarkGray
}

if ($ForceRebuild) {
    throw "-ForceRebuild is no longer supported. Demo sync never builds the Bridge SDK; build/package Bridge separately, then pass -SdkRoot and -PortableRoot."
}

if ($null -eq (Get-Command git -ErrorAction SilentlyContinue)) { throw "git was not found in PATH." }

if ($Remote -eq "." -or $Remote -eq "local") {
    $sourceCommit = ([string](& git -C $RepoRoot rev-parse $SourceBranch)).Trim()
}
else {
    Write-Host "[sync] Fetching $Remote/$SourceBranch metadata..." -ForegroundColor Cyan
    & git -C $RepoRoot fetch --quiet $Remote $SourceBranch
    if ($LASTEXITCODE -ne 0) { throw "Unable to fetch $Remote/$SourceBranch." }
    $sourceCommit = ([string](& git -C $RepoRoot rev-parse "$Remote/$SourceBranch")).Trim()
}
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sourceCommit)) {
    throw "Unable to resolve $Remote/$SourceBranch."
}

if (-not (Test-Path -LiteralPath $Destination -PathType Container) -or
    -not (Test-Path -LiteralPath $PortableDestination -PathType Container)) {
    throw "Demo SDK cache is missing under external/OcctCSharpBridge. sync.ps1 no longer compiles Bridge. Build/package Bridge separately and run .\sync.ps1 -SdkRoot <binary-sdk> -PortableRoot <portable-sdk>."
}

$sdk = Read-ValidatedSdk $Destination
if ([string]$sdk.Manifest.sourceCommit -ne $sourceCommit) {
    throw "Demo SDK cache is stale. Expected $Remote/$SourceBranch @ $sourceCommit, found $($sdk.Manifest.sourceCommit). Provide matching prebuilt artifacts with -SdkRoot and -PortableRoot."
}
[void](Read-ValidatedPortable $PortableDestination $sourceCommit ([string]$sdk.Contract.bridgeVersion))

Write-Host "Binary SDK and Portable SDK already match $Remote/$SourceBranch @ $($sourceCommit.Substring(0, 7))." -ForegroundColor Green
Write-Host "[sync] Validation completed; no Bridge build or smoke test was executed." -ForegroundColor Green

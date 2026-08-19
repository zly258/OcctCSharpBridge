param(
    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$Remote = "origin",
    [string]$OutputDirectory = "",
    [switch]$Zip
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$BuildScript = Join-Path $RepoRoot "build.ps1"
$PortablePackScript = Join-Path $RepoRoot "tools\package-portable-sdk.ps1"
$ContractPath = Join-Path $RepoRoot "bridge-contract.json"
$StableApiCheck = Join-Path $RepoRoot "tests\check-stable-api-compatibility.ps1"
$RuntimeSmokeProject = Join-Path $RepoRoot "tests\OcctNet.RuntimeSmoke\OcctNet.RuntimeSmoke.csproj"
$NativeDll = Join-Path $RepoRoot "build\native\bin\Release\OcctNative.dll"
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"
if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $RepoRoot "artifacts\publish" }
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required path was not found: $Path" }
}

function Get-CommandPath {
    param([Parameter(Mandatory = $true)][string]$Name)
    $command = Get-Command -Name $Name -ErrorAction SilentlyContinue
    if ($null -eq $command) { throw "$Name was not found in PATH." }
    return [string]$command.Source
}

function Invoke-Git {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)
    & git -C $RepoRoot @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Get-CurrentBranch {
    $output = @(& git -C $RepoRoot rev-parse --abbrev-ref HEAD 2>$null)
    if ($LASTEXITCODE -ne 0 -or $output.Count -ne 1) {
        throw "Failed to resolve the current Git branch."
    }

    $branch = [string]$output[0]
    if ([string]::IsNullOrWhiteSpace($branch) -or $branch -eq "HEAD") {
        throw "publish.ps1 must be run from a named branch, not detached HEAD."
    }
    return $branch.Trim()
}

function Get-WorktreeChanges {
    $changes = @(& git -C $RepoRoot status --porcelain --untracked-files=all)
    if ($LASTEXITCODE -ne 0) { throw "Failed to inspect the Git working tree." }
    return $changes
}

function Assert-CleanWorktree {
    param([Parameter(Mandatory = $true)][string]$Stage)
    $changes = @(Get-WorktreeChanges)
    if ($changes.Count -gt 0) {
        throw "The working tree must be clean $Stage. Review or commit changes through the normal PR workflow first."
    }
}

function Assert-RemoteBranchAncestor {
    param([Parameter(Mandatory = $true)][string]$Branch)

    Invoke-Git @("fetch", "--quiet", $Remote, $Branch)
    $remoteRef = "$Remote/$Branch"

    & git -C $RepoRoot merge-base --is-ancestor $remoteRef HEAD
    $ancestorExitCode = $LASTEXITCODE
    if ($ancestorExitCode -eq 0) { return }
    if ($ancestorExitCode -ne 1) { throw "Failed to compare HEAD with $remoteRef." }

    $counts = @(& git -C $RepoRoot rev-list --left-right --count "$remoteRef...HEAD")
    if ($LASTEXITCODE -ne 0 -or $counts.Count -ne 1) {
        throw "Local $Branch is not based on the latest $remoteRef."
    }

    $parts = @(([string]$counts[0]) -split '\s+' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($parts.Count -ne 2) { throw "Unable to compare local $Branch with $remoteRef." }
    throw "Local $Branch is stale or diverged from $remoteRef (remote-only: $($parts[0]), local-only: $($parts[1])). Synchronize $Branch before publishing."
}

function Invoke-Build {
    param([Parameter(Mandatory = $true)][string]$Target)
    & $BuildScript -Target $Target -Configuration "Release" -OcctRoot $OcctRoot
    if ($LASTEXITCODE -ne 0) {
        throw "build.ps1 $Target failed with exit code $LASTEXITCODE."
    }
}

function Test-BinarySdk {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$ExpectedSourceCommit
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

    $contract = Get-Content (Join-Path $Path "bridge-contract.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $manifest = Get-Content (Join-Path $Path "bridge-manifest.json") -Raw -Encoding UTF8 | ConvertFrom-Json

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

    if ([string]$manifest.sourceCommit -ne $ExpectedSourceCommit) {
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
    if (@($manifestNames | Group-Object | Where-Object Count -ne 1).Count -gt 0) {
        throw "Binary SDK manifest contains duplicate file entries."
    }

    foreach ($name in $expectedHashedFiles) {
        if ($name -notin $manifestNames) { throw "Binary SDK manifest does not hash required file: $name" }
    }

    foreach ($entry in $entries) {
        $name = [string]$entry.name
        if ([string]::IsNullOrWhiteSpace($name) -or $name.Contains('/') -or $name.Contains('\')) {
            throw "Invalid Binary SDK manifest file name: $name"
        }
        $file = Join-Path $Path $name
        if (-not (Test-Path $file -PathType Leaf)) { throw "Manifest file is missing: $name" }
        $actualHash = (Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actualHash -ne ([string]$entry.sha256).ToLowerInvariant()) {
            throw "Binary SDK hash mismatch: $name"
        }
    }
}

function Assert-OnlyDistChanges {
    $changes = @(Get-WorktreeChanges)
    foreach ($change in $changes) {
        $path = if ($change.Length -gt 3) { $change.Substring(3).Replace('\', '/') } else { "" }
        if (-not $path.StartsWith("dist/win-x64/", [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Publishing produced an unexpected worktree change outside dist/win-x64: $change"
        }
    }
}

function Assert-RunningWindowsX64 {
    $runningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [System.Runtime.InteropServices.OSPlatform]::Windows)
    if (-not [Environment]::Is64BitProcess -or -not $runningOnWindows) {
        throw "Stable prebuilt release validation must run in a Windows x64 PowerShell process."
    }
}

function Assert-StableContract {
    param([Parameter(Mandatory = $true)]$Contract)

    $version = [string]$Contract.bridgeVersion
    if ([string]::IsNullOrWhiteSpace($version) -or $version.Contains('-')) {
        throw "Stable release gate requires a non-prerelease Bridge version; found '$version'."
    }
    if ([string]$Contract.release.channel -ne "stable") {
        throw "bridge-contract.json release.channel must be 'stable'."
    }
    if ([string]::IsNullOrWhiteSpace([string]$Contract.release.apiBaselineCommit)) {
        throw "Stable release contract must define release.apiBaselineCommit."
    }
    if ([int]$Contract.nativeAbi.current -ne 5 -or [int]$Contract.nativeAbi.minimumSupported -ne 5) {
        throw "Stable Bridge 3 requires ABI 5 as both current and minimum supported ABI."
    }

    $prebuilt = @($Contract.distribution.officialPrebuiltPlatforms | ForEach-Object { [string]$_ })
    if ($prebuilt.Count -ne 1 -or $prebuilt[0] -ne "windows-x64") {
        throw "Stable 3.x officially publishes prebuilt SDKs for windows-x64 only."
    }

    $sourceBuild = @($Contract.distribution.sourceBuildPlatforms | ForEach-Object { [string]$_ })
    if ($sourceBuild.Count -ne 2) {
        throw "Stable source-build platform contract must contain exactly windows-x64 and linux-x64."
    }
    foreach ($platform in @("windows-x64", "linux-x64")) {
        if ($platform -notin $sourceBuild) {
            throw "Stable source-build platform contract is missing '$platform'."
        }
    }
    if ([bool]$Contract.distribution.linuxPrebuiltRelease) {
        throw "Stable 3.x must not advertise an official Linux prebuilt release."
    }
}

function Get-OcctRuntimeDirectories {
    $directories = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $occtBin = Join-Path $OcctRoot "win64\vc14\bin"
    Assert-Path $occtBin
    [void]$directories.Add($occtBin)

    $thirdPartyRoot = Join-Path $OcctRoot "3rdparty-vc14-64"
    if (Test-Path -LiteralPath $thirdPartyRoot -PathType Container) {
        foreach ($dll in @(Get-ChildItem -LiteralPath $thirdPartyRoot -Filter "*.dll" -File -Recurse -ErrorAction SilentlyContinue)) {
            [void]$directories.Add($dll.DirectoryName)
        }
    }
    return @($directories)
}

function Get-InstalledRuntimeMajors {
    param([Parameter(Mandatory = $true)][string]$DotNet)

    $majors = [System.Collections.Generic.HashSet[int]]::new()
    $lines = @(& $DotNet --list-runtimes)
    if ($LASTEXITCODE -ne 0) { throw "dotnet --list-runtimes failed." }
    foreach ($line in $lines) {
        if ([string]$line -match '^Microsoft\.NETCore\.App\s+(\d+)\.') {
            [void]$majors.Add([int]$Matches[1])
        }
    }
    return @($majors)
}

function Invoke-RuntimeMatrix {
    param(
        [Parameter(Mandatory = $true)][string]$DotNet,
        [Parameter(Mandatory = $true)][string]$BridgeVersion
    )

    $installed = @(Get-InstalledRuntimeMajors $DotNet)
    foreach ($major in 8, 9, 10) {
        if ($major -notin $installed) {
            throw ".NET $major runtime is required for the Stable runtime matrix. Install Microsoft.NETCore.App $major.x x64 before releasing."
        }
    }

    Write-Host "[stable] Building the real .NET 8/9/10 native runtime smoke..." -ForegroundColor Cyan
    & $DotNet build $RuntimeSmokeProject -c Release -p:Platform=x64 -p:Version=$BridgeVersion --nologo
    if ($LASTEXITCODE -ne 0) { throw "Runtime smoke build failed." }
    Assert-Path $NativeDll

    $runtimeDirectories = @(Get-OcctRuntimeDirectories)
    $previousPath = $env:PATH
    $previousNativeDirectory = $env:OCCT_BRIDGE_NATIVE_DIR
    $previousOcctRoot = $env:OCCT_ROOT
    $previousCasRoot = $env:CASROOT
    $previousRollForward = $env:DOTNET_ROLL_FORWARD
    $previousExpectedRuntime = $env:OCCT_EXPECTED_RUNTIME_MAJOR

    try {
        foreach ($major in 8, 9, 10) {
            $framework = "net$major.0"
            $output = Join-Path (Split-Path -Parent $RuntimeSmokeProject) "bin\x64\Release\$framework"
            Assert-Path $output
            Copy-Item -LiteralPath $NativeDll -Destination (Join-Path $output "OcctNative.dll") -Force

            $env:PATH = (@($output) + $runtimeDirectories + @($previousPath)) -join [System.IO.Path]::PathSeparator
            $env:OCCT_BRIDGE_NATIVE_DIR = $output
            $env:OCCT_ROOT = $OcctRoot
            $env:CASROOT = $OcctRoot
            $env:DOTNET_ROLL_FORWARD = "LatestPatch"
            $env:OCCT_EXPECTED_RUNTIME_MAJOR = [string]$major

            $runtimeDll = Join-Path $output "OcctNet.RuntimeSmoke.dll"
            Assert-Path $runtimeDll
            Write-Host "[stable] Running Native smoke on actual .NET $major runtime..." -ForegroundColor Cyan
            & $DotNet $runtimeDll
            if ($LASTEXITCODE -ne 0) { throw ".NET $major native runtime smoke failed." }
        }
    }
    finally {
        $env:PATH = $previousPath
        $env:OCCT_BRIDGE_NATIVE_DIR = $previousNativeDirectory
        $env:OCCT_ROOT = $previousOcctRoot
        $env:CASROOT = $previousCasRoot
        $env:DOTNET_ROLL_FORWARD = $previousRollForward
        $env:OCCT_EXPECTED_RUNTIME_MAJOR = $previousExpectedRuntime
    }
}

function Invoke-IsolatedPortableSmoke {
    param(
        [Parameter(Mandatory = $true)][string]$DotNet,
        [Parameter(Mandatory = $true)][string]$BridgeVersion
    )

    $archive = Join-Path $OutputDirectory "OcctCSharpBridge-$BridgeVersion-win-x64-portable.zip"
    Assert-Path $archive

    $sourceOutput = Join-Path (Split-Path -Parent $RuntimeSmokeProject) "bin\x64\Release\net8.0"
    $testPayload = @(
        "OcctNet.RuntimeSmoke.dll",
        "OcctNet.RuntimeSmoke.deps.json",
        "OcctNet.RuntimeSmoke.runtimeconfig.json"
    )
    foreach ($name in $testPayload) { Assert-Path (Join-Path $sourceOutput $name) }

    $isolationRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("OcctCSharpBridge-stable-" + [Guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Path $isolationRoot -Force | Out-Null

    $environmentNames = @(
        "OCCT_BRIDGE_NATIVE_DIR", "OCCT_ROOT", "CASROOT",
        "CSF_OCCTResourcePath", "CSF_SHMessage", "CSF_XSMessage",
        "CSF_StandardDefaults", "CSF_PluginDefaults", "CSF_IGESDefaults",
        "CSF_STEPDefaults", "CSF_ShadersDirectory", "CSF_MDTVTexturesDirectory",
        "CSF_UnitsLexicon", "CSF_UnitsDefinition",
        "DOTNET_ROLL_FORWARD", "OCCT_EXPECTED_RUNTIME_MAJOR"
    )
    $savedEnvironment = @{}
    foreach ($name in $environmentNames) { $savedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name) }
    $previousPath = $env:PATH

    try {
        Expand-Archive -LiteralPath $archive -DestinationPath $isolationRoot -Force
        foreach ($name in $testPayload) {
            Copy-Item -LiteralPath (Join-Path $sourceOutput $name) -Destination (Join-Path $isolationRoot $name) -Force
        }

        foreach ($required in @(
            "OcctNet.dll",
            "runtime\OcctNative.dll",
            "occt\resources",
            "bridge-contract.json",
            "bridge-manifest.json",
            "package-manifest.json"
        )) { Assert-Path (Join-Path $isolationRoot $required) }

        foreach ($name in $environmentNames) { [Environment]::SetEnvironmentVariable($name, $null) }
        $safePathEntries = @(
            ($previousPath -split [System.IO.Path]::PathSeparator) |
            Where-Object {
                -not [string]::IsNullOrWhiteSpace($_) -and
                -not $_.StartsWith($OcctRoot, [System.StringComparison]::OrdinalIgnoreCase) -and
                -not $_.StartsWith($RepoRoot, [System.StringComparison]::OrdinalIgnoreCase)
            }
        )
        $env:PATH = $safePathEntries -join [System.IO.Path]::PathSeparator
        $env:DOTNET_ROLL_FORWARD = "LatestPatch"
        $env:OCCT_EXPECTED_RUNTIME_MAJOR = "8"

        Write-Host "[stable] Running extracted Portable SDK smoke without development OCCT paths..." -ForegroundColor Cyan
        Push-Location $isolationRoot
        try {
            & $DotNet (Join-Path $isolationRoot "OcctNet.RuntimeSmoke.dll")
            if ($LASTEXITCODE -ne 0) { throw "Extracted Portable SDK smoke failed." }
        }
        finally {
            Pop-Location
        }
    }
    finally {
        $env:PATH = $previousPath
        foreach ($name in $environmentNames) {
            [Environment]::SetEnvironmentVariable($name, $savedEnvironment[$name])
        }
        Remove-Item -LiteralPath $isolationRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Assert-Path $BuildScript
Assert-Path $PortablePackScript
Assert-Path $ContractPath
$null = Get-CommandPath "git"

$currentBranch = Get-CurrentBranch
if ($currentBranch -notin @("main", "main-dev")) {
    throw "publish.ps1 validates publishing from main or main-dev only. Current branch: $currentBranch"
}
$publishMode = if ($currentBranch -eq "main") { "Formal" } else { "Development" }
Assert-CleanWorktree "before publishing"
Assert-RemoteBranchAncestor $currentBranch
Write-Host "[publish] $publishMode $currentBranch ancestry validated." -ForegroundColor DarkGray

$sourceCommit = (& git -C $RepoRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sourceCommit)) {
    throw "Failed to resolve the source commit used for Binary SDK publishing."
}

$sourceContract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$runStableValidation = [string]$sourceContract.release.channel -eq "stable"
$dotnet = $null
if ($runStableValidation) {
    Assert-RunningWindowsX64
    Assert-StableContract -Contract $sourceContract
    Assert-Path $StableApiCheck
    Assert-Path $RuntimeSmokeProject
    Assert-Path $OcctRoot
    $dotnet = Get-CommandPath "dotnet"

    Write-Host "[stable] OcctCSharpBridge $($sourceContract.bridgeVersion) Windows x64 Stable gate" -ForegroundColor Green
    Write-Host "[stable] Checking the frozen 3.x public API/ABI baseline..." -ForegroundColor Cyan
    & $StableApiCheck -RepositoryRoot $RepoRoot
    if (-not $?) { throw "Stable API compatibility check failed." }
}

Write-Host "[publish] Running the complete Release SDK gate before Binary SDK validation..." -ForegroundColor Cyan
Invoke-Build "sdk"
Test-BinarySdk -Path $DistRoot -ExpectedSourceCommit $sourceCommit
Assert-OnlyDistChanges

$effectiveZip = $Zip.IsPresent -or $runStableValidation
Write-Host "[publish] Building portable SDK with the OCCT runtime closure..." -ForegroundColor Cyan
& $PortablePackScript `
    -SdkRoot $DistRoot `
    -OcctRoot $OcctRoot `
    -OutputDirectory $OutputDirectory `
    -Zip:$effectiveZip
if ($LASTEXITCODE -ne 0) { throw "Portable Windows SDK packaging failed with exit code $LASTEXITCODE." }

if ($runStableValidation) {
    Invoke-RuntimeMatrix -DotNet $dotnet -BridgeVersion ([string]$sourceContract.bridgeVersion)
    Invoke-IsolatedPortableSmoke -DotNet $dotnet -BridgeVersion ([string]$sourceContract.bridgeVersion)

    Write-Host "Stable release validation completed successfully." -ForegroundColor Green
    Write-Host "Version:        $($sourceContract.bridgeVersion)" -ForegroundColor DarkGray
    Write-Host "Prebuilt:       windows-x64" -ForegroundColor DarkGray
    Write-Host "API baseline:   $($sourceContract.release.apiBaselineCommit)" -ForegroundColor DarkGray
    Write-Host "Runtime matrix: .NET 8 / 9 / 10 native execution" -ForegroundColor DarkGray
    Write-Host "Portable smoke: isolated extracted Windows package" -ForegroundColor DarkGray
}
else {
    Write-Host "Bridge Binary SDK and portable runtime SDK validated successfully." -ForegroundColor Green
}

Write-Host "Mode:       $publishMode" -ForegroundColor DarkGray
Write-Host "Branch:     $currentBranch" -ForegroundColor DarkGray
Write-Host "Source:     $sourceCommit" -ForegroundColor DarkGray
Write-Host "Binary SDK: $DistRoot" -ForegroundColor DarkGray
Write-Host "Portable:   $OutputDirectory" -ForegroundColor DarkGray
Write-Host "No Git commit or push was performed. Publish the portable package through the normal reviewed artifact workflow." -ForegroundColor Cyan

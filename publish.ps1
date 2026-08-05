param(
    [Parameter(Position = 0)]
    [ValidateSet("all", "winform", "wpf")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $(if ($env:OCCT_ROOT) { $env:OCCT_ROOT } else { "D:\tools\occt-vc144-64" }),

    [string]$OutputDirectory = "",

    [switch]$FrameworkDependent,

    [switch]$Zip,

    [switch]$KeepExisting
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
$env:DOTNET_CLI_UI_LANGUAGE = "en-US"
$env:VSLANG = "1033"

if (Test-Path "$env:SystemRoot\System32\chcp.com") {
    & "$env:SystemRoot\System32\chcp.com" 65001 | Out-Null
}

$Target = $Target.ToLowerInvariant()
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $RepoRoot "artifacts\publish"
}
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)

$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"
$NativeDll = Join-Path $RepoRoot "build\native\bin\$Configuration\OcctNative.dll"
$PackageName = "OcctCSharpBridge-Demo-win-x64"
$PackageRoot = Join-Path $OutputDirectory $PackageName
$AppsRoot = Join-Path $PackageRoot "apps"
$RuntimeRoot = Join-Path $PackageRoot "runtime"
$OcctPackageRoot = Join-Path $PackageRoot "occt"
$LicenseRoot = Join-Path $PackageRoot "licenses"

$Projects = [ordered]@{
    winform = @{
        Name = "WinForms"
        Project = "src\CadWinForms\CadWinForms.csproj"
        Folder = "winform"
        Executable = "CAD-Winform.exe"
        Launcher = "Start-WinForms.cmd"
    }
    wpf = @{
        Name = "WPF"
        Project = "src\CadWpf\CadWpf.csproj"
        Folder = "wpf"
        Executable = "CAD-WPF.exe"
        Launcher = "Start-WPF.cmd"
    }
}

function Assert-Path {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [string]$Description = "Required path"
    )

    if (-not (Test-Path $Path)) {
        throw "$Description was not found: $Path"
    }
}

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "$Name was not found in PATH."
    }
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )

    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw $ErrorMessage
    }
}

function Get-FileHashValue {
    param([Parameter(Mandatory = $true)][string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
}

function Copy-RuntimeDll {
    param(
        [Parameter(Mandatory = $true)][System.IO.FileInfo]$Source,
        [Parameter(Mandatory = $true)][string]$Category
    )

    $destination = Join-Path $RuntimeRoot $Source.Name
    if (Test-Path $destination -PathType Leaf) {
        $sourceHash = Get-FileHashValue $Source.FullName
        $destinationHash = Get-FileHashValue $destination
        if ($sourceHash -eq $destinationHash) {
            return
        }
        throw "Conflicting runtime DLL '$($Source.Name)' was found while copying $Category.`nExisting: $destination`nIncoming: $($Source.FullName)"
    }

    Copy-Item -LiteralPath $Source.FullName -Destination $destination -Force
}

function Publish-Application {
    param([Parameter(Mandatory = $true)][string]$Key)

    $application = $Projects[$Key]
    $projectPath = Join-Path $RepoRoot $application.Project
    $destination = Join-Path $AppsRoot $application.Folder
    Assert-Path $projectPath "$($application.Name) project"

    if (Test-Path $destination) {
        Remove-Item $destination -Recurse -Force
    }
    New-Item $destination -ItemType Directory -Force | Out-Null

    $arguments = @(
        "publish", $projectPath,
        "-c", $Configuration,
        "-r", "win-x64",
        "-p:Platform=x64",
        "-p:PublishSingleFile=false",
        "-p:PublishReadyToRun=false",
        "-p:DebugType=None",
        "-p:DebugSymbols=false",
        "--self-contained", $(if ($FrameworkDependent) { "false" } else { "true" }),
        "--nologo",
        "-o", $destination
    )

    Write-Host "[publish] $($application.Name)..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" $arguments "$($application.Name) publish failed."
    Assert-Path (Join-Path $destination $application.Executable) "$($application.Name) executable"
}

function Copy-OcctRuntime {
    Assert-Path $OcctBinDir "OCCT runtime directory"

    Write-Host "[runtime] Copying OCCT DLLs..." -ForegroundColor Cyan
    Get-ChildItem $OcctBinDir -File -Filter "*.dll" | Sort-Object Name | ForEach-Object {
        Copy-RuntimeDll $_ "OCCT runtime"
    }

    if (Test-Path $OcctThirdPartyDir -PathType Container) {
        Write-Host "[runtime] Copying detected third-party DLLs..." -ForegroundColor Cyan
        $candidates = Get-ChildItem $OcctThirdPartyDir -Recurse -File -Filter "*.dll" | Where-Object {
            if ($Configuration -eq "Debug") {
                return $true
            }
            $path = $_.FullName.ToLowerInvariant()
            return $path -notmatch "[\\/]debug[\\/]" -and
                   $_.Name -notmatch "(?i)(_debug|debug|d)\.dll$"
        }

        $candidates | Sort-Object FullName | ForEach-Object {
            Copy-RuntimeDll $_ "OCCT third-party runtime"
        }
    }
}

function Copy-VisualCppRuntime {
    Write-Host "[runtime] Copying available Visual C++ runtime DLLs..." -ForegroundColor Cyan
    $names = @(
        "concrt140.dll",
        "msvcp140.dll",
        "msvcp140_1.dll",
        "msvcp140_2.dll",
        "msvcp140_atomic_wait.dll",
        "msvcp140_codecvt_ids.dll",
        "vcomp140.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll"
    )

    $systemDirectory = [Environment]::SystemDirectory
    foreach ($name in $names) {
        $candidate = Join-Path $systemDirectory $name
        if (Test-Path $candidate -PathType Leaf) {
            Copy-RuntimeDll (Get-Item $candidate) "Visual C++ runtime"
        }
    }
}

function Copy-OcctResources {
    $sourceRoot = Join-Path $OcctRoot "src"
    if (-not (Test-Path $sourceRoot -PathType Container)) {
        Write-Warning "OCCT resource source directory was not found: $sourceRoot"
        return
    }

    $resourceNames = @(
        "Shaders",
        "Textures",
        "StdResource",
        "UnitsAPI",
        "SHMessage",
        "XSMessage",
        "XSTEPResource",
        "XmlOcafResource",
        "TObj"
    )

    $destinationRoot = Join-Path $OcctPackageRoot "src"
    New-Item $destinationRoot -ItemType Directory -Force | Out-Null

    Write-Host "[resources] Copying OCCT resource directories..." -ForegroundColor Cyan
    foreach ($name in $resourceNames) {
        $source = Join-Path $sourceRoot $name
        if (Test-Path $source -PathType Container) {
            Copy-Item $source (Join-Path $destinationRoot $name) -Recurse -Force
        }
    }
}

function Copy-LicenseFiles {
    New-Item $LicenseRoot -ItemType Directory -Force | Out-Null

    $projectLicense = Join-Path $RepoRoot "LICENSE"
    if (Test-Path $projectLicense -PathType Leaf) {
        Copy-Item $projectLicense (Join-Path $LicenseRoot "OcctCSharpBridge-LICENSE.txt") -Force
    }

    $occtLicenseRoot = Join-Path $LicenseRoot "occt"
    New-Item $occtLicenseRoot -ItemType Directory -Force | Out-Null
    Get-ChildItem $OcctRoot -File -ErrorAction SilentlyContinue | Where-Object {
        $_.Name -match "(?i)license|copying|notice|exception"
    } | ForEach-Object {
        Copy-Item $_.FullName (Join-Path $occtLicenseRoot $_.Name) -Force
    }

    if (Test-Path $OcctThirdPartyDir -PathType Container) {
        $thirdPartyLicenseRoot = Join-Path $LicenseRoot "thirdparty"
        New-Item $thirdPartyLicenseRoot -ItemType Directory -Force | Out-Null
        Get-ChildItem $OcctThirdPartyDir -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
            $_.Name -match "(?i)^(license|copying|notice|readme)(\.|$)"
        } | ForEach-Object {
            $relative = $_.FullName.Substring($OcctThirdPartyDir.Length).TrimStart('\', '/')
            $safeName = $relative -replace '[\\/:*?"<>|]', '_'
            Copy-Item $_.FullName (Join-Path $thirdPartyLicenseRoot $safeName) -Force
        }
    }
}

function Write-Launcher {
    param([Parameter(Mandatory = $true)][string]$Key)

    $application = $Projects[$Key]
    $launcherPath = Join-Path $PackageRoot $application.Launcher
    $content = @"
@echo off
setlocal
set "PACKAGE_ROOT=%~dp0"
set "OCCT_ROOT=%~dp0occt"
set "CASROOT=%~dp0occt"
set "OCCT_BRIDGE_NATIVE_DIR=%~dp0runtime"
set "PATH=%~dp0runtime;%PATH%"
pushd "%~dp0apps\$($application.Folder)"
start "" "$($application.Executable)"
popd
endlocal
"@
    [System.IO.File]::WriteAllText($launcherPath, $content, $utf8)
}

function Write-PackageReadme {
    $mode = if ($FrameworkDependent) {
        "Framework-dependent: install the matching .NET 8 Desktop Runtime on the target computer."
    }
    else {
        "Self-contained: the .NET runtime is included."
    }

    $lines = @(
        "OcctCSharpBridge WinForms/WPF Demo",
        "=================================",
        "",
        "Platform: Windows x64",
        "OCCT: 7.9.0",
        $mode,
        "",
        "Run the generated Start-WinForms.cmd or Start-WPF.cmd file.",
        "Do not move only the EXE; keep the package directory structure together.",
        "",
        "The launchers configure PATH, OCCT_BRIDGE_NATIVE_DIR, OCCT_ROOT and CASROOT",
        "relative to the extracted package. No OCCT SDK configuration is required on the target computer.",
        "",
        "Before redistribution, review all license files in the licenses directory."
    )
    [System.IO.File]::WriteAllLines((Join-Path $PackageRoot "README.txt"), $lines, $utf8)
}

function Write-Manifest {
    $manifestPath = Join-Path $PackageRoot "runtime-manifest.txt"
    $header = @(
        "Package=$PackageName",
        "GeneratedUtc=$([DateTime]::UtcNow.ToString('O'))",
        "Configuration=$Configuration",
        "Target=$Target",
        "RuntimeIdentifier=win-x64",
        "SelfContained=$(-not $FrameworkDependent)",
        "OcctRootSource=$OcctRoot",
        ""
    )

    $entries = Get-ChildItem $PackageRoot -Recurse -File | Sort-Object FullName | ForEach-Object {
        $relative = $_.FullName.Substring($PackageRoot.Length).TrimStart('\', '/')
        $version = ""
        if ($_.Extension -match "(?i)^\.(dll|exe)$") {
            try {
                $version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($_.FullName).FileVersion
            }
            catch {
                $version = ""
            }
        }
        $hash = Get-FileHashValue $_.FullName
        "{0}`t{1}`t{2}`t{3}" -f $relative, $_.Length, $version, $hash
    }

    [System.IO.File]::WriteAllLines($manifestPath, @($header + "RelativePath`tBytes`tFileVersion`tSHA256" + $entries), $utf8)
}

Assert-Command "dotnet"
Assert-Path (Join-Path $RepoRoot "build.ps1") "Build script"
Assert-Path $OcctRoot "OCCT root"
Assert-Path $OcctBinDir "OCCT runtime directory"

Write-Host "Target:             $Target"
Write-Host "Configuration:      $Configuration"
Write-Host "OCCT root:          $OcctRoot"
Write-Host "Output directory:   $OutputDirectory"
Write-Host "Self-contained:     $(-not $FrameworkDependent)"

if ((Test-Path $PackageRoot) -and -not $KeepExisting) {
    Remove-Item $PackageRoot -Recurse -Force
}
New-Item $AppsRoot -ItemType Directory -Force | Out-Null
New-Item $RuntimeRoot -ItemType Directory -Force | Out-Null
New-Item $OcctPackageRoot -ItemType Directory -Force | Out-Null
New-Item $LicenseRoot -ItemType Directory -Force | Out-Null

Write-Host "[build] Building the native bridge..." -ForegroundColor Cyan
& (Join-Path $RepoRoot "build.ps1") native $Configuration -OcctRoot $OcctRoot
if (-not $?) {
    throw "Native bridge build failed."
}
Assert-Path $NativeDll "OcctNative.dll"
Copy-Item $NativeDll (Join-Path $RuntimeRoot "OcctNative.dll") -Force

switch ($Target) {
    "winform" { Publish-Application "winform" }
    "wpf" { Publish-Application "wpf" }
    "all" {
        Publish-Application "winform"
        Publish-Application "wpf"
    }
}

Copy-OcctRuntime
Copy-VisualCppRuntime
Copy-OcctResources
Copy-LicenseFiles

if ($Target -in @("winform", "all")) {
    Write-Launcher "winform"
}
if ($Target -in @("wpf", "all")) {
    Write-Launcher "wpf"
}

Write-PackageReadme
Write-Manifest

if ($Zip) {
    $zipPath = Join-Path $OutputDirectory "$PackageName.zip"
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    Write-Host "[zip] Creating $zipPath..." -ForegroundColor Cyan
    Compress-Archive -Path (Join-Path $PackageRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "ZIP: $zipPath" -ForegroundColor Green
}

Write-Host "Package: $PackageRoot" -ForegroundColor Green
Write-Host "Publish completed." -ForegroundColor Green

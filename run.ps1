param(
    [Parameter(Position = 0, Mandatory = $true)]
    [ValidateSet("winform", "wpf", "avalonia")]
    [string]$Target,

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT,
    [int]$StartupTimeoutSeconds = 5
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"

if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    $OcctRoot = $DefaultOcctRoot
}
if (-not (Test-Path -LiteralPath $ContractPath -PathType Leaf)) {
    throw "Bridge Binary SDK contract was not found: $ContractPath. Publish dist/win-x64 from the main branch with main/publish.ps1."
}

& $BuildScript validate $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "Bridge Binary SDK validation failed."
}

$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$targetFramework = [string]$contract.dotnet.targetFramework
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"

function Add-PathEntry {
    param([Parameter(Mandatory = $true)][string]$Directory)

    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) { return }

    $fullDirectory = [System.IO.Path]::GetFullPath($Directory).TrimEnd('\')
    $current = [Environment]::GetEnvironmentVariable("PATH")
    if ($null -eq $current) { $current = "" }

    $alreadyPresent = $false
    foreach ($entry in $current.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        try {
            $normalized = [System.IO.Path]::GetFullPath($entry).TrimEnd('\')
            if ($normalized.Equals($fullDirectory, [System.StringComparison]::OrdinalIgnoreCase)) {
                $alreadyPresent = $true
                break
            }
        }
        catch {
            # Ignore malformed PATH entries owned by other applications.
        }
    }

    if (-not $alreadyPresent) {
        $env:PATH = if ([string]::IsNullOrEmpty($current)) { $fullDirectory } else { "$fullDirectory;$current" }
    }
}

if (-not (Test-Path -LiteralPath $OcctBinDir -PathType Container)) {
    throw "OCCT runtime directory was not found: $OcctBinDir"
}

$apps = @{
    winform = "src\OcctDemo.WinForms\bin\x64\$Configuration\$targetFramework\CAD-Winform.exe"
    wpf = "src\OcctDemo.Wpf\bin\x64\$Configuration\$targetFramework\CAD-WPF.exe"
    avalonia = "src\OcctDemo.Avalonia\bin\x64\$Configuration\$targetFramework\CAD-Avalonia.exe"
}

$executable = Join-Path $RepoRoot $apps[$Target.ToLowerInvariant()]
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) {
    throw "Executable was not found: $executable`nRun .\build.ps1 $Target $Configuration first."
}

$applicationDirectory = Split-Path -Parent $executable
$nativeBridge = Join-Path $applicationDirectory "OcctNative.dll"
if (-not (Test-Path -LiteralPath $nativeBridge -PathType Leaf)) {
    throw "OcctNative.dll was not copied beside the application: $nativeBridge"
}

$env:OCCT_ROOT = $OcctRoot
$env:CASROOT = $OcctRoot
$env:OCCT_BRIDGE_NATIVE_DIR = $applicationDirectory
Add-PathEntry $applicationDirectory
Add-PathEntry $OcctBinDir

if (Test-Path -LiteralPath $OcctThirdPartyDir -PathType Container) {
    Get-ChildItem -LiteralPath $OcctThirdPartyDir -Directory | Sort-Object Name | ForEach-Object {
        Add-PathEntry (Join-Path $_.FullName "bin")
        Add-PathEntry (Join-Path $_.FullName "bin\win64")
        Add-PathEntry (Join-Path $_.FullName "bin\x64")
    }
}

Write-Host "Application: $executable"
Write-Host "Bridge:      $($contract.bridgeVersion), ABI $($contract.nativeAbiVersion)" -ForegroundColor DarkGray
Write-Host "OCCT root:   $OcctRoot" -ForegroundColor DarkGray

$process = Start-Process -FilePath $executable -WorkingDirectory $applicationDirectory -PassThru
Write-Host "Process ID:  $($process.Id)"

if ($Target -eq "avalonia") {
    $deadline = [DateTime]::UtcNow.AddSeconds([Math]::Max(1, $StartupTimeoutSeconds))
    while ([DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 200
        $process.Refresh()
        if ($process.HasExited) {
            throw "avalonia exited during startup with code $($process.ExitCode)."
        }
        if ($process.MainWindowHandle -ne [IntPtr]::Zero) { break }
    }
    if ($process.MainWindowHandle -eq [IntPtr]::Zero) {
        try { Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue } catch { }
        throw "avalonia process did not create a visible main window within $StartupTimeoutSeconds second(s)."
    }
}

$process.WaitForExit()
$process.Refresh()
if ($process.ExitCode -ne 0) {
    throw "$Target exited with code $($process.ExitCode)."
}

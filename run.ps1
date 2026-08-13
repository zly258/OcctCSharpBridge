param(
    [Parameter(Position = 0)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT,
    [int]$StartupTimeoutSeconds = 10
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (-not $IsWindows) {
    throw "run.ps1 launches CAD-Avalonia on Windows x64. Use ./run.sh on Linux."
}

$RepoRoot = Split-Path -Parent $PSCommandPath
$BuildScript = Join-Path $RepoRoot "build.ps1"
$ContractPath = Join-Path $RepoRoot "bridge-contract.json"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"
$DemoTargetFramework = "net10.0"
$Executable = Join-Path $RepoRoot "src\OcctDemo.Avalonia\bin\x64\$Configuration\$DemoTargetFramework\CAD-Avalonia.exe"

if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }
if (-not (Test-Path -LiteralPath $ContractPath -PathType Leaf)) {
    throw "Bridge contract was not found: $ContractPath"
}

& $BuildScript -Target validate -Configuration $Configuration -OcctRoot $OcctRoot
if (-not $?) { throw "Bridge validation failed." }

if (-not (Test-Path -LiteralPath $Executable -PathType Leaf)) {
    throw "Executable was not found: $Executable`nRun .\build.ps1 $Configuration first."
}

$applicationDirectory = Split-Path -Parent $Executable
$nativeBridge = Join-Path $applicationDirectory "OcctNative.dll"
if (-not (Test-Path -LiteralPath $nativeBridge -PathType Leaf)) {
    throw "OcctNative.dll was not copied beside CAD-Avalonia.exe: $nativeBridge`nRun .\build.ps1 $Configuration first."
}

$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"

function Add-PathEntry {
    param([Parameter(Mandatory = $true)][string]$Directory)

    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) { return }
    $fullDirectory = [System.IO.Path]::GetFullPath($Directory).TrimEnd('\')
    $current = [Environment]::GetEnvironmentVariable("PATH")
    if ($null -eq $current) { $current = "" }

    foreach ($entry in $current.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        try {
            if ([System.IO.Path]::GetFullPath($entry).TrimEnd('\').Equals(
                $fullDirectory,
                [System.StringComparison]::OrdinalIgnoreCase)) {
                return
            }
        }
        catch {
            # Ignore malformed PATH entries owned by other applications.
        }
    }

    $env:PATH = if ([string]::IsNullOrEmpty($current)) { $fullDirectory } else { "$fullDirectory;$current" }
}

function Show-StartupDiagnostics {
    param([Parameter(Mandatory = $true)][string]$ApplicationDirectory)

    $traceLog = Join-Path $ApplicationDirectory "CAD-Avalonia.log"
    if (Test-Path -LiteralPath $traceLog -PathType Leaf) {
        Write-Host ""
        Write-Host "[diagnostics] CAD-Avalonia.log" -ForegroundColor Yellow
        Get-Content -LiteralPath $traceLog -Tail 80 -ErrorAction SilentlyContinue | ForEach-Object { Write-Host $_ }
    }

    $localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
    if ([string]::IsNullOrWhiteSpace($localAppData)) { return }
    $crashDirectory = Join-Path $localAppData "OcctCSharpBridge\Logs"
    if (-not (Test-Path -LiteralPath $crashDirectory -PathType Container)) { return }

    $crashLog = Get-ChildItem -LiteralPath $crashDirectory -Filter "CAD-Avalonia-*.log" -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($null -eq $crashLog) { return }

    Write-Host ""
    Write-Host "[diagnostics] $($crashLog.FullName)" -ForegroundColor Yellow
    Get-Content -LiteralPath $crashLog.FullName -Tail 120 -ErrorAction SilentlyContinue | ForEach-Object { Write-Host $_ }
}

if (-not (Test-Path -LiteralPath $OcctBinDir -PathType Container)) {
    throw "OCCT runtime directory was not found: $OcctBinDir"
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

Write-Host "Application: $Executable"
Write-Host "Bridge:      $($contract.bridgeVersion), ABI $($contract.nativeAbiVersion)" -ForegroundColor DarkGray
Write-Host "OCCT root:   $OcctRoot" -ForegroundColor DarkGray

$process = Start-Process -FilePath $Executable -WorkingDirectory $applicationDirectory -PassThru
Write-Host "Process ID:  $($process.Id)"

$deadline = [DateTime]::UtcNow.AddSeconds([Math]::Max(1, $StartupTimeoutSeconds))
$windowDetected = $false
while ([DateTime]::UtcNow -lt $deadline) {
    Start-Sleep -Milliseconds 200
    $process.Refresh()
    if ($process.HasExited) {
        $exitCode = $process.ExitCode
        Show-StartupDiagnostics -ApplicationDirectory $applicationDirectory
        throw "CAD-Avalonia exited during startup with code $exitCode."
    }
    if ($process.MainWindowHandle -ne [IntPtr]::Zero) {
        $windowDetected = $true
        break
    }
}

if ($windowDetected) {
    Write-Host "Avalonia main window detected." -ForegroundColor DarkGray
}
else {
    Write-Warning "CAD-Avalonia is still running after $StartupTimeoutSeconds second(s), but MainWindowHandle is not available yet."
}

$process.WaitForExit()
$process.Refresh()
if ($process.ExitCode -ne 0) {
    $exitCode = $process.ExitCode
    Show-StartupDiagnostics -ApplicationDirectory $applicationDirectory
    throw "CAD-Avalonia exited with code $exitCode."
}

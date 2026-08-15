param(
    [Parameter(Position = 0, Mandatory = $true)]
    [ValidateSet("winform", "wpf", "avalonia")]
    [string]$Target,

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT,
    [int]$StartupTimeoutSeconds = 10
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSCommandPath
$DistRoot = Join-Path $RepoRoot "dist\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"

if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }
if (-not (Test-Path -LiteralPath $ContractPath -PathType Leaf)) { throw "Bridge Binary SDK contract was not found: $ContractPath. Run .\sync.ps1 first." }

& $BuildScript validate $Configuration
if (-not $?) { throw "Bridge validation failed." }

$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json
$targetKey = $Target.ToLowerInvariant()
$apps = @{
    winform = @{
        Framework = [string]$contract.dotnet.desktopTargetFramework
        Path = "src\OcctDemo.WinForms\bin\x64\$Configuration\{0}\CAD-Winform.exe"
    }
    wpf = @{
        Framework = [string]$contract.dotnet.desktopTargetFramework
        Path = "src\OcctDemo.Wpf\bin\x64\$Configuration\{0}\CAD-WPF.exe"
    }
    avalonia = @{
        Framework = [string]$contract.dotnet.targetFramework
        Path = "src\OcctDemo.Avalonia\bin\x64\$Configuration\{0}\CAD-Avalonia.exe"
    }
}
$definition = $apps[$targetKey]
$executable = Join-Path $RepoRoot ([string]::Format([string]$definition.Path, [string]$definition.Framework))
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw "Executable was not found: $executable`nRun .\build.ps1 $targetKey $Configuration first." }

$applicationDirectory = Split-Path -Parent $executable
$nativeBridge = Join-Path $applicationDirectory "OcctNative.dll"
if (-not (Test-Path -LiteralPath $nativeBridge -PathType Leaf)) { throw "OcctNative.dll was not copied beside the application: $nativeBridge" }

$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"
if (-not (Test-Path -LiteralPath $OcctBinDir -PathType Container)) { throw "OCCT runtime directory was not found: $OcctBinDir" }

function Add-PathEntry {
    param([Parameter(Mandatory = $true)][string]$Directory)
    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) { return }
    $fullDirectory = [System.IO.Path]::GetFullPath($Directory).TrimEnd('\')
    $current = [Environment]::GetEnvironmentVariable("PATH")
    if ($null -eq $current) { $current = "" }
    foreach ($entry in $current.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        try {
            if ([System.IO.Path]::GetFullPath($entry).TrimEnd('\').Equals($fullDirectory, [System.StringComparison]::OrdinalIgnoreCase)) { return }
        }
        catch { }
    }
    $env:PATH = if ([string]::IsNullOrEmpty($current)) { $fullDirectory } else { "$fullDirectory;$current" }
}

function Show-StartupDiagnostics {
    param([Parameter(Mandatory = $true)][string]$ApplicationDirectory)
    if ($targetKey -ne "avalonia") { return }
    $traceLog = Join-Path $ApplicationDirectory "CAD-Avalonia.log"
    if (Test-Path -LiteralPath $traceLog -PathType Leaf) {
        Write-Host "[diagnostics] CAD-Avalonia.log" -ForegroundColor Yellow
        Get-Content -LiteralPath $traceLog -Tail 80 -ErrorAction SilentlyContinue | ForEach-Object { Write-Host $_ }
    }
    $localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
    if ([string]::IsNullOrWhiteSpace($localAppData)) { return }
    $crashDirectory = Join-Path $localAppData "OcctCSharpBridge\Logs"
    if (-not (Test-Path -LiteralPath $crashDirectory -PathType Container)) { return }
    $crashLog = Get-ChildItem -LiteralPath $crashDirectory -Filter "CAD-Avalonia-*.log" -File -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($null -ne $crashLog) {
        Write-Host "[diagnostics] $($crashLog.FullName)" -ForegroundColor Yellow
        Get-Content -LiteralPath $crashLog.FullName -Tail 120 -ErrorAction SilentlyContinue | ForEach-Object { Write-Host $_ }
    }
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
Write-Host "Bridge:      $($contract.bridgeVersion), ABI $($contract.nativeAbi.current)" -ForegroundColor DarkGray
Write-Host "OCCT root:   $OcctRoot" -ForegroundColor DarkGray

$process = Start-Process -FilePath $executable -WorkingDirectory $applicationDirectory -PassThru
Write-Host "Process ID:  $($process.Id)"

if ($targetKey -eq "avalonia") {
    $deadline = [DateTime]::UtcNow.AddSeconds([Math]::Max(1, $StartupTimeoutSeconds))
    while ([DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 200
        $process.Refresh()
        if ($process.HasExited) {
            $exitCode = $process.ExitCode
            Show-StartupDiagnostics $applicationDirectory
            throw "CAD-Avalonia exited during startup with code $exitCode."
        }
        if ($process.MainWindowHandle -ne [IntPtr]::Zero) { break }
    }
}

$process.WaitForExit()
$process.Refresh()
if ($process.ExitCode -ne 0) {
    $exitCode = $process.ExitCode
    Show-StartupDiagnostics $applicationDirectory
    throw "$Target exited with code $exitCode."
}

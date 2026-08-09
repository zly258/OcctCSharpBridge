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

$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
if (Test-Path "$env:SystemRoot\System32\chcp.com") {
    & "$env:SystemRoot\System32\chcp.com" 65001 | Out-Null
}

$Target = $Target.ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    throw "OCCT_ROOT is not configured. Pass -OcctRoot <path> or set the OCCT_ROOT environment variable."
}

$OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
$OcctBinDir = Join-Path $OcctRoot "win64\vc14\bin"
$OcctThirdPartyDir = Join-Path $OcctRoot "3rdparty-vc14-64"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

function Add-PathEntry {
    param([Parameter(Mandatory = $true)][string]$Directory)

    if (-not (Test-Path $Directory -PathType Container)) {
        return
    }

    $fullDirectory = [System.IO.Path]::GetFullPath($Directory).TrimEnd('\')
    $currentPath = [Environment]::GetEnvironmentVariable("PATH")
    if ($null -eq $currentPath) {
        $currentPath = ""
    }

    $alreadyPresent = $false
    foreach ($entry in $currentPath.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        try {
            $normalizedEntry = [System.IO.Path]::GetFullPath($entry).TrimEnd('\')
            if ($normalizedEntry.Equals($fullDirectory, [System.StringComparison]::OrdinalIgnoreCase)) {
                $alreadyPresent = $true
                break
            }
        }
        catch {
            # Ignore malformed PATH entries owned by other applications.
        }
    }

    if (-not $alreadyPresent) {
        $env:PATH = if ([string]::IsNullOrEmpty($currentPath)) { $fullDirectory } else { "$fullDirectory;$currentPath" }
    }
}

function Show-AvaloniaLog {
    param([Parameter(Mandatory = $true)][string]$LogPath)

    if (Test-Path $LogPath -PathType Leaf) {
        Write-Host ""
        Write-Host "----- CAD-Avalonia.log -----" -ForegroundColor Yellow
        Get-Content $LogPath -Encoding UTF8
        Write-Host "----- end log -----" -ForegroundColor Yellow
    }
    else {
        Write-Warning "CAD-Avalonia.log was not created: $LogPath"
    }
}

if (-not (Test-Path $OcctBinDir -PathType Container)) {
    throw "OCCT runtime directory was not found: $OcctBinDir"
}

$apps = @{
    winform = "src\OcctDemo.WinForms\bin\x64\$Configuration\net8.0-windows\CAD-Winform.exe"
    wpf = "src\OcctDemo.Wpf\bin\x64\$Configuration\net8.0-windows\CAD-WPF.exe"
    avalonia = "src\OcctDemo.Avalonia\bin\x64\$Configuration\net8.0-windows\CAD-Avalonia.exe"
}

$executable = Join-Path $RepoRoot $apps[$Target]
if (-not (Test-Path $executable -PathType Leaf)) {
    throw "Executable was not found: $executable`nRun: .\build.ps1 $Target $Configuration -OcctRoot `"$OcctRoot`""
}

$applicationDirectory = Split-Path -Parent $executable
$nativeBridge = Join-Path $applicationDirectory "OcctNative.dll"
if (-not (Test-Path $nativeBridge -PathType Leaf)) {
    throw "OcctNative.dll was not found beside the application: $nativeBridge"
}

$env:OCCT_ROOT = $OcctRoot
$env:CASROOT = $OcctRoot
$env:OCCT_BRIDGE_NATIVE_DIR = $applicationDirectory
Add-PathEntry $applicationDirectory
Add-PathEntry $OcctBinDir

if (Test-Path $OcctThirdPartyDir -PathType Container) {
    Get-ChildItem $OcctThirdPartyDir -Directory | Sort-Object Name | ForEach-Object {
        Add-PathEntry (Join-Path $_.FullName "bin")
        Add-PathEntry (Join-Path $_.FullName "bin\win64")
        Add-PathEntry (Join-Path $_.FullName "bin\x64")
    }
}

Write-Host "Application: $executable"
Write-Host "OCCT root:  $OcctRoot" -ForegroundColor DarkGray

$logPath = Join-Path $applicationDirectory "CAD-Avalonia.log"
if ($Target -eq "avalonia" -and (Test-Path $logPath -PathType Leaf)) {
    Remove-Item $logPath -Force -ErrorAction SilentlyContinue
}

$process = Start-Process -FilePath $executable -WorkingDirectory $applicationDirectory -PassThru
Write-Host "Process ID: $($process.Id)"

if ($Target -eq "avalonia") {
    $deadline = [DateTime]::UtcNow.AddSeconds([Math]::Max(1, $StartupTimeoutSeconds))
    $mainWindowHandle = [IntPtr]::Zero

    while ([DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 200
        $process.Refresh()

        if ($process.HasExited) {
            $exitCode = $process.ExitCode
            Write-Host "Process exited during startup. Exit code: $exitCode" -ForegroundColor Red
            Show-AvaloniaLog -LogPath $logPath
            throw "avalonia exited before creating a main window. Exit code: $exitCode"
        }

        $mainWindowHandle = $process.MainWindowHandle
        if ($mainWindowHandle -ne [IntPtr]::Zero) {
            break
        }
    }

    if ($mainWindowHandle -eq [IntPtr]::Zero) {
        Write-Host "Process is alive, but no top-level window was created within $StartupTimeoutSeconds second(s)." -ForegroundColor Red
        Show-AvaloniaLog -LogPath $logPath
        try { Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue } catch { }
        throw "avalonia process is running without a visible main window."
    }

    Write-Host ("Main window: 0x{0:X}" -f $mainWindowHandle.ToInt64()) -ForegroundColor Green
}

$process.WaitForExit()
$process.Refresh()
$exitCode = $process.ExitCode
Write-Host "Exit code: $exitCode"

if ($exitCode -ne 0) {
    if ($Target -eq "avalonia") {
        Show-AvaloniaLog -LogPath $logPath
    }
    throw "$Target exited with code $exitCode."
}

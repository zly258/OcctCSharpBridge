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
$DistRoot = Join-Path $RepoRoot "external\OcctCSharpBridge\win-x64"
$ContractPath = Join-Path $DistRoot "bridge-contract.json"
$BuildScript = Join-Path $RepoRoot "build.ps1"
$DefaultOcctRoot = "D:\tools\occt-vc144-64"

if ([string]::IsNullOrWhiteSpace($OcctRoot)) { $OcctRoot = $DefaultOcctRoot }

$targetKey = $Target.ToLowerInvariant()
& $BuildScript $targetKey $Configuration
if ($LASTEXITCODE -ne 0) { throw "Demo $targetKey build failed." }

if (-not (Test-Path -LiteralPath $ContractPath -PathType Leaf)) {
    throw "Bridge Binary SDK contract was not found after the demo build: $ContractPath"
}
$contract = Get-Content -LiteralPath $ContractPath -Raw -Encoding UTF8 | ConvertFrom-Json

function Get-ProjectTargetFramework {
    param(
        [Parameter(Mandatory = $true)][string]$RelativeProjectPath,
        [Parameter(Mandatory = $true)][string]$ExpectedFramework
    )

    $projectPath = Join-Path $RepoRoot $RelativeProjectPath
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) { throw "Demo project was not found: $projectPath" }
    try { [xml]$project = Get-Content -LiteralPath $projectPath -Raw -Encoding UTF8 }
    catch { throw "Demo project is not valid XML: $RelativeProjectPath`n$($_.Exception.Message)" }

    $node = $project.SelectSingleNode("/Project/PropertyGroup/TargetFramework[normalize-space(.) != '']")
    if ($null -eq $node) { throw "Demo project does not declare TargetFramework: $RelativeProjectPath" }
    $framework = ([string]$node.InnerText).Trim()
    if ($framework -ne $ExpectedFramework) {
        throw "Demo project '$RelativeProjectPath' targets '$framework'; expected '$ExpectedFramework'. Demo runtime TFM must remain independent from the Bridge Binary SDK TFM."
    }
    return $framework
}

$apps = @{
    winform = @{
        Project = "src\OcctDemo.WinForms\OcctDemo.WinForms.csproj"
        ExpectedFramework = "net10.0-windows"
        OutputDirectory = "src\OcctDemo.WinForms\bin\x64\$Configuration"
        Executable = "CAD-Winform.exe"
    }
    wpf = @{
        Project = "src\OcctDemo.Wpf\OcctDemo.Wpf.csproj"
        ExpectedFramework = "net10.0-windows"
        OutputDirectory = "src\OcctDemo.Wpf\bin\x64\$Configuration"
        Executable = "CAD-WPF.exe"
    }
    avalonia = @{
        Project = "src\OcctDemo.Avalonia\OcctDemo.Avalonia.csproj"
        ExpectedFramework = "net10.0"
        OutputDirectory = "src\OcctDemo.Avalonia\bin\x64\$Configuration"
        Executable = "CAD-Avalonia.exe"
    }
}

$definition = $apps[$targetKey]
$demoFramework = Get-ProjectTargetFramework ([string]$definition.Project) ([string]$definition.ExpectedFramework)
$applicationDirectory = Join-Path $RepoRoot (Join-Path ([string]$definition.OutputDirectory) $demoFramework)
$executable = Join-Path $applicationDirectory ([string]$definition.Executable)
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw "Executable was not found: $executable`nRun .\build.ps1 $targetKey $Configuration first." }

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
Write-Host "Demo TFM:    $demoFramework" -ForegroundColor DarkGray
Write-Host "Bridge:      $($contract.bridgeVersion), ABI $($contract.nativeAbi.current), SDK target $($contract.dotnet.targetFramework)" -ForegroundColor DarkGray
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

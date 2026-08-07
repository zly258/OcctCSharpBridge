param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$hostProject = Join-Path $RepositoryRoot "src\OcctNet.Avalonia\OcctNet.Avalonia.csproj"
$hostControl = Join-Path $RepositoryRoot "src\OcctNet.Avalonia\OcctAvaloniaViewport.cs"
$demoProject = Join-Path $RepositoryRoot "src\CadAvalonia\CadAvalonia.csproj"
$demoWindow = Join-Path $RepositoryRoot "src\CadAvalonia\MainWindow.cs"

foreach ($path in @($hostProject, $hostControl, $demoProject, $demoWindow)) {
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Required Avalonia host file was not found: $path"
    }
}

$hostProjectText = [System.IO.File]::ReadAllText($hostProject)
foreach ($token in @(
    '<TargetFramework>net8.0-windows</TargetFramework>',
    '<PlatformTarget>x64</PlatformTarget>',
    '<AssemblyName>OcctNet.Avalonia</AssemblyName>',
    '<PackageReference Include="Avalonia" Version="12.1.0" />',
    '..\OcctNet\OcctNet.csproj'
)) {
    if (-not $hostProjectText.Contains($token)) {
        throw "Avalonia host project contract is missing: $token"
    }
}

$hostText = [System.IO.File]::ReadAllText($hostControl)
foreach ($token in @(
    'public sealed class OcctAvaloniaViewport : NativeControlHost',
    'CreateNativeControlCore',
    'DestroyNativeControlCore',
    'CreateWindowExW',
    'SetWindowLongPtrW',
    'GetDpiForWindow',
    'public OcctEngine Engine',
    'EnableDefaultInteraction',
    'EnableRectangleSelection',
    'ObjectSelectionChanged',
    'WorldPointChanged',
    'EngineInitialized'
)) {
    if (-not $hostText.Contains($token)) {
        throw "Avalonia viewport contract is missing: $token"
    }
}

if ($hostText.Contains('WindowsFormsHost') -or $hostText.Contains('System.Windows.Forms')) {
    throw "Avalonia host must not depend on WinForms or WPF hosting layers."
}

$demoText = [System.IO.File]::ReadAllText($demoWindow)
foreach ($token in @('OcctAvaloniaViewport', 'SetZUpView', 'MakeBox', 'MakeCylinder', 'MakeSphere')) {
    if (-not $demoText.Contains($token)) {
        throw "Avalonia demo contract is missing: $token"
    }
}

foreach ($path in @($hostProject, $hostControl, $demoProject, $demoWindow)) {
    $bytes = [System.IO.File]::ReadAllBytes($path)
    if ($bytes.Length -lt 3 -or $bytes[0] -ne 0xEF -or $bytes[1] -ne 0xBB -or $bytes[2] -ne 0xBF) {
        throw "Avalonia source file is not UTF-8 with BOM: $path"
    }
}

Write-Host "[avalonia-host] Avalonia HWND host contract validated." -ForegroundColor Green

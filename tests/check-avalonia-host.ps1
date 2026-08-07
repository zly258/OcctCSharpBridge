param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$hostProject = Join-Path $RepositoryRoot "src\OcctNet.Avalonia\OcctNet.Avalonia.csproj"
$hostControl = Join-Path $RepositoryRoot "src\OcctNet.Avalonia\OcctAvaloniaViewport.cs"
$demoProject = Join-Path $RepositoryRoot "src\CadAvalonia\CadAvalonia.csproj"
$demoManifest = Join-Path $RepositoryRoot "src\CadAvalonia\app.manifest"
$demoWindow = Join-Path $RepositoryRoot "src\CadAvalonia\MainWindow.cs"
$demoParameters = Join-Path $RepositoryRoot "src\CadAvalonia\ParameterDialog.cs"
$demoProgram = Join-Path $RepositoryRoot "src\CadAvalonia\Program.cs"
$wpfWindow = Join-Path $RepositoryRoot "src\CadWpf\MainWindow.xaml.cs"
$buildScript = Join-Path $RepositoryRoot "build.ps1"

foreach ($path in @($hostProject, $hostControl, $demoProject, $demoManifest, $demoWindow, $demoParameters, $demoProgram, $wpfWindow, $buildScript)) {
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Required Avalonia host/parity file was not found: $path"
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
    'EngineInitialized',
    'SizeChanged += OnHostSizeChanged',
    'ScheduleNativeViewRefresh'
)) {
    if (-not $hostText.Contains($token)) {
        throw "Avalonia viewport contract is missing: $token"
    }
}

if ($hostText.Contains('WindowsFormsHost') -or $hostText.Contains('System.Windows.Forms')) {
    throw "Avalonia host must not depend on WinForms or WPF hosting layers."
}

$demoProjectText = [System.IO.File]::ReadAllText($demoProject)
foreach ($token in @(
    'Avalonia.Fonts.Inter',
    '<ApplicationManifest>app.manifest</ApplicationManifest>',
    '<UseWindowsForms>true</UseWindowsForms>',
    '..\CadCommon\CadCommon.csproj'
)) {
    if (-not $demoProjectText.Contains($token)) {
        throw "Avalonia demo project contract is missing: $token"
    }
}

$manifestText = [System.IO.File]::ReadAllText($demoManifest)
foreach ($token in @(
    'urn:schemas-microsoft-com:compatibility.v1',
    '<supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />'
)) {
    if (-not $manifestText.Contains($token)) {
        throw "Avalonia Windows manifest contract is missing: $token"
    }
}

$programText = [System.IO.File]::ReadAllText($demoProgram)
foreach ($token in @('CAD-Avalonia.log', 'EnsureNativeBridgeIsDiscoverable', 'WithInterFont()', 'LogToTrace()', 'MessageBoxW')) {
    if (-not $programText.Contains($token)) {
        throw "Avalonia startup diagnostics contract is missing: $token"
    }
}

$buildText = [System.IO.File]::ReadAllText($buildScript)
foreach ($token in @('Copy-OcctRuntimeDependencies', 'TKernel.dll', 'OcctThirdPartyDir')) {
    if (-not $buildText.Contains($token)) {
        throw "Avalonia runtime deployment contract is missing: $token"
    }
}

$demoText = [System.IO.File]::ReadAllText($demoWindow)
foreach ($token in @(
    'OcctAvaloniaViewport',
    'new CadSession(_viewport.Engine)',
    'CadCommandCatalog.Get',
    'RunCommandAsync',
    'Session.Undo()',
    'Session.Redo()',
    'Session.Open',
    'Session.Import',
    'Session.SaveAll',
    'Session.ExportSelected',
    'DumpView',
    'RefreshObjectTree',
    'ShowObjectProperties',
    'SetFaceBoundariesVisible',
    'SetSelectionMode',
    'SetSceneLighting',
    'SetDefaultMaterial',
    'SetProjection',
    'SetDisplayPrecision',
    'SetLanguage',
    'MainWindowKeyDown'
)) {
    if (-not $demoText.Contains($token)) {
        throw "Avalonia CAD parity contract is missing: $token"
    }
}

# Keep every CadCommandId exposed by the WPF reference UI available in the Avalonia UI.
$wpfText = [System.IO.File]::ReadAllText($wpfWindow)
$commandPattern = 'CadCommandId\.([A-Za-z0-9_]+)'
$wpfCommands = @([regex]::Matches($wpfText, $commandPattern) | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
$avaloniaCommands = @([regex]::Matches($demoText, $commandPattern) | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
$missingCommands = @($wpfCommands | Where-Object { $_ -notin $avaloniaCommands })
if ($missingCommands.Count -gt 0) {
    throw "Avalonia CAD demo is missing WPF commands: $($missingCommands -join ', ')"
}
Write-Host "[avalonia-host] WPF command parity: $($wpfCommands.Count) command IDs covered." -ForegroundColor Green

$parameterText = [System.IO.File]::ReadAllText($demoParameters)
foreach ($token in @('CadParameterDefinition', 'GetValuesAsync', 'CadParameterKind.Boolean', 'CadParameterKind.Choice')) {
    if (-not $parameterText.Contains($token)) {
        throw "Avalonia parameter dialog contract is missing: $token"
    }
}

foreach ($path in @($hostProject, $hostControl, $demoProject, $demoManifest, $demoWindow, $demoParameters, $demoProgram)) {
    $bytes = [System.IO.File]::ReadAllBytes($path)
    if ($bytes.Length -lt 3 -or $bytes[0] -ne 0xEF -or $bytes[1] -ne 0xBB -or $bytes[2] -ne 0xBF) {
        throw "Avalonia source file is not UTF-8 with BOM: $path"
    }
}

Write-Host "[avalonia-host] Avalonia HWND host, full CAD demo parity, Windows manifest, startup diagnostics, and runtime deployment contracts validated." -ForegroundColor Green

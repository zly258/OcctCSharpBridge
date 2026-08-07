param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$winFormsProject = Join-Path $RepositoryRoot "src\OcctNet.WinForms\OcctNet.WinForms.csproj"
$wpfProject = Join-Path $RepositoryRoot "src\OcctNet.Wpf\OcctNet.Wpf.csproj"
$wpfControl = Join-Path $RepositoryRoot "src\OcctNet.Wpf\OcctWpfViewport.cs"
$avaloniaControl = Join-Path $RepositoryRoot "src\OcctNet.Avalonia\OcctAvaloniaViewport.cs"

foreach ($path in @($winFormsProject, $wpfProject, $wpfControl, $avaloniaControl)) {
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Required UI host file was not found: $path"
    }
}

$projectText = [System.IO.File]::ReadAllText($wpfProject)
foreach ($token in @(
    '<UseWPF>true</UseWPF>',
    '<UseWindowsForms>true</UseWindowsForms>',
    '..\OcctNet.WinForms\OcctNet.WinForms.csproj',
    '<AssemblyName>OcctNet.Wpf</AssemblyName>'
)) {
    if (-not $projectText.Contains($token)) {
        throw "WPF host project contract is missing: $token"
    }
}

$controlText = [System.IO.File]::ReadAllText($wpfControl)
foreach ($token in @(
    'public sealed class OcctWpfViewport',
    'WindowsFormsHost',
    'public OcctEngine Engine => _viewport.Engine;',
    'public OcctViewportControl WinFormsViewport => _viewport;',
    'DependencyProperty.Register',
    'EngineInitialized',
    'ObjectSelectionChanged',
    'WorldPointChanged',
    'ErrorOccurred'
)) {
    if (-not $controlText.Contains($token)) {
        throw "WPF viewport contract is missing: $token"
    }
}

if ($controlText -notmatch 'OcctWpfViewport\s*:\s*(?:WpfUserControl|System\.Windows\.Controls\.UserControl)') {
    throw "OcctWpfViewport must derive from the WPF UserControl type."
}


$demoFiles = @(
    (Join-Path $RepositoryRoot "src\CadWinForms\MainForm.cs"),
    (Join-Path $RepositoryRoot "src\CadWpf\MainWindow.xaml.cs")
)
foreach ($demoFile in $demoFiles) {
    $demoText = [System.IO.File]::ReadAllText($demoFile)
    foreach ($token in @('Menu.ShadedEdges', 'SetFaceBoundariesVisible')) {
        if (-not $demoText.Contains($token)) {
            throw "Demo shaded-edge display contract is missing '$token' in $demoFile"
        }
    }
}

$avaloniaText = [System.IO.File]::ReadAllText($avaloniaControl)
foreach ($token in @(
    'SsNotify',
    'WmNcHitTest',
    'WmSize',
    'WmWindowPosChanged',
    'ScheduleNativeViewRefresh',
    'SetCapture(hwnd)',
    'ScreenToClient',
    'ZoomAtPoint',
    'if (lParam != hwnd)'
)) {
    if (-not $avaloniaText.Contains($token)) {
        throw "Avalonia viewport interaction/resize contract is missing: $token"
    }
}

Write-Host "[ui-hosts] WinForms, WPF and Avalonia viewport host contracts validated." -ForegroundColor Green

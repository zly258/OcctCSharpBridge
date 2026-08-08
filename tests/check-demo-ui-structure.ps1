param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$requiredPartialFiles = @(
    "src/CadWinForms/MainForm.Layout.cs",
    "src/CadWinForms/MainForm.Menus.cs",
    "src/CadWinForms/MainForm.Commands.cs",
    "src/CadWinForms/MainForm.Objects.cs",
    "src/CadWinForms/MainForm.Localization.cs",
    "src/CadWpf/MainWindow.xaml.Menus.cs",
    "src/CadWpf/MainWindow.xaml.Commands.cs",
    "src/CadWpf/MainWindow.xaml.Objects.cs",
    "src/CadWpf/MainWindow.xaml.Localization.cs",
    "src/CadAvalonia/MainWindow.Layout.cs",
    "src/CadAvalonia/MainWindow.Menus.cs",
    "src/CadAvalonia/MainWindow.Commands.cs",
    "src/CadAvalonia/MainWindow.Objects.cs",
    "src/CadAvalonia/MainWindow.Localization.cs"
)
foreach ($relativePath in $requiredPartialFiles) {
    if (-not (Test-Path (Join-Path $RepositoryRoot $relativePath) -PathType Leaf)) {
        throw "Demo UI responsibility file is missing: $relativePath"
    }
}

# Keep the window roots focused on state, construction, event wiring and safe execution.
$rootLimits = [ordered]@{
    "src/CadWinForms/MainForm.cs" = 16000
    "src/CadWpf/MainWindow.xaml.cs" = 16000
    "src/CadAvalonia/MainWindow.cs" = 16000
}
foreach ($entry in $rootLimits.GetEnumerator()) {
    $file = Get-Item (Join-Path $RepositoryRoot $entry.Key)
    if ($file.Length -gt $entry.Value) {
        throw "Demo window root grew too large ($($file.Length) bytes > $($entry.Value)): $($entry.Key). Move responsibilities into partial files."
    }
}

# No responsibility partial should return to the former 50-60 KB monolith size.
foreach ($directory in @("src/CadWinForms", "src/CadWpf", "src/CadAvalonia")) {
    Get-ChildItem (Join-Path $RepositoryRoot $directory) -Filter "Main*.cs" -File | ForEach-Object {
        if ($_.Length -gt 24000) {
            throw "Demo responsibility file is too large ($($_.Length) bytes): $($_.FullName)"
        }
    }
}

$winForms = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/CadWinForms/MainForm.Designer.cs"))
foreach ($token in @(
    "_rootLayout.Controls.Add(_logGroup, 0, 3)",
    "_rootLayout.Controls.Add(_statusBar, 0, 4)",
    "_centerRightSplitContainer.Panel2.Controls.Add(_propertyGroup)",
    "_logBox.BackColor = SystemColors.Window",
    "_logBox.ForeColor = SystemColors.WindowText"
)) {
    if (-not $winForms.Contains($token)) {
        throw "WinForms bottom log layout contract is missing: $token"
    }
}
foreach ($token in @("_rightSplitContainer", "Color.FromArgb(16, 24, 32)")) {
    if ($winForms.Contains($token)) {
        throw "WinForms legacy right-side/dark log layout remains: $token"
    }
}

$wpf = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/CadWpf/MainWindow.xaml"))
foreach ($token in @(
    '<GroupBox x:Name="CommandLineGroup" Header="Command Line" Grid.Row="2">',
    'Background="White"',
    'Foreground="#20262C"'
)) {
    if (-not $wpf.Contains($token)) {
        throw "WPF bottom light log layout contract is missing: $token"
    }
}
foreach ($token in @('#101820', '#D8E2EA', '#48525B')) {
    if ($wpf.Contains($token)) {
        throw "WPF dark log palette remains: $token"
    }
}

$avaloniaLayout = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/CadAvalonia/MainWindow.Layout.cs"))
$avaloniaRoot = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/CadAvalonia/MainWindow.cs"))
foreach ($token in @(
    'RowDefinitions = new RowDefinitions("Auto,Auto,*,5,160,Auto")',
    'Grid.SetRow(_commandLineGroup, 4)',
    'Grid.SetColumn(_propertiesGroup, 4)'
)) {
    if (-not $avaloniaLayout.Contains($token)) {
        throw "Avalonia bottom log layout contract is missing: $token"
    }
}
foreach ($token in @(
    'Background = AvaloniaBrushes.White',
    'Foreground = new SolidColorBrush(AvaloniaColor.Parse("#20262C"))'
)) {
    if (-not $avaloniaRoot.Contains($token)) {
        throw "Avalonia light log palette contract is missing: $token"
    }
}
foreach ($token in @('#101820', '#D8E2EA', '#48525B')) {
    if ($avaloniaRoot.Contains($token) -or $avaloniaLayout.Contains($token)) {
        throw "Avalonia dark log palette remains: $token"
    }
}

Write-Host "[demo-ui] WinForms, WPF and Avalonia use split responsibilities with full-width bottom light log panels." -ForegroundColor Green

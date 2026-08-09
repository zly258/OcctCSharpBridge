param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot "ContractTestHelpers.psm1") -Force

$contracts = [ordered]@{
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj" = @('..\OcctNet\OcctNet.csproj')
    "src/OcctNet.WinForms/OcctViewportControl.cs" = @(
        'public sealed class OcctViewportControl',
        'OcctViewportInteractionPolicy.',
        'FirstSelectedObjectOwned',
        'SelectedObjectsOwned'
    )
    "src/OcctNet.Wpf/OcctNet.Wpf.csproj" = @(
        '<UseWPF>true</UseWPF>',
        '<UseWindowsForms>true</UseWindowsForms>',
        '..\OcctNet.WinForms\OcctNet.WinForms.csproj',
        '<AssemblyName>OcctNet.Wpf</AssemblyName>'
    )
    "src/OcctNet.Wpf/OcctWpfViewport.cs" = @(
        'public sealed class OcctWpfViewport',
        'WindowsFormsHost',
        'public OcctEngine Engine => _viewport.Engine;',
        'public OcctViewportControl WinFormsViewport => _viewport;',
        'DependencyProperty.Register',
        'EngineInitialized',
        'ObjectSelectionChanged',
        'WorldPointChanged',
        'ErrorOccurred'
    )
    "src/OcctNet.Avalonia/OcctNet.Avalonia.csproj" = @(
        '<TargetFramework>net8.0-windows</TargetFramework>',
        '..\OcctNet\OcctNet.csproj',
        '<PackageReference Include="Avalonia" Version="12.1.0" ExcludeAssets="analyzers" />',
        '<AssemblyName>OcctNet.Avalonia</AssemblyName>'
    )
    "src/OcctNet.Avalonia/OcctAvaloniaViewport.cs" = @(
        'public sealed class OcctAvaloniaViewport',
        'NativeControlHost',
        'OperatingSystem.IsWindows()',
        'HandleDescriptor',
        '"HWND"',
        'CreateWindowExW',
        'OcctViewportInteractionPolicy.',
        'FirstSelectedObjectOwned',
        'SelectedObjectsOwned',
        'EngineInitialized',
        'ObjectSelectionChanged',
        'WorldPointChanged',
        'ErrorOccurred'
    )
}

Assert-ContractMap -RepositoryRoot $RepositoryRoot -Contracts $contracts -ContractName "UI host contract"

$wpfText = Get-ContractText -RepositoryRoot $RepositoryRoot -RelativePath "src/OcctNet.Wpf/OcctWpfViewport.cs"
if ($wpfText -notmatch 'OcctWpfViewport\s*:\s*(?:WpfUserControl|System\.Windows\.Controls\.UserControl)') {
    throw "OcctWpfViewport must derive from the WPF UserControl type."
}

$coreProject = Get-ContractText -RepositoryRoot $RepositoryRoot -RelativePath "src/OcctNet/OcctNet.csproj"
if ($coreProject -match 'Avalonia|WindowsForms|UseWPF') {
    throw "OcctNet core must remain UI-framework independent."
}

$isDemoLayout = Test-Path (Join-Path $RepositoryRoot "src\CadCommon\CadCommon.csproj") -PathType Leaf
if (-not $isDemoLayout) {
    foreach ($cadPath in @("src/CadCommon", "src/CadWinForms", "src/CadWpf", "src/CadAvalonia")) {
        if (Test-Path (Join-Path $RepositoryRoot $cadPath)) {
            throw "Reusable main SDK must not contain upper-layer CAD application project: $cadPath"
        }
    }
}

$layoutName = if ($isDemoLayout) { "demo" } else { "bridge" }
Write-Host "[ui-hosts] WinForms, WPF and Windows-HWND Avalonia hosts validated for $layoutName layout; OcctNet core remains UI-framework independent." -ForegroundColor Green

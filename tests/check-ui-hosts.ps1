param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot "ContractTestHelpers.psm1") -Force

$contracts = [ordered]@{
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj" = @()
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
}

Assert-ContractMap -RepositoryRoot $RepositoryRoot -Contracts $contracts -ContractName "UI host contract"

$controlText = Get-ContractText -RepositoryRoot $RepositoryRoot -RelativePath "src/OcctNet.Wpf/OcctWpfViewport.cs"
if ($controlText -notmatch 'OcctWpfViewport\s*:\s*(?:WpfUserControl|System\.Windows\.Controls\.UserControl)') {
    throw "OcctWpfViewport must derive from the WPF UserControl type."
}

Write-Host "[ui-hosts] WinForms and WPF viewport host contracts validated." -ForegroundColor Green

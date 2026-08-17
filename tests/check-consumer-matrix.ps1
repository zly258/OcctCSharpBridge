param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-ProjectFrameworks {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Consumer matrix project was not found: $RelativePath" }
    [xml]$project = Get-Content -LiteralPath $path -Raw -Encoding UTF8
    $node = $project.SelectSingleNode("/Project/PropertyGroup/TargetFrameworks[normalize-space(.) != '']")
    if ($null -eq $node) { throw "Consumer matrix project must declare TargetFrameworks: $RelativePath" }
    return @(([string]$node.InnerText).Split(';', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() })
}

$contractPath = Join-Path $RepositoryRoot "bridge-contract.json"
if (-not (Test-Path -LiteralPath $contractPath -PathType Leaf)) { throw "Bridge contract file was not found." }
$contract = Get-Content -LiteralPath $contractPath -Raw -Encoding UTF8 | ConvertFrom-Json

$expectedCore = @($contract.dotnet.supportedConsumerFrameworks | ForEach-Object { [string]$_ })
$expectedDesktop = @($contract.dotnet.supportedDesktopConsumerFrameworks | ForEach-Object { [string]$_ })
$actualCore = @(Read-ProjectFrameworks "tests/OcctNet.ConsumerMatrix/OcctNet.ConsumerMatrix.csproj")
$actualDesktop = @(Read-ProjectFrameworks "tests/OcctNet.DesktopConsumerMatrix/OcctNet.DesktopConsumerMatrix.csproj")

function Assert-ExactSet {
    param([string]$Name, [string[]]$Expected, [string[]]$Actual)
    $missing = @($Expected | Where-Object { $_ -notin $Actual })
    $extra = @($Actual | Where-Object { $_ -notin $Expected })
    if ($Expected.Count -ne $Actual.Count -or $missing.Count -gt 0 -or $extra.Count -gt 0) {
        throw "$Name matrix must exactly match bridge-contract.json. Expected: $($Expected -join ', '); actual: $($Actual -join ', ')."
    }
}

Assert-ExactSet "Core/Avalonia consumer" $expectedCore $actualCore
Assert-ExactSet "WinForms/WPF consumer" $expectedDesktop $actualDesktop

Write-Host "[consumer-matrix] Core/Avalonia: $($actualCore -join ', '); WinForms/WPF: $($actualDesktop -join ', ')." -ForegroundColor Green

param(
    [string]$OcctRoot = $env:OCCT_ROOT,
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$PublishScript = Join-Path $RepoRoot "publish.ps1"
if (-not (Test-Path -LiteralPath $PublishScript -PathType Leaf)) {
    throw "publish.ps1 was not found: $PublishScript"
}

Write-Warning "tools/validate-stable-release.ps1 is deprecated. Stable validation is now integrated into publish.ps1."

$arguments = @{}
if (-not [string]::IsNullOrWhiteSpace($OcctRoot)) { $arguments.OcctRoot = $OcctRoot }
if (-not [string]::IsNullOrWhiteSpace($OutputDirectory)) { $arguments.OutputDirectory = $OutputDirectory }

& $PublishScript @arguments -Zip

param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$publishPath = Join-Path $RepositoryRoot "publish.ps1"
if (-not (Test-Path $publishPath -PathType Leaf)) { throw "publish.ps1 was not found." }
$text = [System.IO.File]::ReadAllText($publishPath)

foreach ($token in @(
    '[string]$Target = "all"',
    '$UseSelfContained = -not $FrameworkDependent.IsPresent',
    '--self-contained", $UseSelfContained.ToString().ToLowerInvariant()',
    'function Test-PackagedNativeClosure',
    'function Write-PackageContract',
    'package-contract.json',
    'native-dependencies.txt',
    'Get-VisualCppRuntimeFiles',
    'msvcp140_atomic_wait.dll',
    'vcruntime140_threads.dll',
    'throw "Required OCCT resource directory was not found: $name"'
)) {
    if (-not $text.Contains($token)) { throw "Required package token is missing: $token" }
}

if ($text -match 'return Test-Path \(Join-Path \(\[Environment\]::SystemDirectory\) \$Name\)' -and
    -not $text.Contains('Test-VisualCppRuntimeDependency')) {
    throw "VC runtime dependencies may still be incorrectly classified as system DLLs."
}

Write-Host "[package] Self-contained .NET, OCCT, third-party, VC runtime, resources and closure validation rules passed." -ForegroundColor Green

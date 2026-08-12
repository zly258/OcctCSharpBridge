param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$runtimePath = Join-Path $RepositoryRoot "src\OcctNet\OcctRuntime.cs"
$diagnosticsPath = Join-Path $RepositoryRoot "src\OcctNet\OcctRuntime.Diagnostics.cs"
$testPath = Join-Path $RepositoryRoot "tests\OcctNet.ManagedTests\RuntimeDiagnosticTests.cs"

foreach ($path in @($runtimePath, $diagnosticsPath, $testPath)) {
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Runtime diagnostics contract file was not found: $path"
    }
}

$runtime = [System.IO.File]::ReadAllText($runtimePath)
foreach ($token in @("public static partial class OcctRuntime", "GetDiagnosticReport")) {
    if (-not $runtime.Contains($token)) {
        throw "OcctRuntime compatibility contract is missing: $token"
    }
}

$diagnostics = [System.IO.File]::ReadAllText($diagnosticsPath)
foreach ($token in @(
    "OcctRuntimeDiagnosticInfo",
    "GetDiagnosticInfo",
    "OCCT_BRIDGE_NATIVE_DIR",
    "OCCT_ROOT",
    "CASROOT",
    "ConfiguredNativeBridgeExists",
    "ConfiguredOcctKernelExists",
    "LoadedNativeBridgePath",
    "LoadedOcctKernelPath",
    "DiagnosticReport",
    "TryFindLoadedRuntimeModule"
)) {
    if (-not $diagnostics.Contains($token)) {
        throw "Structured runtime diagnostics contract is missing: $token"
    }
}

$test = [System.IO.File]::ReadAllText($testPath)
foreach ($token in @("OcctRuntime.GetDiagnosticInfo()", "DiagnosticReport", "Is64BitProcess")) {
    if (-not $test.Contains($token)) {
        throw "Runtime diagnostics managed regression coverage is missing: $token"
    }
}

foreach ($relativePath in @("docs/RUNTIME_DIAGNOSTICS.md", "docs/RUNTIME_DIAGNOSTICS.zh-CN.md")) {
    $path = Join-Path $RepositoryRoot $relativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Runtime diagnostics documentation is missing: $relativePath"
    }
}

Write-Host "[runtime-diagnostics] Structured snapshot and legacy text-report contracts validated." -ForegroundColor Green

param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$paths = [ordered]@{
    Core = Join-Path $RepositoryRoot "src\OcctNet\OcctRuntime.cs"
    Probing = Join-Path $RepositoryRoot "src\OcctNet\OcctRuntime.Probing.cs"
    Environment = Join-Path $RepositoryRoot "src\OcctNet\OcctRuntime.Environment.cs"
    Diagnostics = Join-Path $RepositoryRoot "src\OcctNet\OcctRuntime.Diagnostics.cs"
    Test = Join-Path $RepositoryRoot "tests\OcctNet.ManagedTests\RuntimeDiagnosticTests.cs"
}
foreach ($entry in $paths.GetEnumerator()) {
    if (-not (Test-Path $entry.Value -PathType Leaf)) {
        throw "Runtime contract file was not found ($($entry.Key)): $($entry.Value)"
    }
}

$core = [System.IO.File]::ReadAllText($paths.Core)
foreach ($token in @(
    "public static partial class OcctRuntime",
    "public static void Configure()",
    "ValidateExplicitConfiguration",
    "ValidateReconfiguration"
)) {
    if (-not $core.Contains($token)) {
        throw "OcctRuntime core contract is missing: $token"
    }
}
foreach ($forbiddenDefinition in @(
    "public static string GetDiagnosticReport(",
    "private static IReadOnlyList<string> GetNativeLibraryCandidatesCore(",
    "private static string? ResolveNativeBridgeDirectory(",
    "private static string? ResolveOcctRoot(",
    "private static void InitializeNativeSearchPolicy("
)) {
    if ($core.Contains($forbiddenDefinition)) {
        throw "OcctRuntime core contains an implementation that belongs in another partial: $forbiddenDefinition"
    }
}

$probing = [System.IO.File]::ReadAllText($paths.Probing)
foreach ($token in @(
    "GetNativeLibraryCandidates",
    "GetNativeLibraryCandidatesCore",
    "ResolveNativeBridgeDirectory",
    "ResolveOcctRoot",
    "FindRepositoryRoot",
    "FindResourceDirectory"
)) {
    if (-not $probing.Contains($token)) {
        throw "OcctRuntime probing contract is missing: $token"
    }
}

$environment = [System.IO.File]::ReadAllText($paths.Environment)
foreach ($token in @(
    "InitializeNativeSearchPolicy",
    "AddRuntimeSearchPath",
    "AddThirdPartyRuntimePaths",
    "ConfigureResources",
    "PrependPath",
    "SetDefaultDllDirectories",
    "AddDllDirectory",
    "SetDllDirectory"
)) {
    if (-not $environment.Contains($token)) {
        throw "OcctRuntime environment/search contract is missing: $token"
    }
}

$diagnostics = [System.IO.File]::ReadAllText($paths.Diagnostics)
foreach ($token in @(
    "OcctRuntimeDiagnosticInfo",
    "GetDiagnosticInfo",
    "GetDiagnosticReport",
    "ApplicationNativeBridgePath",
    "ApplicationNativeBridgeExists",
    "ApplicationOcctKernelPath",
    "ApplicationOcctKernelExists",
    "OCCT_BRIDGE_NATIVE_DIR",
    "OCCT_ROOT",
    "CASROOT",
    "ConfiguredNativeBridgeExists",
    "ConfiguredOcctKernelExists",
    "LoadedNativeBridgePath",
    "LoadedOcctKernelPath",
    "DiagnosticTryFindLoadedRuntimeModule"
)) {
    if (-not $diagnostics.Contains($token)) {
        throw "Structured runtime diagnostics contract is missing: $token"
    }
}

$test = [System.IO.File]::ReadAllText($paths.Test)
foreach ($token in @(
    "OcctRuntime.GetDiagnosticInfo()",
    "OcctRuntime.GetDiagnosticReport()",
    "ApplicationNativeBridgeExists",
    "ApplicationOcctKernelExists",
    "Runtime diagnostics changed environment variable",
    "Is64BitProcess"
)) {
    if (-not $test.Contains($token)) {
        throw "Runtime diagnostics managed regression coverage is missing: $token"
    }
}

foreach ($relativePath in @("docs/RUNTIME_DIAGNOSTICS.md", "docs/RUNTIME_DIAGNOSTICS.zh-CN.md")) {
    $path = Join-Path $RepositoryRoot $relativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Runtime diagnostics documentation is missing: $relativePath"
    }
    $documentation = [System.IO.File]::ReadAllText($path)
    foreach ($token in @("ApplicationNativeBridge", "ApplicationOcctKernel", "GetDiagnosticInfo")) {
        if (-not $documentation.Contains($token)) {
            throw "Runtime diagnostics documentation contract is missing '$token': $relativePath"
        }
    }
}

Write-Host "[runtime-diagnostics] Runtime configuration, probing, environment/search policy, app-local diagnostics, and side-effect-free reports validated." -ForegroundColor Green

param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require-Pattern {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -notmatch $Pattern) { throw $Message }
}

$nativeRoot = Join-Path $RepositoryRoot "src\OcctNative"
$viewerExecutorPath = Join-Path $nativeRoot "core\OcctInternal.hxx"
$modelExecutorPath = Join-Path $nativeRoot "modeling\OcctModelingSessionInternal.hxx"
$corePath = Join-Path $nativeRoot "core\OcctEngine.cpp"

foreach ($path in @($viewerExecutorPath, $modelExecutorPath, $corePath)) {
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Native exception-boundary input is missing: $path"
    }
}

$viewerExecutor = [System.IO.File]::ReadAllText($viewerExecutorPath)
$modelExecutor = [System.IO.File]::ReadAllText($modelExecutorPath)
$core = [System.IO.File]::ReadAllText($corePath)

$requiredCatches = @(
    'catch\s*\(const\s+Standard_Failure&',
    'catch\s*\(const\s+std::invalid_argument&',
    'catch\s*\(const\s+std::logic_error&',
    'catch\s*\(const\s+std::bad_alloc&',
    'catch\s*\(const\s+std::exception&',
    'catch\s*\(\.\.\.\)'
)
foreach ($executor in @(
    @{ Name = "Viewer"; Text = $viewerExecutor },
    @{ Name = "Modeling"; Text = $modelExecutor }
)) {
    foreach ($pattern in $requiredCatches) {
        Require-Pattern $executor.Text $pattern "$($executor.Name) ABI executor is missing required exception handling: $pattern"
    }
}

foreach ($requiredViewerToken in @(
    'currentErrorCode\(\)',
    'currentErrorMessage\(\)',
    'errorsByThread',
    'std::recursive_mutex\s+errorMutex'
)) {
    Require-Pattern $viewerExecutor $requiredViewerToken "Viewer thread-local error contract is missing: $requiredViewerToken"
}

Require-Pattern $core 'occt_engine_last_error_code[\s\S]*?catch\s*\(\.\.\.\)' "occt_engine_last_error_code must contain a no-throw catch boundary."
Require-Pattern $core 'occt_engine_last_error_message[\s\S]*?catch\s*\(const\s+std::bad_alloc&[\s\S]*?catch\s*\(\.\.\.\)' "occt_engine_last_error_message must translate allocation and unknown exceptions."

$tracked = @(& git -C $RepositoryRoot ls-files -- "src/OcctNative/*.cpp" "src/OcctNative/**/*.cpp" 2>$null)
if ($LASTEXITCODE -ne 0) { throw "Unable to enumerate tracked native C++ sources." }

$sharedErrorViolations = @()
$unwrappedThrowViolations = @()
$wrapperPattern = '\b(?:execute|executeObject|executeStatus|executeValue|executeShape|executeShapeStatus|executeAlgorithmStatus)\s*\(|\btry\s*\{'
foreach ($relativePath in $tracked) {
    $path = Join-Path $RepositoryRoot $relativePath
    $text = [System.IO.File]::ReadAllText($path)

    if ($text -match 'engine->errors\.(?:code|message|scratch)') {
        $sharedErrorViolations += $relativePath
    }

    if ($text -match 'extern\s+"C"' -and $text -match '\bthrow\b' -and $text -notmatch $wrapperPattern) {
        $unwrappedThrowViolations += $relativePath
    }
}

if ($sharedErrorViolations.Count -gt 0) {
    throw "C ABI sources directly read shared Viewer errors: $($sharedErrorViolations -join ', ')"
}
if ($unwrappedThrowViolations.Count -gt 0) {
    throw "C ABI source contains throw expressions without an executor or try/catch boundary: $($unwrappedThrowViolations -join ', ')"
}

Write-Host (
    "[exception-boundary] Viewer/Modeling catch matrices, thread-local error access, no-throw error queries and {0} native sources validated." -f
    $tracked.Count
) -ForegroundColor Green

param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-RepositoryText {
    param([Parameter(Mandatory = $true)][string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Bulk ABI contract file was not found: $RelativePath"
    }
    return [System.IO.File]::ReadAllText($path)
}

$analysisSession = Read-RepositoryText "src\OcctNet\OcctModelingSession.Analysis.cs"
foreach ($token in @(
    "occt_model_ray_hits_copy",
    "new NativeModelRayHit[count]",
    "Native ray-hit count changed during bulk copy"
)) {
    if (-not $analysisSession.Contains($token)) {
        throw "Managed ray-hit bulk transfer contract is missing: $token"
    }
}
if ($analysisSession.Contains("occt_model_ray_hit_at(_handle")) {
    throw "IntersectRay must not regress to N+1 indexed ray-hit P/Invoke calls."
}

$analysisNativeMethods = Read-RepositoryText "src\OcctNet\ModelNativeMethods.Analysis.cs"
foreach ($token in @("occt_model_ray_hit_at", "occt_model_ray_hits_copy", "[Out] NativeModelRayHit[]")) {
    if (-not $analysisNativeMethods.Contains($token)) {
        throw "Ray-hit Native ABI compatibility/bulk declaration is missing: $token"
    }
}

$analysisNative = Read-RepositoryText "src\OcctNative\OcctModelingAnalysis.cpp"
foreach ($token in @("occt_model_ray_hit_at", "occt_model_ray_hits_copy", "model->rayHits")) {
    if (-not $analysisNative.Contains($token)) {
        throw "Ray-hit Native compatibility/bulk implementation is missing: $token"
    }
}

$historySession = Read-RepositoryText "src\OcctNet\OcctModelingSession.History.cs"
foreach ($token in @(
    "GetHistoryShapes",
    "occt_model_history_generated_copy",
    "occt_model_history_modified_copy",
    "Native topology-history count changed during bulk copy"
)) {
    if (-not $historySession.Contains($token)) {
        throw "Managed topology-history bulk transfer contract is missing: $token"
    }
}
if ($historySession.Contains("occt_model_history_generated_at(_handle") -or
    $historySession.Contains("occt_model_history_modified_at(_handle")) {
    throw "Generated/Modified topology-history collection APIs must not regress to N+1 indexed P/Invoke calls."
}

$historyNativeMethods = Read-RepositoryText "src\OcctNet\ModelNativeMethods.History.cs"
foreach ($token in @(
    "occt_model_history_generated_at",
    "occt_model_history_generated_copy",
    "occt_model_history_modified_at",
    "occt_model_history_modified_copy",
    "[Out] long[]"
)) {
    if (-not $historyNativeMethods.Contains($token)) {
        throw "Topology-history Native ABI compatibility/bulk declaration is missing: $token"
    }
}

$historyNative = Read-RepositoryText "src\OcctNative\OcctModelingHistory.cpp"
foreach ($token in @(
    "occt_model_history_generated_at",
    "occt_model_history_generated_copy",
    "occt_model_history_modified_at",
    "occt_model_history_modified_copy",
    "historyCopy"
)) {
    if (-not $historyNative.Contains($token)) {
        throw "Topology-history Native compatibility/bulk implementation is missing: $token"
    }
}

$historyInternal = Read-RepositoryText "src\OcctNative\OcctModelingAlgorithmInternal.hxx"
if (-not $historyInternal.Contains("inline int historyCopy(")) {
    throw "Linear topology-history bulk-copy helper is missing."
}

$nativeHeader = Read-RepositoryText "src\OcctNative\OcctModeling.h"
foreach ($token in @(
    "occt_model_ray_hits_copy",
    "occt_model_history_generated_copy",
    "occt_model_history_modified_copy"
)) {
    if (-not $nativeHeader.Contains($token)) {
        throw "Bulk Native ABI export declaration is missing: $token"
    }
}

Write-Host "[modeling-bulk-abi] Ray hits and topology history use bulk transfer while indexed ABI compatibility exports remain available." -ForegroundColor Green

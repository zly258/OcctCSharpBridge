param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contracts = [ordered]@{
    "src/OcctNative/OcctModelingTopologyAnalysis.h" = @(
        "OcctModelFreeBoundary_Closed",
        "OcctModelFreeBoundary_Open",
        "occt_model_shape_free_bounds"
    )
    "src/OcctNative/OcctModelingTopologyAnalysis.cpp" = @(
        "ShapeAnalysis_FreeBounds",
        "occt_model_shape_free_bounds",
        "analysis.GetClosedWires()",
        "analysis.GetOpenWires()"
    )
    "src/OcctNet/ModelNativeMethods.TopologyAnalysis.cs" = @(
        "occt_model_shape_free_bounds",
        "CallingConvention.Cdecl",
        "ExactSpelling = true"
    )
    "src/OcctNet/OcctModelingSession.TopologyAnalysis.cs" = @(
        "AnalyzeFreeBounds",
        "splitClosed",
        "splitOpen",
        "OcctFreeBoundsResult",
        "GetWires(closedCompound)",
        "GetWires(openCompound)"
    )
    "src/OcctNet/OcctTopologyAnalysisTypes.cs" = @(
        "OcctFreeBoundsResult",
        "ClosedWires",
        "OpenWires",
        "HasFreeBounds",
        "HasOpenFreeBounds"
    )
    "src/OcctNet/OcctModelingSession.TopologyConvenience.cs" = @(
        "GetAdjacentFaces",
        "GetIncidentEdges",
        "GetIncidentFaces",
        "GetBoundaryEdgeCandidates",
        "GetManifoldInteriorEdges",
        "GetNonManifoldEdges"
    )
    "tests/OcctNet.Smoke/FreeBoundsSmoke.cs" = @(
        "GetBoundaryEdgeCandidates",
        "AnalyzeFreeBounds",
        "ClosedWireCount",
        "OpenWireCount"
    )
    "docs/TOPOLOGY_ANALYSIS.md" = @(
        "Fast adjacency screening",
        "Strict free-boundary analysis",
        "GetBoundaryEdgeCandidates()",
        "AnalyzeFreeBounds()"
    )
    "docs/TOPOLOGY_ANALYSIS.zh-CN.md" = @(
        "快速邻接筛选",
        "严格自由边界分析",
        "GetBoundaryEdgeCandidates()",
        "AnalyzeFreeBounds()"
    )
}

foreach ($contract in $contracts.GetEnumerator()) {
    $path = Join-Path $RepositoryRoot $contract.Key
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Topology analysis contract file was not found: $($contract.Key)"
    }

    $text = [System.IO.File]::ReadAllText($path)
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Topology analysis token is missing from $($contract.Key): $token"
        }
    }
}

$cmake = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src\OcctNative\CMakeLists.txt"))
foreach ($token in @("OcctModelingTopologyAnalysis.cpp", "OcctModelingTopologyAnalysis.h", "TKShHealing")) {
    if (-not $cmake.Contains($token)) {
        throw "Native topology analysis build contract is missing: $token"
    }
}

Write-Host "[topology-analysis] Adjacency screening and strict ShapeAnalysis_FreeBounds contracts validated." -ForegroundColor Green

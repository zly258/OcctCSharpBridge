param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot "ContractTestHelpers.psm1") -Force

$contracts = [ordered]@{
    "src/OcctNative/OcctModelingTopologyAnalysis.h" = @(
        "OcctModelFreeBoundary_Closed",
        "OcctModelFreeBoundary_Open",
        "OcctModelEdgeAdjacency",
        "occt_model_shape_free_bounds",
        "occt_model_shape_edge_adjacency"
    )
    "src/OcctNative/OcctModelingTopologyAnalysis.cpp" = @(
        "ShapeAnalysis_FreeBounds",
        "occt_model_shape_free_bounds",
        "analysis.GetClosedWires()",
        "analysis.GetOpenWires()",
        "occt_model_shape_edge_adjacency",
        "MapShapesAndUniqueAncestors",
        "TopAbs_EDGE",
        "TopAbs_FACE"
    )
    "src/OcctNet/ModelNativeMethods.TopologyAnalysis.cs" = @(
        "occt_model_shape_free_bounds",
        "occt_model_shape_edge_adjacency",
        "CallingConvention.Cdecl",
        "ExactSpelling = true"
    )
    "src/OcctNet/OcctModelingSession.TopologyAnalysis.cs" = @(
        "AnalyzeFreeBounds",
        "splitClosed",
        "splitOpen",
        "OcctFreeBoundsResult",
        "GetWires(closedCompound)",
        "GetWires(openCompound)",
        "AnalyzeEdgeAdjacency",
        "NativeModelEdgeAdjacency",
        "OcctEdgeAdjacencyResult"
    )
    "src/OcctNet/OcctTopologyAnalysisTypes.cs" = @(
        "OcctFreeBoundsResult",
        "ClosedWires",
        "OpenWires",
        "HasFreeBounds",
        "HasOpenFreeBounds",
        "OcctEdgeAdjacencyInfo",
        "OcctEdgeAdjacencyResult",
        "BoundaryCandidates",
        "ManifoldInteriorEdges",
        "NonManifoldEdges"
    )
    "src/OcctNet/OcctModelingSession.TopologyConvenience.cs" = @(
        "GetAdjacentFaces",
        "GetIncidentEdges",
        "GetIncidentFaces",
        "GetBoundaryEdgeCandidates",
        "GetManifoldInteriorEdges",
        "GetNonManifoldEdges",
        "AnalyzeEdgeAdjacency(root)"
    )
    "tests/OcctNet.Smoke/FreeBoundsSmoke.cs" = @(
        "GetBoundaryEdgeCandidates",
        "AnalyzeFreeBounds",
        "ClosedWireCount",
        "OpenWireCount"
    )
    "tests/OcctNet.Smoke/EdgeAdjacencySmoke.cs" = @(
        "AnalyzeEdgeAdjacency",
        "ManifoldInteriorEdges",
        "BoundaryCandidates"
    )
    "docs/TOPOLOGY_ANALYSIS.md" = @(
        "Batch adjacency analysis",
        "Strict free-boundary analysis",
        "AnalyzeEdgeAdjacency()",
        "AnalyzeFreeBounds()"
    )
    "docs/TOPOLOGY_ANALYSIS.zh-CN.md" = @(
        "批量邻接分析",
        "严格自由边界分析",
        "AnalyzeEdgeAdjacency()",
        "AnalyzeFreeBounds()"
    )
}

Assert-ContractMap -RepositoryRoot $RepositoryRoot -Contracts $contracts -ContractName "Topology analysis contract"

$cmake = Get-ContractText -RepositoryRoot $RepositoryRoot -RelativePath "src/OcctNative/CMakeLists.txt"
foreach ($token in @("OcctModelingTopologyAnalysis.cpp", "OcctModelingTopologyAnalysis.h", "TKShHealing")) {
    Assert-TextContains -Text $cmake -Token $token -Message "Native topology analysis build contract is missing: $token"
}

Write-Host "[topology-analysis] Batched edge adjacency and strict ShapeAnalysis_FreeBounds contracts validated." -ForegroundColor Green

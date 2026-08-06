param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$requiredFiles = @(
    "src/OcctNet/OcctModelingSession.cs",
    "src/OcctNet/OcctModelingSession.ShapeQueries.cs",
    "src/OcctNet/OcctModelingSession.Topology.cs",
    "src/OcctNet/OcctModelingSession.GeometryQueries.cs",
    "src/OcctNet/OcctModelingSession.AnalyticGeometry.cs",
    "src/OcctNet/OcctModelingSession.DifferentialGeometry.cs",
    "src/OcctNet/OcctModelingSession.Geometry.cs",
    "src/OcctNet/OcctModelingSession.Algorithms.cs",
    "src/OcctNet/OcctModelingSession.Analysis.cs",
    "src/OcctNet/OcctModelingSession.History.cs"
)
foreach ($relativePath in $requiredFiles) {
    if (-not (Test-Path (Join-Path $RepositoryRoot $relativePath) -PathType Leaf)) {
        throw "Managed API category file is missing: $relativePath"
    }
}

$baseText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/OcctModelingSession.cs"))
foreach ($forbidden in @("GetShapeHash(", "GetTopologyCount(", "GetVertexPoint(", "GetEdgeCurveType(")) {
    if ($baseText.Contains($forbidden)) {
        throw "OcctModelingSession.cs contains a categorized API method: $forbidden"
    }
}

$canonicalContracts = [ordered]@{
    "src/OcctNet/OcctModelingSession.ShapeQueries.cs" = @(
        "GetShapeOrientation",
        "GetShapeBounds",
        "GetShapeDistance",
        "GetShapeLocation",
        "SetShapeLocation"
    )
    "src/OcctNet/OcctModelingSession.Topology.cs" = @("GetSubshapeAt")
    "src/OcctNet/OcctModelingSession.GeometryQueries.cs" = @(
        "EvaluateEdgeNormalized",
        "GetEdgeCurveType",
        "GetFaceSurfaceType",
        "GetFaceUvBounds",
        "EvaluateFaceAtParameters"
    )
}
foreach ($contract in $canonicalContracts.GetEnumerator()) {
    $text = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $contract.Key))
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Canonical managed API is missing from $($contract.Key): $token"
        }
    }
}

$nativeMethodFiles = Get-ChildItem (Join-Path $RepositoryRoot "src/OcctNet") -Filter "*NativeMethods*.cs" -File
foreach ($file in $nativeMethodFiles) {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    $attributes = [regex]::Matches($text, '\[DllImport\(LibraryName(?<body>.*?)\)\]', 'Singleline')
    foreach ($attribute in $attributes) {
        $body = $attribute.Groups['body'].Value
        if ($body -notmatch 'CallingConvention\s*=\s*CallingConvention\.Cdecl') {
            throw "Bridge P/Invoke does not declare Cdecl: $($file.Name)"
        }
        if ($body -notmatch 'ExactSpelling\s*=\s*true') {
            throw "Bridge P/Invoke does not use exact symbol spelling: $($file.Name)"
        }
    }
}

$docs = @(Get-ChildItem (Join-Path $RepositoryRoot "docs") -File | Select-Object -ExpandProperty Name | Sort-Object)
$expectedDocs = @("API_COVERAGE.md", "API_COVERAGE.zh-CN.md")
if (Compare-Object $expectedDocs $docs) {
    throw "The docs directory must contain only API_COVERAGE.md and API_COVERAGE.zh-CN.md."
}

Write-Host "[organization] Managed categories, canonical naming, P/Invoke attributes and documentation layout validated." -ForegroundColor Green

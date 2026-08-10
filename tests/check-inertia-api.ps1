param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Text {
    param([string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) { throw "Inertia API contract file was not found: $RelativePath" }
    return [System.IO.File]::ReadAllText($path)
}

$nativeHeader = Read-Text "src\OcctNative\OcctModelingInertia.h"
foreach ($token in @(
    "struct OcctModelInertiaProperties",
    "occt_model_shape_linear_inertia",
    "occt_model_shape_surface_inertia",
    "occt_model_shape_volume_inertia",
    "principalMoment1",
    "principalAxis1",
    "radiusOfGyration1",
    "hasSymmetryAxis",
    "hasSymmetryPoint"
)) {
    if (-not $nativeHeader.Contains($token)) { throw "Native inertia contract is missing: $token" }
}

$native = Read-Text "src\OcctNative\OcctModelingInertia.cpp"
foreach ($token in @(
    "MatrixOfInertia()",
    "PrincipalProperties()",
    "principal.Moments",
    "principal.RadiusOfGyration",
    "principal.FirstAxisOfInertia",
    "principal.HasSymmetryAxis",
    "BRepGProp::LinearProperties",
    "BRepGProp::SurfaceProperties",
    "BRepGProp::VolumeProperties"
)) {
    if (-not $native.Contains($token)) { throw "Native inertia implementation is missing: $token" }
}
if ($native.Contains("Eigen") -or $native.Contains("Jacobi")) {
    throw "Inertia API must use OCCT principal properties instead of a custom eigensolver."
}

$managedType = Read-Text "src\OcctNet\OcctInertiaProperties.cs"
foreach ($token in @(
    "public readonly record struct OcctInertiaProperties",
    "double Ixx",
    "double Ixy",
    "double PrincipalMoment1",
    "OcctVector3d PrincipalAxis1",
    "double RadiusOfGyration1",
    "bool HasSymmetryAxis",
    "bool HasSymmetryPoint",
    "internal struct NativeModelInertiaProperties",
    "ToManaged()"
)) {
    if (-not $managedType.Contains($token)) { throw "Managed inertia DTO contract is missing: $token" }
}

$nativeMethods = Read-Text "src\OcctNet\ModelNativeMethods.Inertia.cs"
foreach ($token in @(
    "occt_model_shape_linear_inertia",
    "occt_model_shape_surface_inertia",
    "occt_model_shape_volume_inertia",
    "CallingConvention.Cdecl",
    "ExactSpelling = true"
)) {
    if (-not $nativeMethods.Contains($token)) { throw "Managed inertia P/Invoke contract is missing: $token" }
}

$session = Read-Text "src\OcctNet\OcctModelingSession.Inertia.cs"
foreach ($token in @(
    "GetLinearInertiaProperties",
    "GetSurfaceInertiaProperties",
    "GetVolumeInertiaProperties",
    "EnsureShape(shape)",
    "Check(query(_handle, shape.Id, out var result))"
)) {
    if (-not $session.Contains($token)) { throw "Managed inertia session contract is missing: $token" }
}

$legacyTypes = Read-Text "src\OcctNative\OcctNative.h"
$massMatch = [regex]::Match($legacyTypes, 'struct\s+OcctMassProperties\s*\{(?<body>.*?)\};', [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $massMatch.Success) { throw "Existing OcctMassProperties Native ABI type is missing." }
$massBody = $massMatch.Groups['body'].Value
if ($massBody -notmatch 'double\s+mass\s*;' -or $massBody -notmatch 'OcctPoint3d\s+centerOfMass\s*;') {
    throw "Existing OcctMassProperties fields changed unexpectedly."
}
if ($massBody -match 'inertia|principal|gyration') {
    throw "Existing OcctMassProperties must not be enlarged; inertia uses a separate additive DTO."
}

Write-Host "[inertia-api] Separate additive inertia DTO, OCCT principal-property implementation, managed owner-aware API, and legacy mass-property ABI boundary validated." -ForegroundColor Green

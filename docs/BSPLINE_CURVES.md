# B-Spline Curve and Surface Inspection

OcctCSharpBridge exposes interpolated B-Spline construction, differential evaluation, and read-only definition inspection for both edge curves and face surfaces.

## Curve definition

```csharp
using var model = new OcctModelingSession();

var edge = model.MakeInterpolatedBSpline(new[]
{
    new OcctPoint3d(0, 0, 0),
    new OcctPoint3d(20, 15, 5),
    new OcctPoint3d(45, -5, 12),
    new OcctPoint3d(70, 20, 18),
    new OcctPoint3d(100, 0, 25)
});

var curve = model.GetBSplineCurveData(edge);
Console.WriteLine($"Degree: {curve.Degree}");
Console.WriteLine($"Rational: {curve.IsRational}");
Console.WriteLine($"Periodic: {curve.IsPeriodic}");
Console.WriteLine($"Poles: {curve.PoleCount}");
Console.WriteLine($"Knots: {curve.KnotCount}");
```

`OcctBSplineCurveData` contains:

- `Degree`
- `IsRational`
- `IsPeriodic`
- `Poles`
- `Weights`
- `Knots`
- `Multiplicities`
- `PoleCount`
- `KnotCount`

The lists are read-only snapshots. Mutating the source OCCT shape later does not mutate a previously returned snapshot.

## Surface definition

For a face whose underlying surface is a B-Spline:

```csharp
var surface = model.GetBSplineSurfaceData(face);

Console.WriteLine($"Degree: {surface.UDegree} x {surface.VDegree}");
Console.WriteLine($"Pole grid: {surface.UPoleCount} x {surface.VPoleCount}");
Console.WriteLine($"U knots: {surface.UKnotCount}");
Console.WriteLine($"V knots: {surface.VKnotCount}");

var pole = surface.GetPole(2, 3);
var weight = surface.GetWeight(2, 3);
```

`OcctBSplineSurfaceData` exposes:

- `UDegree`, `VDegree`
- `IsURational`, `IsVRational`
- `IsUPeriodic`, `IsVPeriodic`
- `UPoleCount`, `VPoleCount`, `PoleCount`
- flattened read-only `Poles` and `Weights`
- `UKnots`, `UMultiplicities`
- `VKnots`, `VMultiplicities`
- `GetPole(uIndex, vIndex)` and `GetWeight(uIndex, vIndex)`

The flat pole/weight arrays use **U-major storage with V varying fastest**:

```text
flatIndex = uIndex * VPoleCount + vIndex
```

Callers that need grid semantics should normally use `GetPole()` and `GetWeight()` instead of calculating the flat index themselves.

## Indexing and weights

All public .NET pole/knot indices are zero-based. The native bridge translates them to OCCT's one-based indices internally.

For non-rational B-Splines, OCCT reports unit weights, so rational and non-rational definitions can be processed through one code path.

Curve `Knots` and surface `UKnots` / `VKnots` contain distinct knot values. Their corresponding multiplicity collections have exactly the same lengths.

## Error behavior

`GetBSplineCurveData()` requires an Edge whose 3D curve is a B-Spline. `GetBSplineSurfaceData()` requires a Face whose underlying surface is a B-Spline. Passing an incompatible shape fails through the normal `OcctException` native-error path instead of returning partial data.

Managed validation additionally rejects malformed native results such as:

- non-finite poles or knots;
- non-positive weights or multiplicities;
- invalid degree/count metadata;
- non-increasing distinct knots.

## ABI design

B-Spline inspection is isolated in `OcctModelingBSpline.h/.cpp` rather than the generic modeling extension module.

Curve inspection uses:

- `occt_model_edge_bspline_info`
- `occt_model_edge_bspline_pole_at`
- `occt_model_edge_bspline_knot_at`

Surface inspection uses:

- `occt_model_face_bspline_info`
- `occt_model_face_bspline_pole_at`
- `occt_model_face_bspline_u_knot_at`
- `occt_model_face_bspline_v_knot_at`

These are additive ABI 3 extensions. Existing ABI 3 functions and signatures are unchanged.

## Native verification

Cloud CI verifies declaration/definition/PInvoke parity and compiles the Smoke project, but it does not have the project OCCT SDK. Real extraction is covered by:

- `tests/OcctNet.Smoke/BSplineSmoke.cs`
- `tests/OcctNet.Smoke/BSplineSurfaceSmoke.cs`

Run both on a Windows machine with OCCT 7.9.0 through the normal native smoke gate:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```

For matrix, coordinate, bounds, and topology convenience APIs, see [Managed Geometry and Transform Utilities](GEOMETRY_UTILITIES.md).

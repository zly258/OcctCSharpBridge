# B-Spline Curve Inspection

OcctCSharpBridge exposes interpolated B-Spline construction, differential evaluation, and now full read-only curve definition inspection through `OcctModelingSession.GetBSplineCurveData()`.

## Read the curve definition

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

var data = model.GetBSplineCurveData(edge);

Console.WriteLine($"Degree: {data.Degree}");
Console.WriteLine($"Rational: {data.IsRational}");
Console.WriteLine($"Periodic: {data.IsPeriodic}");
Console.WriteLine($"Poles: {data.PoleCount}");
Console.WriteLine($"Knots: {data.KnotCount}");
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

The returned lists are read-only snapshots. Mutating the source OCCT shape later does not mutate a previously returned snapshot.

## Indexing and weights

The public .NET collections are zero-based. The native bridge translates these indices to OCCT's one-based pole/knot indexing internally.

`Weights.Count` always equals `Poles.Count`. For a non-rational B-Spline, OCCT reports unit weights, so callers can process rational and non-rational data with one code path.

`Knots` contains distinct knot values. `Multiplicities[index]` is the multiplicity of `Knots[index]`; the two collections always have the same length.

## Error behavior

`GetBSplineCurveData()` requires an Edge whose 3D curve is a B-Spline. Passing a line, circle, or another non-B-Spline edge fails through the normal `OcctException` native-error path instead of returning partial data.

Managed validation additionally rejects malformed native results such as non-finite poles/knots, non-positive weights/multiplicities, or non-increasing distinct knots.

## ABI design

The high-level snapshot is backed by three compact C ABI functions:

- `occt_model_edge_bspline_info`
- `occt_model_edge_bspline_pole_at`
- `occt_model_edge_bspline_knot_at`

This is an additive ABI 3 extension. Existing ABI 3 functions and signatures are unchanged.

## Native verification

Cloud CI verifies declaration/definition/PInvoke parity and compiles the Smoke project, but it does not have the project OCCT SDK. Real extraction is therefore covered by `tests/OcctNet.Smoke/BSplineSmoke.cs` and must be executed on a Windows machine with OCCT 7.9.0:

```powershell
.\build.ps1 smoke Release -OcctRoot "<OCCT 7.9.0 root>"
```

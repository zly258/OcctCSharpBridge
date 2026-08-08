using System.Runtime.CompilerServices;
using OcctNet;

internal static class BSplineSmoke
{
    [ModuleInitializer]
    internal static void Run()
    {
        using var model = new OcctModelingSession();
        var edge = model.MakeInterpolatedBSpline(new[]
        {
            new OcctPoint3d(0, 0, 0),
            new OcctPoint3d(20, 15, 5),
            new OcctPoint3d(45, -5, 12),
            new OcctPoint3d(70, 20, 18),
            new OcctPoint3d(100, 0, 25)
        });

        if (model.GetEdgeCurveType(edge) != OcctCurveType.BSpline)
            throw new InvalidOperationException("Interpolated curve is not reported as a B-Spline.");

        var data = model.GetBSplineCurveData(edge);
        if (data.Degree < 1 || data.PoleCount < 2 || data.KnotCount < 2)
            throw new InvalidOperationException("B-Spline metadata is incomplete.");
        if (data.Poles.Count != data.Weights.Count)
            throw new InvalidOperationException("B-Spline pole/weight counts differ.");
        if (data.Knots.Count != data.Multiplicities.Count)
            throw new InvalidOperationException("B-Spline knot/multiplicity counts differ.");
        if (data.Poles.Any(point => !point.IsFinite))
            throw new InvalidOperationException("B-Spline contains a non-finite pole.");
        if (data.Weights.Any(weight => !double.IsFinite(weight) || weight <= 0))
            throw new InvalidOperationException("B-Spline contains an invalid weight.");
        if (data.Knots.Any(knot => !double.IsFinite(knot)))
            throw new InvalidOperationException("B-Spline contains a non-finite knot.");
        if (data.Multiplicities.Any(multiplicity => multiplicity <= 0))
            throw new InvalidOperationException("B-Spline contains an invalid multiplicity.");

        for (var index = 1; index < data.Knots.Count; index++)
        {
            if (data.Knots[index] <= data.Knots[index - 1])
                throw new InvalidOperationException("B-Spline knots are not strictly increasing.");
        }
    }
}

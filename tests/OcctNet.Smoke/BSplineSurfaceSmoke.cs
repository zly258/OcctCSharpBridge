using System.Runtime.CompilerServices;
using OcctNet;

internal static class BSplineSurfaceSmoke
{
    [ModuleInitializer]
    internal static void Run()
    {
        using var model = new OcctModelingSession();
        var sections = new[]
        {
            model.MakeRectangleWire(80, 50, new OcctPoint3d(0, 0, 0)),
            model.MakeRectangleWire(105, 65, new OcctPoint3d(-10, 8, 35)),
            model.MakeRectangleWire(70, 90, new OcctPoint3d(12, -6, 75)),
            model.MakeRectangleWire(95, 55, new OcctPoint3d(-5, 10, 115))
        };

        var loft = model.Loft(sections, makeSolid: false, ruled: false, tolerance: 1e-6).Shape;
        var bsplineFace = model.GetFaces(loft)
            .FirstOrDefault(face => model.GetFaceSurfaceType(face) == OcctSurfaceType.BSpline);
        if (!bsplineFace.IsValid)
            throw new InvalidOperationException("Loft did not produce a B-Spline face for surface inspection.");

        var data = model.GetBSplineSurfaceData(bsplineFace);
        if (data.UDegree < 1 || data.VDegree < 1)
            throw new InvalidOperationException("B-Spline surface degree metadata is invalid.");
        if (data.UPoleCount < 2 || data.VPoleCount < 2 || data.PoleCount != data.Poles.Count)
            throw new InvalidOperationException("B-Spline surface pole-grid metadata is invalid.");
        if (data.Poles.Count != data.Weights.Count)
            throw new InvalidOperationException("B-Spline surface pole/weight counts differ.");
        if (data.UKnots.Count != data.UMultiplicities.Count ||
            data.VKnots.Count != data.VMultiplicities.Count)
        {
            throw new InvalidOperationException("B-Spline surface knot/multiplicity counts differ.");
        }
        if (data.Poles.Any(point => !point.IsFinite))
            throw new InvalidOperationException("B-Spline surface contains a non-finite pole.");
        if (data.Weights.Any(weight => !double.IsFinite(weight) || weight <= 0))
            throw new InvalidOperationException("B-Spline surface contains an invalid weight.");

        for (var index = 1; index < data.UKnots.Count; index++)
        {
            if (data.UKnots[index] <= data.UKnots[index - 1])
                throw new InvalidOperationException("B-Spline surface U knots are not strictly increasing.");
        }
        for (var index = 1; index < data.VKnots.Count; index++)
        {
            if (data.VKnots[index] <= data.VKnots[index - 1])
                throw new InvalidOperationException("B-Spline surface V knots are not strictly increasing.");
        }

        var firstPole = data.GetPole(0, 0);
        var firstWeight = data.GetWeight(0, 0);
        if (!firstPole.IsFinite || !double.IsFinite(firstWeight) || firstWeight <= 0)
            throw new InvalidOperationException("B-Spline surface indexed pole access failed.");
    }
}

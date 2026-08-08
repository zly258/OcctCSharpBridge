using System.Runtime.CompilerServices;
using OcctNet;

internal static class GeometryUtilityTests
{
    [ModuleInitializer]
    internal static void Run()
    {
        var midpoint = new OcctPoint3d(0, 0, 0).Lerp(new OcctPoint3d(10, 20, 30), 0.5);
        Assert(midpoint.AlmostEquals(new OcctPoint3d(5, 10, 15)), "Point interpolation regression.");
        Assert(!midpoint.AlmostEquals(new OcctPoint3d(5.01, 10, 15), 1e-4), "Point tolerance regression.");

        var rightAngle = OcctVector3d.UnitX.AngleTo(OcctVector3d.UnitY);
        Assert(Math.Abs(rightAngle - Math.PI / 2) < 1e-12, "Vector angle regression.");
        var vector = new OcctVector3d(2, 3, 4);
        Assert(vector.ProjectOnto(OcctVector3d.UnitX).AlmostEquals(new OcctVector3d(2, 0, 0)), "Vector projection regression.");
        Assert(vector.RejectFrom(OcctVector3d.UnitX).AlmostEquals(new OcctVector3d(0, 3, 4)), "Vector rejection regression.");

        var firstBounds = new OcctBounds
        {
            MinX = 0,
            MinY = 0,
            MinZ = 0,
            MaxX = 10,
            MaxY = 20,
            MaxZ = 30
        };
        var secondBounds = new OcctBounds
        {
            MinX = 8,
            MinY = -2,
            MinZ = 5,
            MaxX = 12,
            MaxY = 4,
            MaxZ = 8
        };
        Assert(firstBounds.IsValid(), "Bounds validity regression.");
        Assert(Math.Abs(firstBounds.GetVolume() - 6000) < 1e-12, "Bounds volume regression.");
        Assert(firstBounds.Contains(new OcctPoint3d(5, 10, 15)), "Bounds containment regression.");
        Assert(firstBounds.Intersects(secondBounds), "Bounds intersection regression.");
        var union = firstBounds.Union(secondBounds);
        Assert(union.GetMinimumPoint() == new OcctPoint3d(0, -2, 0), "Bounds union minimum regression.");
        Assert(union.GetMaximumPoint() == new OcctPoint3d(12, 20, 30), "Bounds union maximum regression.");
        var expanded = firstBounds.Expanded(2);
        Assert(expanded.MinX == -2 && expanded.MaxZ == 32, "Bounds expansion regression.");

        var uv = new OcctUvBounds { UMin = -1, UMax = 3, VMin = 2, VMax = 6 };
        Assert(uv.IsValid(), "UV validity regression.");
        Assert(uv.GetCenter() == (1.0, 4.0), "UV center regression.");
        Assert(uv.Contains(1, 4), "UV containment regression.");

        var distance = new OcctDistanceResult
        {
            Distance = 5,
            PointOnFirst = new OcctPoint3d(0, 0, 0),
            PointOnSecond = new OcctPoint3d(3, 4, 0)
        };
        Assert(distance.IsFinite(), "Distance result finite-state regression.");
        Assert(distance.GetSeparationVector() == new OcctVector3d(3, 4, 0), "Distance separation-vector regression.");
        Assert(distance.GetMidpoint() == new OcctPoint3d(1.5, 2, 0), "Distance midpoint regression.");
        Assert(distance.IsWithin(5) && !distance.IsWithin(4.999), "Distance threshold regression.");

        var translation = OcctGeometryExtensions.CreateTranslationLocation(10, 20, 30);
        Assert(translation.TransformPoint(new OcctPoint3d(1, 2, 3)) == new OcctPoint3d(11, 22, 33), "Location point translation regression.");
        Assert(translation.TransformVector(OcctVector3d.UnitX) == OcctVector3d.UnitX, "Location vector translation regression.");

        var rotation = OcctGeometryExtensions.CreateRotationLocation(OcctVector3d.UnitZ, Math.PI / 2);
        Assert(rotation.TransformPoint(new OcctPoint3d(1, 0, 0)).AlmostEquals(new OcctPoint3d(0, 1, 0), 1e-12), "Location rotation regression.");

        var center = new OcctPoint3d(2, 3, 4);
        var scale = OcctGeometryExtensions.CreateUniformScaleLocation(2, center);
        Assert(scale.TransformPoint(center).AlmostEquals(center, 1e-12), "Centered scale origin regression.");
        Assert(scale.TransformPoint(new OcctPoint3d(3, 3, 4)).AlmostEquals(new OcctPoint3d(4, 3, 4), 1e-12), "Centered scale regression.");

        var composed = translation.Multiply(rotation);
        Assert(composed.TransformPoint(new OcctPoint3d(1, 0, 0)).AlmostEquals(new OcctPoint3d(10, 21, 30), 1e-12), "Transform composition order regression.");
        var inverse = composed.Inverted();
        var source = new OcctPoint3d(7, -2, 5);
        Assert(inverse.TransformPoint(composed.TransformPoint(source)).AlmostEquals(source, 1e-10), "Transform inverse round-trip regression.");

        var viewerTransform = composed.ToTransform3d();
        Assert(viewerTransform.TransformPoint(source).AlmostEquals(composed.TransformPoint(source), 1e-12), "Viewer/model transform conversion regression.");
        var modelTransform = viewerTransform.ToModelLocation();
        Assert(modelTransform.TransformPoint(source).AlmostEquals(composed.TransformPoint(source), 1e-12), "Model/viewer transform round-trip regression.");
        Assert(!default(OcctModelLocation).TryInvert(out _), "Singular transform inversion regression.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

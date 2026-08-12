using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class GeometryUtilityTests
{
    [TestMethod]
    public void GeometryUtilitiesRemainStable()
    {
        var midpoint = new OcctPoint3d(0, 0, 0).Lerp(new OcctPoint3d(10, 20, 30), 0.5);
        Check(midpoint.AlmostEquals(new OcctPoint3d(5, 10, 15)), "Point interpolation regression.");
        Check(!midpoint.AlmostEquals(new OcctPoint3d(5.01, 10, 15), 1e-4), "Point tolerance regression.");

        var rightAngle = OcctVector3d.UnitX.AngleTo(OcctVector3d.UnitY);
        Check(Math.Abs(rightAngle - Math.PI / 2) < 1e-12, "Vector angle regression.");
        var vector = new OcctVector3d(2, 3, 4);
        Check(vector.ProjectOnto(OcctVector3d.UnitX).AlmostEquals(new OcctVector3d(2, 0, 0)), "Vector projection regression.");
        Check(vector.RejectFrom(OcctVector3d.UnitX).AlmostEquals(new OcctVector3d(0, 3, 4)), "Vector rejection regression.");

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
        Check(firstBounds.IsValid(), "Bounds validity regression.");
        Check(Math.Abs(firstBounds.GetVolume() - 6000) < 1e-12, "Bounds volume regression.");
        Check(firstBounds.Contains(new OcctPoint3d(5, 10, 15)), "Bounds containment regression.");
        Check(firstBounds.Intersects(secondBounds), "Bounds intersection regression.");
        var union = firstBounds.Union(secondBounds);
        Check(union.GetMinimumPoint() == new OcctPoint3d(0, -2, 0), "Bounds union minimum regression.");
        Check(union.GetMaximumPoint() == new OcctPoint3d(12, 20, 30), "Bounds union maximum regression.");
        var expanded = firstBounds.Expanded(2);
        Check(expanded.MinX == -2 && expanded.MaxZ == 32, "Bounds expansion regression.");

        var uv = new OcctUvBounds { UMin = -1, UMax = 3, VMin = 2, VMax = 6 };
        Check(uv.IsValid(), "UV validity regression.");
        Check(uv.GetCenter() == (1.0, 4.0), "UV center regression.");
        Check(uv.Contains(1, 4), "UV containment regression.");

        var distance = new OcctDistanceResult
        {
            Distance = 5,
            PointOnFirst = new OcctPoint3d(0, 0, 0),
            PointOnSecond = new OcctPoint3d(3, 4, 0)
        };
        Check(distance.IsFinite(), "Distance result finite-state regression.");
        Check(distance.GetSeparationVector() == new OcctVector3d(3, 4, 0), "Distance separation-vector regression.");
        Check(distance.GetMidpoint() == new OcctPoint3d(1.5, 2, 0), "Distance midpoint regression.");
        Check(distance.IsWithin(5) && !distance.IsWithin(4.999), "Distance threshold regression.");

        var translation = OcctGeometryExtensions.CreateTranslationLocation(10, 20, 30);
        Check(translation.TransformPoint(new OcctPoint3d(1, 2, 3)) == new OcctPoint3d(11, 22, 33), "Location point translation regression.");
        Check(translation.TransformVector(OcctVector3d.UnitX) == OcctVector3d.UnitX, "Location vector translation regression.");

        var rotation = OcctGeometryExtensions.CreateRotationLocation(OcctVector3d.UnitZ, Math.PI / 2);
        Check(rotation.TransformPoint(new OcctPoint3d(1, 0, 0)).AlmostEquals(new OcctPoint3d(0, 1, 0), 1e-12), "Location rotation regression.");

        var center = new OcctPoint3d(2, 3, 4);
        var scale = OcctGeometryExtensions.CreateUniformScaleLocation(2, center);
        Check(scale.TransformPoint(center).AlmostEquals(center, 1e-12), "Centered scale origin regression.");
        Check(scale.TransformPoint(new OcctPoint3d(3, 3, 4)).AlmostEquals(new OcctPoint3d(4, 3, 4), 1e-12), "Centered scale regression.");

        var composed = translation.Multiply(rotation);
        Check(composed.TransformPoint(new OcctPoint3d(1, 0, 0)).AlmostEquals(new OcctPoint3d(10, 21, 30), 1e-12), "Transform composition order regression.");
        var inverse = composed.Inverted();
        var source = new OcctPoint3d(7, -2, 5);
        Check(inverse.TransformPoint(composed.TransformPoint(source)).AlmostEquals(source, 1e-10), "Transform inverse round-trip regression.");

        var viewerTransform = composed.ToTransform3d();
        Check(viewerTransform.TransformPoint(source).AlmostEquals(composed.TransformPoint(source), 1e-12), "Viewer/model transform conversion regression.");
        var modelTransform = viewerTransform.ToModelLocation();
        Check(modelTransform.TransformPoint(source).AlmostEquals(composed.TransformPoint(source), 1e-12), "Model/viewer transform round-trip regression.");
        Check(!default(OcctModelLocation).TryInvert(out _), "Singular transform inversion regression.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            Assert.Fail(message);
    }
}
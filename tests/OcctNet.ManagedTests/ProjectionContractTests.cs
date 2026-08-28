using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class ProjectionContractTests
{
    [TestMethod]
    public void ProjectionResultLayoutsRemainBlittableAndStable()
    {
        var actualEdgeSize = Marshal.SizeOf<OcctEdgeProjectionResult>();
        var actualFaceSize = Marshal.SizeOf<OcctFaceProjectionResult>();
        Assert.AreEqual(64, actualEdgeSize);
        Assert.AreEqual(72, actualFaceSize);

        var edge = new OcctEdgeProjectionResult
        {
            Point = new OcctPoint3d(1, 2, 3),
            Tangent = OcctVector3d.UnitX,
            NormalizedParameter = 0.25,
            Distance = 4
        };
        Assert.AreEqual(new OcctPoint3d(1, 2, 3), edge.Point);
        Assert.AreEqual(OcctVector3d.UnitX, edge.Tangent);
        Assert.AreEqual(0.25, edge.NormalizedParameter);
        Assert.AreEqual(4.0, edge.Distance);

        var face = new OcctFaceProjectionResult
        {
            Point = new OcctPoint3d(5, 6, 7),
            Normal = OcctVector3d.UnitZ,
            U = 8,
            V = 9,
            Distance = 10
        };
        Assert.AreEqual(new OcctPoint3d(5, 6, 7), face.Point);
        Assert.AreEqual(OcctVector3d.UnitZ, face.Normal);
        Assert.AreEqual(8.0, face.U);
        Assert.AreEqual(9.0, face.V);
        Assert.AreEqual(10.0, face.Distance);
    }


    [TestMethod]
    public void ModelingSessionExposesBulkGeometryApis()
    {
        var type = typeof(OcctModelingSession);
        Assert.IsNotNull(type.GetMethod(nameof(OcctModelingSession.ProjectPointsOnEdge)));
        Assert.IsNotNull(type.GetMethod(nameof(OcctModelingSession.ProjectPointsOnFace)));
        Assert.IsNotNull(type.GetMethod(nameof(OcctModelingSession.GetDistances)));
        Assert.IsNotNull(type.GetMethod(nameof(OcctModelingSession.MakeCylinderFace)));
        Assert.IsNotNull(type.GetMethod(nameof(OcctModelingSession.MakeConeFace)));
        Assert.IsNotNull(type.GetMethod(nameof(OcctModelingSession.MakeSphereFace)));
        Assert.IsNotNull(type.GetMethod(nameof(OcctModelingSession.MakeTorusFace)));
        Assert.IsNotNull(type.GetMethod(nameof(OcctModelingSession.MakeHelix)));
        Assert.IsNotNull(type.GetMethod(nameof(OcctModelingSession.GetEdgeLengthAtParameter)));
    }
}

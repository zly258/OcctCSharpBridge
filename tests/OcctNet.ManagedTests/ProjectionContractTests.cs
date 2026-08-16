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
        Assert.AreEqual(40, actualEdgeSize);
        Assert.AreEqual(48, actualFaceSize);

        var edge = new OcctEdgeProjectionResult
        {
            Point = new OcctPoint3d(1, 2, 3),
            NormalizedParameter = 0.25,
            Distance = 4
        };
        Assert.AreEqual(new OcctPoint3d(1, 2, 3), edge.Point);
        Assert.AreEqual(0.25, edge.NormalizedParameter);
        Assert.AreEqual(4.0, edge.Distance);

        var face = new OcctFaceProjectionResult
        {
            Point = new OcctPoint3d(5, 6, 7),
            U = 8,
            V = 9,
            Distance = 10
        };
        Assert.AreEqual(new OcctPoint3d(5, 6, 7), face.Point);
        Assert.AreEqual(8.0, face.U);
        Assert.AreEqual(9.0, face.V);
        Assert.AreEqual(10.0, face.Distance);
    }
}

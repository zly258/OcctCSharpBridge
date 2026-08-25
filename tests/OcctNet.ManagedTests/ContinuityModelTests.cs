using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class ContinuityModelTests
{
    [TestMethod]
    public void ContinuityDefaultsAreFiniteAndPositive()
    {
        var options = OcctCurveContinuityOptions.Default;

        Assert.IsTrue(options.PositionTolerance > 0);
        Assert.IsTrue(options.AngularTolerance > 0);
        Assert.IsTrue(options.CurvatureTolerance > 0);
        Assert.IsTrue(options.FirstDerivativeTolerance > 0);
        Assert.IsTrue(options.SecondDerivativeTolerance > 0);
    }

    [TestMethod]
    public void ContinuityLevelsPreserveCAndGOrders()
    {
        Assert.AreEqual(0, (int)OcctContinuityLevel.None);
        Assert.AreEqual(1, (int)OcctContinuityLevel.Order0);
        Assert.AreEqual(2, (int)OcctContinuityLevel.Order1);
        Assert.AreEqual(3, (int)OcctContinuityLevel.Order2);
    }
    [TestMethod]
    public void SurfaceQualityDefaultsAreBounded()
    {
        var options = OcctSurfaceQualityOptions.Default;

        Assert.IsTrue(options.USamples >= 2);
        Assert.IsTrue(options.VSamples >= 2);
        Assert.IsTrue(options.Resolution > 0);
        Assert.IsTrue(options.ZebraFrequency > 0);
        Assert.IsTrue(options.ViewDirection.IsFinite);
        Assert.IsTrue(options.ViewDirection.LengthSquared > 0);
    }

}

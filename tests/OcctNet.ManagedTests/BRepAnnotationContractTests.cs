using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class BRepAnnotationContractTests
{
    [TestMethod]
    public void BRepTextDefaultsRemainCrossPlatformAndHeadless()
    {
        var options = OcctBRepTextOptions.Default;

        Assert.AreEqual(OcctPoint3d.Origin, options.Position);
        Assert.AreEqual(OcctVector3d.UnitZ, options.Normal);
        Assert.AreEqual(OcctVector3d.UnitX, options.XDirection);
        Assert.AreEqual(10d, options.Height);
        Assert.AreEqual(0d, options.ExtrusionDepth);
        Assert.AreEqual(string.Empty, options.FontName);
        Assert.IsFalse(options.Bold);
        Assert.IsFalse(options.Italic);
        Assert.AreEqual(OcctTextHorizontalAlignment.Left, options.HorizontalAlignment);
        Assert.AreEqual(OcctTextVerticalAlignment.Bottom, options.VerticalAlignment);
    }

    [TestMethod]
    public void BRepAnnotationDefaultsRemainStable()
    {
        var options = OcctBRepAnnotationOptions.Default;

        Assert.AreEqual(20d, options.Offset);
        Assert.AreEqual(5d, options.TextHeight);
        Assert.AreEqual(3d, options.ArrowSize);
        Assert.AreEqual(string.Empty, options.FontName);
    }

    [TestMethod]
    public void TextAlignmentNativeValuesRemainStable()
    {
        Assert.AreEqual(0, (int)OcctTextHorizontalAlignment.Left);
        Assert.AreEqual(1, (int)OcctTextHorizontalAlignment.Center);
        Assert.AreEqual(2, (int)OcctTextHorizontalAlignment.Right);
        Assert.AreEqual(0, (int)OcctTextVerticalAlignment.Bottom);
        Assert.AreEqual(1, (int)OcctTextVerticalAlignment.Center);
        Assert.AreEqual(2, (int)OcctTextVerticalAlignment.Top);
    }
}

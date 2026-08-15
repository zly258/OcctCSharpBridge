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
        AssertEnumValue(0, OcctTextHorizontalAlignment.Left);
        AssertEnumValue(1, OcctTextHorizontalAlignment.Center);
        AssertEnumValue(2, OcctTextHorizontalAlignment.Right);
        AssertEnumValue(0, OcctTextVerticalAlignment.Bottom);
        AssertEnumValue(1, OcctTextVerticalAlignment.Center);
        AssertEnumValue(2, OcctTextVerticalAlignment.Top);
    }

    private static void AssertEnumValue<TEnum>(int expected, TEnum actual)
        where TEnum : struct, Enum
    {
        Assert.AreEqual(expected, Convert.ToInt32(actual));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class BridgeCompatibilityTests
{
    [TestMethod]
    public void SameOrNewerNativeVersionIsAcceptedWithinAbi()
    {
        Assert.IsTrue(OcctBridgeInfo.IsNativeVersionCompatible("2.7.0", "2.7.0"));
        Assert.IsTrue(OcctBridgeInfo.IsNativeVersionCompatible("2.8.0", "2.7.0"));
    }

    [TestMethod]
    public void OlderOrInvalidNativeVersionIsRejected()
    {
        Assert.IsFalse(OcctBridgeInfo.IsNativeVersionCompatible("2.6.0", "2.7.0"));
        Assert.IsFalse(OcctBridgeInfo.IsNativeVersionCompatible("invalid", "2.7.0"));
    }
}

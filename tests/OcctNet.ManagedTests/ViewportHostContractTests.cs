using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class ViewportHostContractTests
{
    [TestMethod]
    public void HostStateChangedCarriesStateAndGeneration()
    {
        var args = new OcctViewportHostStateChangedEventArgs(
            OcctViewportHostState.Initializing,
            OcctViewportHostState.Ready,
            3);

        Assert.AreEqual(OcctViewportHostState.Initializing, args.PreviousState);
        Assert.AreEqual(OcctViewportHostState.Ready, args.State);
        Assert.AreEqual(3L, args.Generation);
    }

    [TestMethod]
    public void FaultedCarriesExceptionAndGeneration()
    {
        var exception = new InvalidOperationException("viewer failed");
        var args = new OcctViewportFaultedEventArgs(exception, 2);

        Assert.AreSame(exception, args.Exception);
        Assert.AreEqual(2L, args.Generation);
    }

    [TestMethod]
    public void HostLifecycleArgumentsRejectInvalidGenerations()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new OcctViewportHostStateChangedEventArgs(
                OcctViewportHostState.Detached,
                OcctViewportHostState.Initializing,
                -1));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new OcctViewportFaultedEventArgs(new InvalidOperationException(), -1));
    }

    [TestMethod]
    public void HostStateNamesDoNotOverlapCameraViewportState()
    {
        Assert.AreEqual(nameof(OcctViewportHostState), typeof(OcctViewportHostState).Name);
        Assert.AreEqual(nameof(OcctViewportState), typeof(OcctViewportState).Name);
        Assert.AreNotEqual(typeof(OcctViewportHostState), typeof(OcctViewportState));
    }
}

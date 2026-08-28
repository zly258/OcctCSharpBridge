using System.Drawing;
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
    public void FirstFrameCarriesEngineGeneration()
    {
        var args = new OcctFirstFrameRenderedEventArgs(4);
        Assert.AreEqual(4L, args.Generation);
    }

    [TestMethod]
    public void NativeHandleChangedCarriesBothHandlesAndGeneration()
    {
        var previous = new IntPtr(123);
        var current = new IntPtr(456);
        var args = new OcctNativeHandleChangedEventArgs(previous, current, 5);

        Assert.AreEqual(previous, args.PreviousHandle);
        Assert.AreEqual(current, args.NativeHandle);
        Assert.AreEqual(5L, args.Generation);
    }


    [TestMethod]
    public void DefaultInitialOptionsMatchBridgeViewerDefaults()
    {
        var options = new OcctViewportInitializationOptions();

        Assert.AreEqual(Color.FromArgb(240, 245, 250), options.BackgroundColor);
        Assert.AreEqual(OcctViewOrientation.Isometric, options.ViewOrientation);
        Assert.AreEqual(OcctProjectionType.Orthographic, options.Projection);
        Assert.IsTrue(options.TriedronVisible);
        Assert.IsTrue(options.ViewCubeVisible);
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

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new OcctFirstFrameRenderedEventArgs(0));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new OcctNativeHandleChangedEventArgs(IntPtr.Zero, new IntPtr(1), 0));
    }

    [TestMethod]
    public void HostStateNamesDoNotOverlapCameraViewportState()
    {
        var actualHostStateName = typeof(OcctViewportHostState).Name;
        var actualCameraStateName = typeof(OcctViewportState).Name;
        var actualHostStateType = typeof(OcctViewportHostState);
        var actualCameraStateType = typeof(OcctViewportState);

        Assert.AreEqual(nameof(OcctViewportHostState), actualHostStateName);
        Assert.AreEqual(nameof(OcctViewportState), actualCameraStateName);
        Assert.AreNotEqual(actualHostStateType, actualCameraStateType);
    }
}

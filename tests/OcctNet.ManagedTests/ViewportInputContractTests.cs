using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class ViewportInputContractTests
{
    [TestMethod]
    public void InteractionFeatureGroupsRemainStable()
    {
        var expectedSelection =
            OcctViewportInteractionFeatures.PointSelection |
            OcctViewportInteractionFeatures.RectangleSelection;
        var expectedNavigation =
            OcctViewportInteractionFeatures.Rotate |
            OcctViewportInteractionFeatures.Pan |
            OcctViewportInteractionFeatures.Zoom;

        Assert.AreEqual(expectedSelection, OcctViewportInteractionFeatures.Selection);
        Assert.AreEqual(expectedNavigation, OcctViewportInteractionFeatures.Navigation);
        Assert.AreEqual(
            OcctViewportInteractionFeatures.HoverDetection | expectedSelection | expectedNavigation,
            OcctViewportInteractionFeatures.Default);
    }

    [TestMethod]
    public void PointerInputCarriesPlatformNeutralState()
    {
        var input = new OcctPointerInputEventArgs(
            OcctPointerInputKind.Wheel,
            OcctPointerButton.None,
            OcctPointerButtons.Left | OcctPointerButtons.Middle,
            120,
            240,
            120,
            OcctInputModifiers.Control | OcctInputModifiers.Shift);

        Assert.AreEqual(OcctPointerInputKind.Wheel, input.Kind);
        Assert.AreEqual(OcctPointerButton.None, input.Button);
        Assert.AreEqual(OcctPointerButtons.Left | OcctPointerButtons.Middle, input.Buttons);
        Assert.AreEqual(120, input.X);
        Assert.AreEqual(240, input.Y);
        Assert.AreEqual(120, input.WheelDelta);
        Assert.AreEqual(OcctInputModifiers.Control | OcctInputModifiers.Shift, input.Modifiers);
        Assert.IsFalse(input.Handled);

        input.Handled = true;
        Assert.IsTrue(input.Handled);
    }

    [TestMethod]
    public void KeyInputCarriesHandledAndRepeatState()
    {
        var input = new OcctKeyInputEventArgs(
            OcctKeyInputKind.Pressed,
            OcctKey.F8,
            OcctInputModifiers.Control,
            isRepeat: true);

        Assert.AreEqual(OcctKeyInputKind.Pressed, input.Kind);
        Assert.AreEqual(OcctKey.F8, input.Key);
        Assert.AreEqual(OcctInputModifiers.Control, input.Modifiers);
        Assert.IsTrue(input.IsRepeat);
        Assert.IsFalse(input.Handled);

        input.Handled = true;
        Assert.IsTrue(input.Handled);
    }

    [TestMethod]
    public void HoverTrackerChangesOnlyWhenDetectedIdentityChanges()
    {
        var tracker = new OcctViewportHoverTracker();
        IOcctObject owner = new TestObject(17, OcctObjectKind.Shape);
        var first = new OcctSelectionHitDetail(
            owner,
            OcctShapeType.Face,
            3,
            new OcctPoint3d(1, 2, 3),
            10,
            20);
        var sameIdentityDifferentGeometry = first with
        {
            Point = new OcctPoint3d(5, 6, 7),
            Depth = 11,
            DistanceToEye = 21
        };
        var differentSubshape = first with { SubshapeIndex = 4 };

        Assert.IsTrue(tracker.Update(first));
        Assert.IsFalse(tracker.Update(sameIdentityDifferentGeometry));
        Assert.IsTrue(tracker.Update(differentSubshape));
        Assert.IsTrue(tracker.Update(null));
        Assert.IsFalse(tracker.Update(null));
    }

    [TestMethod]
    public void HoverEventCarriesScreenPositionAndOptionalHit()
    {
        IOcctObject owner = new TestObject(23, OcctObjectKind.Shape);
        var hit = new OcctSelectionHitDetail(
            owner,
            OcctShapeType.Edge,
            2,
            new OcctPoint3d(10, 20, 30),
            4,
            5);
        var args = new OcctViewportHoverHitChangedEventArgs(120, 240, hit);

        Assert.AreEqual(120, args.ScreenX);
        Assert.AreEqual(240, args.ScreenY);
        Assert.AreEqual(hit, args.Hit);
        Assert.IsNotNull(typeof(IOcctViewportInputSource).GetEvent(nameof(IOcctViewportInputSource.HoverHitChanged)));
    }

    [TestMethod]
    public void InputArgumentsRejectUnknownFlagBits()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new OcctPointerInputEventArgs(
                OcctPointerInputKind.Moved,
                OcctPointerButton.None,
                (OcctPointerButtons)(1 << 12),
                0,
                0,
                0,
                OcctInputModifiers.None));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new OcctKeyInputEventArgs(
                OcctKeyInputKind.Pressed,
                OcctKey.Enter,
                (OcctInputModifiers)(1 << 12)));
    }

    private sealed class TestObject(long id, OcctObjectKind kind) : IOcctObject
    {
        public long Id { get; } = id;
        public OcctObjectKind Kind { get; } = kind;
        public bool IsValid => Id > 0;
    }
}

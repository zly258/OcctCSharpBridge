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
}

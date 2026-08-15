using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetObjectAppearance(IOcctObject value, OcctObjectAppearance appearance)
    {
        ArgumentNullException.ThrowIfNull(appearance);
        if (!double.IsFinite(appearance.Transparency) ||
            appearance.Transparency < 0.0 ||
            appearance.Transparency > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(appearance), "Transparency must be finite and in the range [0, 1].");
        }
        OcctGuard.Positive(appearance.LineWidth, nameof(appearance));
        if (!Enum.IsDefined(appearance.DisplayMode))
            throw new ArgumentOutOfRangeException(nameof(appearance), "DisplayMode is not supported by this SDK.");
        if (!Enum.IsDefined(appearance.Material))
            throw new ArgumentOutOfRangeException(nameof(appearance), "Material is not supported by this SDK.");
        EnsureObject(value);

        var mask = NativeViewerObjectUpdateMask.Color |
                   NativeViewerObjectUpdateMask.Transparency |
                   NativeViewerObjectUpdateMask.Visibility |
                   NativeViewerObjectUpdateMask.LineWidth |
                   NativeViewerObjectUpdateMask.Material;
        var options = ObjectUpdateOptions(mask);
        options.Color = ToNativeObjectColor(appearance.Color);
        options.Transparency = appearance.Transparency;
        options.Visible = appearance.Visible ? 1 : 0;
        options.LineWidth = appearance.LineWidth;
        options.Material = (int)appearance.Material;
        UpdateObject(value.Id, options);
        SetDisplayModeOverride(value, appearance.DisplayMode);
    }

    public void SetObjectColor(IOcctObject value, Color color)
    {
        EnsureObject(value);
        var options = ObjectUpdateOptions(NativeViewerObjectUpdateMask.Color);
        options.Color = ToNativeObjectColor(color);
        UpdateObject(value.Id, options);
    }

    public void SetObjectTransparency(IOcctObject value, double transparency)
    {
        EnsureObject(value);
        if (!double.IsFinite(transparency) || transparency < 0.0 || transparency > 1.0)
            throw new ArgumentOutOfRangeException(nameof(transparency));
        var options = ObjectUpdateOptions(NativeViewerObjectUpdateMask.Transparency);
        options.Transparency = transparency;
        UpdateObject(value.Id, options);
    }

    public void SetObjectVisible(IOcctObject value, bool visible)
    {
        EnsureObject(value);
        var options = ObjectUpdateOptions(NativeViewerObjectUpdateMask.Visibility);
        options.Visible = visible ? 1 : 0;
        UpdateObject(value.Id, options);
    }

    public void SetObjectDisplayMode(IOcctObject value, OcctDisplayMode displayMode) =>
        SetDisplayModeOverride(value, displayMode);

    public void SetObjectLineWidth(IOcctObject value, double width)
    {
        EnsureObject(value);
        OcctGuard.Positive(width, nameof(width));
        var options = ObjectUpdateOptions(NativeViewerObjectUpdateMask.LineWidth);
        options.LineWidth = width;
        UpdateObject(value.Id, options);
    }

    public void SetObjectMaterial(IOcctObject value, OcctMaterial material)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(material)) throw new ArgumentOutOfRangeException(nameof(material));
        var options = ObjectUpdateOptions(NativeViewerObjectUpdateMask.Material);
        options.Material = (int)material;
        UpdateObject(value.Id, options);
    }

    private static NativeViewerObjectUpdateOptions ObjectUpdateOptions(
        NativeViewerObjectUpdateMask mask) => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeViewerObjectUpdateOptions>(),
        ApiVersion = 1,
        UpdateMask = mask,
        LineWidth = 1.0
    };

    private static NativeViewColorRgb ToNativeObjectColor(Color value) => new()
    {
        R = value.R / 255.0,
        G = value.G / 255.0,
        B = value.B / 255.0
    };
}

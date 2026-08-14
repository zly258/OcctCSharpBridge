using System.Drawing;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetColor(IOcctObject value, Color color)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_set_object_color(
            _handle,
            value.Id,
            color.R / 255.0,
            color.G / 255.0,
            color.B / 255.0));
    }

    public void SetTransparency(IOcctObject value, double transparency)
    {
        EnsureObject(value);
        OcctGuard.UnitInterval(transparency, nameof(transparency));
        CheckInitialized(() => NativeMethods.occt_set_object_transparency(_handle, value.Id, transparency));
    }

    public void SetVisible(IOcctObject value, bool visible)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_set_object_visible(_handle, value.Id, visible ? 1 : 0));
    }

    public void SetDisplayMode(IOcctObject value, OcctDisplayMode displayMode) =>
        SetDisplayModeOverride(value, displayMode);

    public void SetLineWidth(IOcctObject value, double width)
    {
        EnsureObject(value);
        OcctGuard.Positive(width, nameof(width));
        CheckInitialized(() => NativeMethods.occt_set_object_line_width(_handle, value.Id, width));
    }

    public void SetMaterial(IOcctObject value, OcctMaterial material)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(material)) throw new ArgumentOutOfRangeException(nameof(material));
        CheckInitialized(() => NativeMethods.occt_set_object_material(_handle, value.Id, (int)material));
    }

    public void ShowAll() => CheckInitialized(() => NativeMethods.occt_show_all(_handle));
    public void HideAll() => CheckInitialized(() => NativeMethods.occt_hide_all(_handle));

    public void Redisplay(IOcctObject value)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_redisplay_object(_handle, value.Id));
    }
}

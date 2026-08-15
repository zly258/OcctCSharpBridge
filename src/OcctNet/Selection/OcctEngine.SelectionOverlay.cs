using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    /// <summary>
    /// Displays or updates an OCCT-native 2D rubber-band rectangle in the top overlay layer.
    /// Coordinates use window client pixels with the origin at the upper-left corner.
    /// </summary>
    public void ShowSelectionRectangle(
        int x1,
        int y1,
        int x2,
        int y2,
        Color lineColor,
        Color fillColor,
        double fillTransparency = 0.82,
        double lineWidth = 1.0)
    {
        if (!double.IsFinite(fillTransparency) || fillTransparency is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(fillTransparency), "Transparency must be between 0 and 1.");
        if (!double.IsFinite(lineWidth) || lineWidth <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(lineWidth), "Line width must be greater than zero.");

        var options = new NativeViewerSelectionRectangleOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerSelectionRectangleOptions>(),
            ApiVersion = 1,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            LineColor = ToNativeViewColor(lineColor),
            FillColor = ToNativeViewColor(fillColor),
            FillTransparency = fillTransparency,
            LineWidth = lineWidth
        };
        EnsureInitialized();
        CheckSelectionStatus(SelectionOverlayNativeMethods.occt_engine_selection_rectangle_overlay_show(
            _handle,
            in options));
    }

    /// <summary>Removes the OCCT-native rubber-band selection overlay.</summary>
    public void HideSelectionRectangle()
    {
        EnsureInitialized();
        CheckSelectionStatus(SelectionOverlayNativeMethods.occt_engine_selection_rectangle_overlay_hide(_handle));
    }
}

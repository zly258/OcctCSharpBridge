using System.Drawing;

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
        if (fillTransparency is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(fillTransparency), "Transparency must be between 0 and 1.");
        if (lineWidth <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(lineWidth), "Line width must be greater than zero.");

        CheckInitialized(() => SelectionNativeMethods.occt_show_selection_rectangle(
            _handle,
            x1,
            y1,
            x2,
            y2,
            lineColor.R / 255.0,
            lineColor.G / 255.0,
            lineColor.B / 255.0,
            fillColor.R / 255.0,
            fillColor.G / 255.0,
            fillColor.B / 255.0,
            fillTransparency,
            lineWidth));
    }

    /// <summary>Removes the OCCT-native rubber-band selection overlay.</summary>
    public void HideSelectionRectangle() =>
        CheckInitialized(() => SelectionNativeMethods.occt_hide_selection_rectangle(_handle));
}

namespace OcctDemo.Common;

public static class DemoViewportSettings
{
    private const double DefaultZoomSensitivity = 1.0;
    private const double MinimumZoomSensitivity = 0.1;
    private const double MaximumZoomSensitivity = 5.0;

    public static double GetZoomSensitivity(object viewport)
    {
        ArgumentNullException.ThrowIfNull(viewport);
        var property = viewport.GetType().GetProperty("ZoomSensitivity");
        return property?.CanRead == true && property.GetValue(viewport) is double value && double.IsFinite(value)
            ? value
            : DefaultZoomSensitivity;
    }

    public static bool TrySetZoomSensitivity(object viewport, double value)
    {
        ArgumentNullException.ThrowIfNull(viewport);
        if (!double.IsFinite(value)) return false;

        var property = viewport.GetType().GetProperty("ZoomSensitivity");
        if (property?.CanWrite != true || property.PropertyType != typeof(double)) return false;
        property.SetValue(viewport, Math.Clamp(value, MinimumZoomSensitivity, MaximumZoomSensitivity));
        return true;
    }
}

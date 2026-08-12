using System.Drawing;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetSelectionHighlightColor(Color color) => CheckInitialized(() =>
        AppearanceNativeMethods.occt_set_selection_highlight_color(
            _handle,
            color.R / 255.0,
            color.G / 255.0,
            color.B / 255.0));

    public void SetHoverHighlightColor(Color color) => CheckInitialized(() =>
        AppearanceNativeMethods.occt_set_hover_highlight_color(
            _handle,
            color.R / 255.0,
            color.G / 255.0,
            color.B / 255.0));

    public void SetSceneLighting(OcctSceneLightingSettings settings)
    {
        ValidateLightIntensity(settings.AmbientIntensity, nameof(settings.AmbientIntensity));
        ValidateDirectionalLight(settings.CameraLight, nameof(settings.CameraLight));
        ValidateDirectionalLight(settings.SunLight, nameof(settings.SunLight));
        ValidateDirectionalLight(settings.FillLight, nameof(settings.FillLight));

        var native = new AppearanceNativeMethods.NativeSceneLightingSettings
        {
            AmbientColor = ToNativeColor(settings.AmbientColor),
            AmbientIntensity = settings.AmbientIntensity,
            CameraLightEnabled = settings.CameraLight.Enabled ? 1 : 0,
            CameraLightColor = ToNativeColor(settings.CameraLight.Color),
            CameraLightIntensity = settings.CameraLight.Intensity,
            CameraLightDirection = settings.CameraLight.Direction,
            SunLightEnabled = settings.SunLight.Enabled ? 1 : 0,
            SunLightColor = ToNativeColor(settings.SunLight.Color),
            SunLightIntensity = settings.SunLight.Intensity,
            SunLightDirection = settings.SunLight.Direction,
            FillLightEnabled = settings.FillLight.Enabled ? 1 : 0,
            FillLightColor = ToNativeColor(settings.FillLight.Color),
            FillLightIntensity = settings.FillLight.Intensity,
            FillLightDirection = settings.FillLight.Direction
        };

        CheckInitialized(() => AppearanceNativeMethods.occt_set_scene_lighting_ex(_handle, in native));
    }

    public void ApplyLightingPreset(OcctLightingPreset preset) =>
        SetSceneLighting(OcctLightingPresets.Create(preset));

    private static AppearanceNativeMethods.NativeColorRgb ToNativeColor(Color color) => new()
    {
        R = color.R / 255.0,
        G = color.G / 255.0,
        B = color.B / 255.0
    };

    private static void ValidateDirectionalLight(OcctDirectionalLightSettings light, string name)
    {
        ValidateLightIntensity(light.Intensity, $"{name}.{nameof(light.Intensity)}");
        if (light.Enabled && light.Direction.Length <= 1e-12)
        {
            throw new ArgumentException("Enabled directional lights require a non-zero direction.", name);
        }
    }

    private static void ValidateLightIntensity(double value, string name)
    {
        if (!double.IsFinite(value) || value < 0.0 || value > 10.0)
        {
            throw new ArgumentOutOfRangeException(name, value, "Light intensity must be between 0 and 10.");
        }
    }
}

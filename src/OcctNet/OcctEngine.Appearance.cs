using System.Drawing;
using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetSelectionHighlightColor(Color color)
    {
        EnsureInitialized();
        var options = HighlightOptions(
            NativeViewerHighlightUpdateMask.Selection,
            selectionColor: color);
        CheckAppearanceStatus(
            AppearanceNativeMethods.occt_engine_highlight_colors_set(_handle, in options));
    }

    public void SetHoverHighlightColor(Color color)
    {
        EnsureInitialized();
        var options = HighlightOptions(
            NativeViewerHighlightUpdateMask.Hover,
            hoverColor: color);
        CheckAppearanceStatus(
            AppearanceNativeMethods.occt_engine_highlight_colors_set(_handle, in options));
    }

    public void SetSceneLighting(OcctSceneLightingSettings settings)
    {
        ValidateLightIntensity(settings.AmbientIntensity, nameof(settings.AmbientIntensity));
        ValidateDirectionalLight(settings.CameraLight, nameof(settings.CameraLight));
        ValidateDirectionalLight(settings.SunLight, nameof(settings.SunLight));
        ValidateDirectionalLight(settings.FillLight, nameof(settings.FillLight));

        var nativeSettings = new AppearanceNativeMethods.NativeSceneLightingSettings
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
        var options = new AppearanceNativeMethods.NativeViewerLightingOptions
        {
            StructSize = (uint)Marshal.SizeOf<AppearanceNativeMethods.NativeViewerLightingOptions>(),
            ApiVersion = 1,
            Settings = nativeSettings
        };

        EnsureInitialized();
        CheckAppearanceStatus(
            AppearanceNativeMethods.occt_engine_scene_lighting_set(_handle, in options));
    }

    public void ResetSceneLighting()
    {
        EnsureInitialized();
        CheckAppearanceStatus(AppearanceNativeMethods.occt_engine_scene_lighting_reset(_handle));
    }

    public void ApplyLightingPreset(OcctLightingPreset preset) =>
        SetSceneLighting(OcctLightingPresets.Create(preset));

    private static AppearanceNativeMethods.NativeViewerHighlightOptions HighlightOptions(
        NativeViewerHighlightUpdateMask updateMask,
        Color? selectionColor = null,
        Color? hoverColor = null) => new()
    {
        StructSize = (uint)Marshal.SizeOf<AppearanceNativeMethods.NativeViewerHighlightOptions>(),
        ApiVersion = 1,
        UpdateMask = updateMask,
        SelectionColor = ToNativeColor(selectionColor ?? Color.Black),
        HoverColor = ToNativeColor(hoverColor ?? Color.Black)
    };

    private static AppearanceNativeMethods.NativeColorRgb ToNativeColor(Color color) => new()
    {
        R = color.R / 255.0,
        G = color.G / 255.0,
        B = color.B / 255.0
    };

    private void CheckAppearanceStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }

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

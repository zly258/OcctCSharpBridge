using System.Drawing;

namespace OcctNet;

public enum OcctLineStyle
{
    Solid = 0,
    Dash = 1,
    Dot = 2,
    DotDash = 3,
    Center = 4
}

public enum OcctLightingPreset
{
    Neutral = 0,
    Studio = 1,
    Sunlight = 2,
    Flat = 3
}

public readonly record struct OcctDirectionalLightSettings(
    bool Enabled,
    Color Color,
    double Intensity,
    OcctVector3d Direction);

public readonly record struct OcctSceneLightingSettings(
    Color AmbientColor,
    double AmbientIntensity,
    OcctDirectionalLightSettings CameraLight,
    OcctDirectionalLightSettings SunLight,
    OcctDirectionalLightSettings FillLight);

public static class OcctLightingPresets
{
    public static OcctSceneLightingSettings Create(OcctLightingPreset preset)
    {
        return preset switch
        {
            OcctLightingPreset.Neutral => new(
                Color.White,
                0.45,
                new(true, Color.White, 0.90, new OcctVector3d(0, 0, -1)),
                new(false, Color.White, 0.0, new OcctVector3d(-1, -1, -2)),
                new(false, Color.White, 0.0, new OcctVector3d(1, 1, -1))),
            OcctLightingPreset.Sunlight => new(
                Color.FromArgb(245, 248, 255),
                0.25,
                new(true, Color.White, 0.25, new OcctVector3d(0, 0, -1)),
                new(true, Color.FromArgb(255, 242, 210), 1.40, new OcctVector3d(-1, -0.6, -1.8)),
                new(true, Color.FromArgb(190, 215, 255), 0.20, new OcctVector3d(1, 0.5, -1))),
            OcctLightingPreset.Flat => new(
                Color.White,
                0.85,
                new(true, Color.White, 0.25, new OcctVector3d(0, 0, -1)),
                new(false, Color.White, 0.0, new OcctVector3d(-1, -1, -2)),
                new(false, Color.White, 0.0, new OcctVector3d(1, 1, -1))),
            _ => new(
                Color.FromArgb(248, 250, 255),
                0.30,
                new(true, Color.White, 0.85, new OcctVector3d(0, 0, -1)),
                new(true, Color.FromArgb(255, 244, 220), 0.75, new OcctVector3d(-1, -1, -2)),
                new(true, Color.FromArgb(195, 220, 255), 0.35, new OcctVector3d(1, 0.5, -1)))
        };
    }
}

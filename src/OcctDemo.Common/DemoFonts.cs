namespace OcctDemo.Common;

/// <summary>
/// Provides cross-platform font name resolution for OCCT 3D text/annotations
/// and UI controls across Windows, Linux, and macOS.
/// </summary>
public static class DemoFonts
{
    /// <summary>
    /// OCCT standard font alias resolved by Font_FontMgr on every supported platform.
    /// </summary>
    public const string OcctSansSerif = "sans-serif";

    /// <summary>
    /// Default fallback font family name string for Avalonia UI controls.
    /// </summary>
    public const string DefaultUiFontFamily = "Segoe UI, Microsoft YaHei, Ubuntu, Noto Sans CJK SC, WenQuanYi Zen Hei, DejaVu Sans, sans-serif";

    /// <summary>
    /// Resolves font names to OCCT-compatible font aliases.
    /// </summary>
    public static string ResolveOcctFont(string? fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName)) return OcctSansSerif;
        var value = fontName.Trim();
        if (value.Equals("Microsoft YaHei UI", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Microsoft YaHei", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase))
        {
            return OcctSansSerif;
        }

        return value;
    }
}

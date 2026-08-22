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
    /// Resolves font names to valid system TrueType fonts supporting CJK and Latin characters.
    /// </summary>
    public static string ResolveOcctFont(string? fontName)
    {
        if (OperatingSystem.IsWindows())
        {
            var fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            if (!string.IsNullOrWhiteSpace(fontName))
            {
                var trimmed = fontName.Trim();
                if (File.Exists(trimmed)) return trimmed;

                if (trimmed.Equals("SimSun", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("宋体", StringComparison.OrdinalIgnoreCase))
                {
                    var simsun = Path.Combine(fontsDir, "simsun.ttc");
                    if (File.Exists(simsun)) return simsun;
                }
                if (trimmed.Equals("SimHei", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("黑体", StringComparison.OrdinalIgnoreCase))
                {
                    var simhei = Path.Combine(fontsDir, "simhei.ttf");
                    if (File.Exists(simhei)) return simhei;
                }
                if (trimmed.Equals("Microsoft YaHei", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Equals("Microsoft YaHei UI", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Equals("微软雅黑", StringComparison.OrdinalIgnoreCase))
                {
                    var msyh = Path.Combine(fontsDir, "msyh.ttc");
                    if (File.Exists(msyh)) return msyh;
                }
            }

            // Default Windows fallback: Microsoft YaHei has complete CJK + Latin glyphs
            var defaultMsyh = Path.Combine(fontsDir, "msyh.ttc");
            if (File.Exists(defaultMsyh)) return defaultMsyh;
            var defaultSimsun = Path.Combine(fontsDir, "simsun.ttc");
            if (File.Exists(defaultSimsun)) return defaultSimsun;
        }

        if (string.IsNullOrWhiteSpace(fontName)) return OcctSansSerif;
        return fontName.Trim();
    }
}

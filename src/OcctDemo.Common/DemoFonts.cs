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
        if (string.IsNullOrWhiteSpace(fontName))
        {
            return OperatingSystem.IsWindows() ? "Microsoft YaHei" : OcctSansSerif;
        }

        var trimmed = fontName.Trim();

        // If it's an existing physical font file path, keep it
        if (File.Exists(trimmed))
        {
            return trimmed;
        }

        // Map Chinese / Windows font aliases to standard family names recognized by OCCT Font_FontMgr
        if (trimmed.Equals("Microsoft YaHei", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("Microsoft YaHei UI", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("微软雅黑", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("msyh", StringComparison.OrdinalIgnoreCase))
        {
            return OperatingSystem.IsWindows() ? "Microsoft YaHei" : "Noto Sans CJK SC";
        }

        if (trimmed.Equals("SimSun", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("宋体", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("simsun", StringComparison.OrdinalIgnoreCase))
        {
            return OperatingSystem.IsWindows() ? "SimSun" : "Noto Serif CJK SC";
        }

        if (trimmed.Equals("SimHei", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("黑体", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("simhei", StringComparison.OrdinalIgnoreCase))
        {
            return OperatingSystem.IsWindows() ? "SimHei" : "Noto Sans CJK SC";
        }

        if (trimmed.Equals("KaiTi", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("楷体", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("simkai", StringComparison.OrdinalIgnoreCase))
        {
            return OperatingSystem.IsWindows() ? "KaiTi" : "AR PL UKai CN";
        }

        if (trimmed.Equals("FangSong", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("仿宋", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("simfang", StringComparison.OrdinalIgnoreCase))
        {
            return OperatingSystem.IsWindows() ? "FangSong" : "Noto Serif CJK SC";
        }

        return trimmed;
    }
}

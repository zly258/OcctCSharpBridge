namespace OcctDemo.Common;

public static class DemoFonts
{
    /// <summary>
    /// OCCT standard font alias resolved by Font_FontMgr on every supported platform.
    /// </summary>
    public const string OcctSansSerif = "sans-serif";

    public static string ResolveOcctFont(string? fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName)) return OcctSansSerif;
        var value = fontName.Trim();
        return value.Equals("Microsoft YaHei UI", StringComparison.OrdinalIgnoreCase)
            ? OcctSansSerif
            : value;
    }
}

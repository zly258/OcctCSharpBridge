using OcctNet;

namespace OcctDemo.Common;

public static class DemoViewportStatus
{
    public static string Hover(OcctViewportHoverHitChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Hit is not { } hit)
        {
            return DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
                ? "悬停：无"
                : "Hover: none";
        }

        return DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? $"悬停：对象 {hit.Owner.Id} / {hit.SubshapeType} #{hit.SubshapeIndex}"
            : $"Hover: object {hit.Owner.Id} / {hit.SubshapeType} #{hit.SubshapeIndex}";
    }
}

using System.Globalization;
using System.Windows;

namespace OcctScript.Editor;

internal static class LanguageService
{
    public const string English = "en-US";
    public const string Chinese = "zh-CN";
    public static string CurrentCulture { get; private set; } = English;

    public static void Apply(string cultureName)
    {
        if (cultureName is not (English or Chinese))
            throw new ArgumentOutOfRangeException(nameof(cultureName));

        var resources = Application.Current.Resources.MergedDictionaries;
        for (var index = resources.Count - 1; index >= 0; index--)
        {
            var source = resources[index].Source?.OriginalString ?? string.Empty;
            if (source.Contains("Resources/Strings.", StringComparison.OrdinalIgnoreCase))
                resources.RemoveAt(index);
        }

        resources.Add(new ResourceDictionary { Source = new Uri($"Resources/Strings.{cultureName}.xaml", UriKind.Relative) });
        resources.Add(new ResourceDictionary { Source = new Uri($"Resources/Strings.Script.{cultureName}.xaml", UriKind.Relative) });
        CurrentCulture = cultureName;
        var culture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}

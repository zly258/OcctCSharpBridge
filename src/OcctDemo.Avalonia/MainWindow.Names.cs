using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private static string SelectionModeName(OcctSelectionMode mode) => DemoLocalization.SelectionMode(mode);

    private static string LightingPresetName(OcctLightingPreset preset) => preset switch
    {
        OcctLightingPreset.Neutral => Local("Neutral", "中性"),
        OcctLightingPreset.Sunlight => Local("Sunlight", "日光"),
        OcctLightingPreset.Flat => Local("Flat", "平光"),
        _ => Local("Studio", "摄影棚")
    };

    private static string Local(string english, string chinese) =>
        DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? chinese : english;
}

using System.Globalization;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using Input = System.Windows.Input;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private void SetLanguage(DemoLanguage language)
    {
        DemoLocalization.CurrentLanguage = language;
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        FontFamily = new System.Windows.Media.FontFamily(
            DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "Microsoft YaHei UI" : "Segoe UI");
        if (_session is not null) ExecuteSafe(ApplyViewCubeLanguage);
        Title = DemoLocalization.Text("AppTitle.Wpf");
        ModelExplorerGroup.Header = DemoLocalization.Text("Panel.ModelExplorer");
        PropertiesGroup.Header = DemoLocalization.Text("Panel.Properties");
        CommandLineGroup.Header = DemoLocalization.Text("Panel.CommandLine");
        PropertyNameColumn.Header = DemoLocalization.Text("Property.Name");
        PropertyValueColumn.Header = DemoLocalization.Text("Property.Value");
        if (_session is null)
        {
            CommandStatus.Text = DemoLocalization.Text("Status.Initializing");
            SelectionStatus.Text = DemoLocalization.Text("Status.NoneSelected");
        }
        else
        {
            CommandStatus.Text = DemoLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        }
        BuildMenus();
        BuildToolbar();
        RefreshObjectTree();
        ShowObjectProperties(_session?.ActiveObject);
    }

    private void ApplyViewCubeLanguage()
    {
        if (_session is null) return;
        _session.Engine.SetViewCubeLanguage(
            DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
                ? OcctViewCubeLanguage.ChineseSimplified
                : OcctViewCubeLanguage.English);
    }

    private void ShowMouseHelp()
    {
        System.Windows.MessageBox.Show(this, DemoLocalization.Text("Dialog.MouseText"),
            DemoLocalization.Text("Menu.MouseHelp"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    private void ShowAbout()
    {
        System.Windows.MessageBox.Show(this, DemoLocalization.Text("Dialog.AboutText"),
            DemoLocalization.Text("Menu.About"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    private static string MaterialDisplayName(OcctMaterial material)
    {
        if (DemoLocalization.CurrentLanguage == DemoLanguage.English) return material.ToString();
        return material switch
        {
            OcctMaterial.Brass => "黄铜", OcctMaterial.Bronze => "青铜", OcctMaterial.Copper => "铜", OcctMaterial.Gold => "金",
            OcctMaterial.Pewter => "锡合金", OcctMaterial.Plastered => "石膏", OcctMaterial.Plastified => "塑料", OcctMaterial.Silver => "银",
            OcctMaterial.Steel => "钢", OcctMaterial.Stone => "石材", OcctMaterial.ShinyPlastified => "高光塑料", OcctMaterial.Satin => "缎面",
            OcctMaterial.Metalized => "金属化", OcctMaterial.Ionized => "离子化", OcctMaterial.Chrome => "铬", OcctMaterial.Aluminum => "铝",
            OcctMaterial.Obsidian => "黑曜石", OcctMaterial.Neon => "霓虹", OcctMaterial.Jade => "玉石", OcctMaterial.Charcoal => "木炭",
            OcctMaterial.Water => "水", OcctMaterial.Glass => "玻璃", OcctMaterial.Diamond => "钻石", OcctMaterial.Transparent => "透明",
            OcctMaterial.Default => "OCCT 默认", _ => material.ToString()
        };
    }
}

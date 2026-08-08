using System.Globalization;
using CadCommon;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using Input = System.Windows.Input;

namespace CadWpf;

public partial class MainWindow
{
    private void SetLanguage(CadLanguage language)
    {
        CadLocalization.CurrentLanguage = language;
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        FontFamily = new System.Windows.Media.FontFamily(
            CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "Microsoft YaHei UI" : "Segoe UI");
        if (_session is not null) ExecuteSafe(ApplyViewCubeLanguage);
        Title = CadLocalization.Text("AppTitle.Wpf");
        ModelExplorerGroup.Header = CadLocalization.Text("Panel.ModelExplorer");
        PropertiesGroup.Header = CadLocalization.Text("Panel.Properties");
        CommandLineGroup.Header = CadLocalization.Text("Panel.CommandLine");
        PropertyNameColumn.Header = CadLocalization.Text("Property.Name");
        PropertyValueColumn.Header = CadLocalization.Text("Property.Value");
        if (_session is null)
        {
            CommandStatus.Text = CadLocalization.Text("Status.Initializing");
            SelectionStatus.Text = CadLocalization.Text("Status.NoneSelected");
        }
        else
        {
            CommandStatus.Text = CadLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
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
            CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
                ? OcctViewCubeLanguage.ChineseSimplified
                : OcctViewCubeLanguage.English);
    }

    private void ShowMouseHelp()
    {
        System.Windows.MessageBox.Show(this, CadLocalization.Text("Dialog.MouseText"),
            CadLocalization.Text("Menu.MouseHelp"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    private void ShowAbout()
    {
        System.Windows.MessageBox.Show(this, CadLocalization.Text("Dialog.AboutText"),
            CadLocalization.Text("Menu.About"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    private static string MaterialDisplayName(OcctMaterial material)
    {
        if (CadLocalization.CurrentLanguage == CadLanguage.English) return material.ToString();
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

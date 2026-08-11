using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using AvaloniaBrushes = Avalonia.Media.Brushes;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaFontFamily = Avalonia.Media.FontFamily;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaloniaOrientation = Avalonia.Layout.Orientation;
using AvaloniaToolTip = Avalonia.Controls.ToolTip;
using Forms = System.Windows.Forms;
using Button = Avalonia.Controls.Button;
using CheckBox = Avalonia.Controls.CheckBox;
using ContextMenu = Avalonia.Controls.ContextMenu;
using MenuItem = Avalonia.Controls.MenuItem;
using ComboBox = Avalonia.Controls.ComboBox;
using Control = Avalonia.Controls.Control;
using GroupBox = Avalonia.Controls.GroupBox;
using TextBox = Avalonia.Controls.TextBox;
using TreeView = Avalonia.Controls.TreeView;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private void SetLanguage(DemoLanguage language)
    {
        DemoLocalization.CurrentLanguage = language;
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        FontFamily = UiFontFamily;
        Title = "OCCT CAD - Avalonia";
        _modelExplorerGroup.Header = DemoLocalization.Text("Panel.ModelExplorer");
        _propertiesGroup.Header = DemoLocalization.Text("Panel.Properties");
        _commandLineGroup.Header = DemoLocalization.Text("Panel.CommandLine");
        if (_session is null)
        {
            _commandStatus.Text = DemoLocalization.Text("Status.Initializing");
            _selectionStatus.Text = DemoLocalization.Text("Status.NoneSelected");
        }
        else
        {
            _commandStatus.Text = DemoLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
            ExecuteSafe(ApplyViewCubeLanguage);
        }
        BuildMenus();
        BuildToolbar();
        RefreshObjectTree();
        ShowSelectionProperties(_session?.Engine.SelectedObjects ?? Array.Empty<IOcctObject>());
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
        Forms.MessageBox.Show(DemoLocalization.Text("Dialog.MouseText"), DemoLocalization.Text("Menu.MouseHelp"),
            Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
    }

    private void ShowAbout()
    {
        Forms.MessageBox.Show(DemoProductInfo.AboutText(DemoLocalization.CurrentLanguage), DemoLocalization.Text("Menu.About"),
            Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
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

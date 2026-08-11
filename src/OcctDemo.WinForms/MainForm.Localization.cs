using System.Globalization;
using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private void SetLanguage(DemoLanguage language)
    {
        DemoLocalization.CurrentLanguage = language;
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        Font = new Font(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "Microsoft YaHei UI" : "Segoe UI", 9F);
        if (_session is not null) ExecuteSafe(ApplyViewCubeLanguage);
        Text = DemoLocalization.Text("AppTitle.WinForms");
        _objectGroup.Text = DemoLocalization.Text("Panel.ModelExplorer");
        _propertyGroup.Text = DemoLocalization.Text("Panel.Properties");
        _logGroup.Text = DemoLocalization.Text("Panel.CommandLine");
        _propertyNameColumn.HeaderText = DemoLocalization.Text("Property.Name");
        _propertyValueColumn.HeaderText = DemoLocalization.Text("Property.Value");
        if (_session is null)
        {
            _commandStatus.Text = DemoLocalization.Text("Status.Initializing");
            _selectionStatus.Text = DemoLocalization.Text("Status.NoneSelected");
        }
        else
        {
            _commandStatus.Text = DemoLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        }
        BuildMenus();
        BuildToolBar();
        ApplyDemoEnhancements();
        _objectTree.ContextMenuStrip?.Dispose();
        _objectTree.ContextMenuStrip = BuildTreeContextMenu();
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
        MessageBox.Show(this, DemoLocalization.Text("Dialog.MouseText"), DemoLocalization.Text("Menu.MouseHelp"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowAbout()
    {
        MessageBox.Show(this, DemoProductInfo.AboutText(DemoLocalization.CurrentLanguage), DemoLocalization.Text("Menu.About"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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

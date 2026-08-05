using CadCommon;

namespace CadWinForms;

public sealed partial class MainForm
{
    public void AttachApiCenter()
    {
        if (_menu.Items.OfType<ToolStripItem>().Any(item => string.Equals(item.Name, "ApiCenterMenu", StringComparison.Ordinal)))
            return;

        var menu = new ToolStripMenuItem
        {
            Name = "ApiCenterMenu",
            Text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "API 中心" : "API Center"
        };
        var open = new ToolStripMenuItem
        {
            Text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "接口目录与综合场景..." : "API Catalog and Scenarios..."
        };
        open.Click += (_, _) => ShowApiCenter();
        menu.DropDownItems.Add(open);
        menu.DropDownItems.Add(new ToolStripSeparator());
        var summary = new ToolStripMenuItem(ApiDemoCatalog.CoverageSummary) { Enabled = false };
        menu.DropDownItems.Add(summary);

        var helpIndex = Math.Max(0, _menu.Items.Count - 1);
        _menu.Items.Insert(helpIndex, menu);
    }

    private void ShowApiCenter()
    {
        if (_session is null)
        {
            MessageBox.Show(
                this,
                CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
                    ? "OCCT 视口尚未初始化，请稍后再试。"
                    : "The OCCT viewport is not initialized yet.",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var dialog = new ApiCenterForm(Session);
        dialog.ShowDialog(this);
        RefreshObjectTree();
    }
}

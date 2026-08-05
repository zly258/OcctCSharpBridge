using CadCommon;
using Controls = System.Windows.Controls;

namespace CadWpf;

public partial class MainWindow
{
    public void AttachApiCenter()
    {
        if (MainMenu.Items.OfType<Controls.MenuItem>().Any(item => string.Equals(item.Name, "ApiCenterMenu", StringComparison.Ordinal)))
            return;

        var menu = new Controls.MenuItem
        {
            Name = "ApiCenterMenu",
            Header = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "API 中心" : "API Center"
        };
        var open = new Controls.MenuItem
        {
            Header = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "接口目录与综合场景..." : "API Catalog and Scenarios..."
        };
        open.Click += (_, _) => ShowApiCenter();
        menu.Items.Add(open);
        menu.Items.Add(new Controls.Separator());
        menu.Items.Add(new Controls.MenuItem { Header = ApiDemoCatalog.CoverageSummary, IsEnabled = false });

        var helpIndex = Math.Max(0, MainMenu.Items.Count - 1);
        MainMenu.Items.Insert(helpIndex, menu);
    }

    private void ShowApiCenter()
    {
        if (_session is null)
        {
            System.Windows.MessageBox.Show(
                this,
                CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
                    ? "OCCT 视口尚未初始化，请稍后再试。"
                    : "The OCCT viewport is not initialized yet.",
                Title,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            return;
        }

        var dialog = new ApiCenterWindow(Session) { Owner = this };
        dialog.ShowDialog();
        RefreshObjectTree();
    }
}

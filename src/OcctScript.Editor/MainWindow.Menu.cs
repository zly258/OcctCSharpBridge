using System.Windows.Controls;

namespace OcctScript.Editor;

public partial class MainWindow
{
    private void InstallDynamicMenus()
    {
        if (Content is not DockPanel dock) return;
        var menu = dock.Children.OfType<Menu>().FirstOrDefault();
        if (menu is null) return;

        var createMenu = menu.Items.OfType<MenuItem>().FirstOrDefault(item => string.Equals(item.Header?.ToString(), ResourceText("Ui.Create"), StringComparison.Ordinal));
        if (createMenu is not null)
        {
            createMenu.Items.Clear();
            foreach (var group in commandRegistry.GetAll().GroupBy(definition => definition.CategoryKey).OrderBy(group => group.Min(definition => definition.Order)))
            {
                var category = new MenuItem();
                category.SetResourceReference(HeaderedItemsControl.HeaderProperty, group.Key);
                foreach (var definition in group.OrderBy(item => item.Order))
                {
                    var item = new MenuItem { Tag = definition.Type };
                    item.SetResourceReference(HeaderedItemsControl.HeaderProperty, definition.DisplayNameKey);
                    item.Click += AddCommandMenu_Click;
                    category.Items.Add(item);
                }
                createMenu.Items.Add(category);
            }
            createMenu.Items.Add(new Separator());
            var addParameter = new MenuItem();
            addParameter.SetResourceReference(HeaderedItemsControl.HeaderProperty, "Ui.AddParameter");
            addParameter.Click += AddParameter_Click;
            createMenu.Items.Add(addParameter);
        }

        var existingHelp = menu.Items.OfType<MenuItem>().FirstOrDefault(item => Equals(item.Tag, "DynamicHelpMenu"));
        if (existingHelp is not null) menu.Items.Remove(existingHelp);
        var help = new MenuItem { Tag = "DynamicHelpMenu" };
        help.SetResourceReference(HeaderedItemsControl.HeaderProperty, "Ui.Help");
        var about = new MenuItem();
        about.SetResourceReference(HeaderedItemsControl.HeaderProperty, "Ui.About");
        about.Click += (_, _) => new AboutWindow { Owner = this }.ShowDialog();
        help.Items.Add(about);
        menu.Items.Add(help);
    }
}

using OcctDemo.Common;

namespace OcctDemo.Wpf;

internal sealed class ParameterDialog : System.Windows.Window
{
    private readonly Dictionary<string, System.Windows.Controls.Control> _editors = new(StringComparer.OrdinalIgnoreCase);

    private ParameterDialog(string title, IReadOnlyList<DemoParameterDefinition> parameters)
    {
        Title = title;
        Width = 480;
        Height = Math.Min(760, 130 + parameters.Count * 45);
        MinHeight = 180;
        ResizeMode = System.Windows.ResizeMode.NoResize;
        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        FontFamily = new System.Windows.Media.FontFamily(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "Microsoft YaHei UI" : "Segoe UI");
        FontSize = 13;

        var root = new System.Windows.Controls.DockPanel { Margin = new System.Windows.Thickness(14) };
        var buttons = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new System.Windows.Thickness(0, 12, 0, 0)
        };
        var ok = new System.Windows.Controls.Button { Content = DemoLocalization.Text("Dialog.Ok"), Width = 86, IsDefault = true, Margin = new System.Windows.Thickness(4) };
        var cancel = new System.Windows.Controls.Button { Content = DemoLocalization.Text("Dialog.Cancel"), Width = 86, IsCancel = true, Margin = new System.Windows.Thickness(4) };
        ok.Click += (_, _) => { DialogResult = true; Close(); };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        System.Windows.Controls.DockPanel.SetDock(buttons, System.Windows.Controls.Dock.Bottom);
        root.Children.Add(buttons);

        var grid = new System.Windows.Controls.Grid();
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(155) });
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(48) });

        for (var row = 0; row < parameters.Count; row++)
        {
            var parameter = parameters[row];
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(42) });
            var label = new System.Windows.Controls.TextBlock { Text = parameter.Label, VerticalAlignment = System.Windows.VerticalAlignment.Center, Margin = new System.Windows.Thickness(3) };
            var editor = CreateEditor(parameter);
            editor.Margin = new System.Windows.Thickness(3, 5, 3, 5);
            editor.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            var unit = new System.Windows.Controls.TextBlock { Text = parameter.Unit ?? string.Empty, VerticalAlignment = System.Windows.VerticalAlignment.Center, Margin = new System.Windows.Thickness(3) };
            System.Windows.Controls.Grid.SetRow(label, row);
            System.Windows.Controls.Grid.SetRow(editor, row);
            System.Windows.Controls.Grid.SetRow(unit, row);
            System.Windows.Controls.Grid.SetColumn(editor, 1);
            System.Windows.Controls.Grid.SetColumn(unit, 2);
            grid.Children.Add(label);
            grid.Children.Add(editor);
            grid.Children.Add(unit);
            _editors[parameter.Key] = editor;
        }

        root.Children.Add(new System.Windows.Controls.ScrollViewer { Content = grid, VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto });
        Content = root;
    }

    public static bool TryGetValues(System.Windows.Window owner, string title, IReadOnlyList<DemoParameterDefinition> parameters, out IReadOnlyDictionary<string, string> values)
    {
        if (parameters.Count == 0)
        {
            values = new Dictionary<string, string>();
            return true;
        }
        var dialog = new ParameterDialog(title, parameters) { Owner = owner };
        if (dialog.ShowDialog() != true)
        {
            values = new Dictionary<string, string>();
            return false;
        }
        values = dialog.ReadValues();
        return true;
    }

    private static System.Windows.Controls.Control CreateEditor(DemoParameterDefinition parameter)
    {
        return parameter.Kind switch
        {
            DemoParameterKind.Boolean => new System.Windows.Controls.CheckBox { IsChecked = string.Equals(parameter.DefaultValue, "true", StringComparison.OrdinalIgnoreCase), VerticalContentAlignment = System.Windows.VerticalAlignment.Center },
            DemoParameterKind.Choice => new System.Windows.Controls.ComboBox { ItemsSource = parameter.Options ?? Array.Empty<string>(), SelectedItem = parameter.DefaultValue },
            _ => new System.Windows.Controls.TextBox { Text = parameter.DefaultValue, VerticalContentAlignment = System.Windows.VerticalAlignment.Center }
        };
    }

    private IReadOnlyDictionary<string, string> ReadValues()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _editors)
        {
            values[pair.Key] = pair.Value switch
            {
                System.Windows.Controls.CheckBox checkBox => checkBox.IsChecked == true ? "true" : "false",
                System.Windows.Controls.ComboBox comboBox => comboBox.SelectedItem?.ToString() ?? string.Empty,
                System.Windows.Controls.TextBox textBox => textBox.Text.Trim(),
                _ => string.Empty
            };
        }
        return values;
    }
}

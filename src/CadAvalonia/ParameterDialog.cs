using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using CadCommon;

namespace CadAvalonia;

internal sealed class ParameterDialog : Window
{
    private readonly Dictionary<string, Control> _editors = new(StringComparer.OrdinalIgnoreCase);

    private ParameterDialog(string title, IReadOnlyList<CadParameterDefinition> parameters)
    {
        Title = title;
        Width = 500;
        Height = Math.Min(760, 150 + parameters.Count * 46);
        MinHeight = 190;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;

        var root = new DockPanel { Margin = new Thickness(16) };
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Margin = new Thickness(0, 14, 0, 0)
        };
        var ok = new Button
        {
            Content = CadLocalization.Text("Dialog.Ok"),
            Width = 90,
            IsDefault = true
        };
        var cancel = new Button
        {
            Content = CadLocalization.Text("Dialog.Cancel"),
            Width = 90,
            IsCancel = true
        };
        ok.Click += (_, _) => Close(true);
        cancel.Click += (_, _) => Close(false);
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        DockPanel.SetDock(buttons, Dock.Bottom);
        root.Children.Add(buttons);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("165,*,55")
        };

        for (var row = 0; row < parameters.Count; row++)
        {
            var parameter = parameters[row];
            grid.RowDefinitions.Add(new RowDefinition(44, GridUnitType.Pixel));

            var label = new TextBlock
            {
                Text = parameter.Label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4)
            };
            var editor = CreateEditor(parameter);
            editor.Margin = new Thickness(4, 6);
            editor.VerticalAlignment = VerticalAlignment.Center;
            var unit = new TextBlock
            {
                Text = parameter.Unit ?? string.Empty,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4)
            };

            Grid.SetRow(label, row);
            Grid.SetRow(editor, row);
            Grid.SetRow(unit, row);
            Grid.SetColumn(editor, 1);
            Grid.SetColumn(unit, 2);
            grid.Children.Add(label);
            grid.Children.Add(editor);
            grid.Children.Add(unit);
            _editors[parameter.Key] = editor;
        }

        root.Children.Add(new ScrollViewer
        {
            Content = grid,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });
        Content = root;
    }

    public static async Task<(bool Accepted, IReadOnlyDictionary<string, string> Values)> GetValuesAsync(
        Window owner,
        string title,
        IReadOnlyList<CadParameterDefinition> parameters)
    {
        if (parameters.Count == 0)
            return (true, new Dictionary<string, string>());

        var dialog = new ParameterDialog(title, parameters);
        var accepted = await dialog.ShowDialog<bool>(owner);
        return accepted
            ? (true, dialog.ReadValues())
            : (false, new Dictionary<string, string>());
    }

    private static Control CreateEditor(CadParameterDefinition parameter)
    {
        return parameter.Kind switch
        {
            CadParameterKind.Boolean => new CheckBox
            {
                IsChecked = string.Equals(parameter.DefaultValue, "true", StringComparison.OrdinalIgnoreCase),
                VerticalContentAlignment = VerticalAlignment.Center
            },
            CadParameterKind.Choice => new ComboBox
            {
                ItemsSource = parameter.Options ?? Array.Empty<string>(),
                SelectedItem = parameter.DefaultValue
            },
            _ => new TextBox
            {
                Text = parameter.DefaultValue,
                VerticalContentAlignment = VerticalAlignment.Center
            }
        };
    }

    private IReadOnlyDictionary<string, string> ReadValues()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _editors)
        {
            values[pair.Key] = pair.Value switch
            {
                CheckBox checkBox => checkBox.IsChecked == true ? "true" : "false",
                ComboBox comboBox => comboBox.SelectedItem?.ToString() ?? string.Empty,
                TextBox textBox => (textBox.Text ?? string.Empty).Trim(),
                _ => string.Empty
            };
        }
        return values;
    }
}

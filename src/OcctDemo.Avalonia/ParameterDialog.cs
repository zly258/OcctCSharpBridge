using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using OcctDemo.Common;
using AvaloniaFontFamily = Avalonia.Media.FontFamily;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaloniaOrientation = Avalonia.Layout.Orientation;
using Button = Avalonia.Controls.Button;
using CheckBox = Avalonia.Controls.CheckBox;
using ComboBox = Avalonia.Controls.ComboBox;
using Control = Avalonia.Controls.Control;
using TextBox = Avalonia.Controls.TextBox;

namespace OcctDemo.Avalonia;

internal sealed class ParameterDialog : Window
{
    private readonly Dictionary<string, Control> _editors = new(StringComparer.OrdinalIgnoreCase);

    private ParameterDialog(string title, IReadOnlyList<DemoParameterDefinition> parameters)
    {
        Title = title;
        Width = 520;
        Height = Math.Min(760, 150 + parameters.Count * 46);
        MinHeight = 190;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        FontFamily = OperatingSystem.IsWindows()
            ? new AvaloniaFontFamily("Microsoft YaHei UI")
            : new AvaloniaFontFamily("Inter");

        var root = new DockPanel { Margin = new Thickness(16) };
        var buttons = new StackPanel
        {
            Orientation = AvaloniaOrientation.Horizontal,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Right,
            Spacing = 8,
            Margin = new Thickness(0, 14, 0, 0)
        };
        var ok = new Button
        {
            Content = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "确定" : "OK",
            Width = 96,
            HorizontalContentAlignment = AvaloniaHorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            IsDefault = true
        };
        var cancel = new Button
        {
            Content = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "取消" : "Cancel",
            Width = 96,
            HorizontalContentAlignment = AvaloniaHorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            IsCancel = true
        };
        buttons.Children.Add(cancel);
        buttons.Children.Add(ok);
        DockPanel.SetDock(buttons, Dock.Bottom);
        root.Children.Add(buttons);

        var form = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("190,*"),
            RowDefinitions = new RowDefinitions(string.Join(',', Enumerable.Repeat("Auto", Math.Max(1, parameters.Count))))
        };

        for (var index = 0; index < parameters.Count; index++)
        {
            var parameter = parameters[index];
            var label = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(parameter.Unit) ? parameter.Label : $"{parameter.Label} ({parameter.Unit})",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 4, 12, 4)
            };
            var editor = CreateEditor(parameter);
            editor.Margin = new Thickness(0, 3);
            Grid.SetRow(label, index);
            Grid.SetRow(editor, index);
            Grid.SetColumn(editor, 1);
            form.Children.Add(label);
            form.Children.Add(editor);
            _editors[parameter.Key] = editor;
        }

        root.Children.Add(new ScrollViewer
        {
            Content = form,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });
        Content = root;

        ok.Click += (_, _) => Close(new ParameterDialogResult(true, ReadValues()));
        cancel.Click += (_, _) => Close(ParameterDialogResult.Cancelled);
    }

    public static async Task<ParameterDialogResult> GetValuesAsync(
        Window owner,
        string title,
        IReadOnlyList<DemoParameterDefinition> parameters)
    {
        if (parameters.Count == 0)
            return new ParameterDialogResult(true, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        var dialog = new ParameterDialog(title, parameters);
        return await dialog.ShowDialog<ParameterDialogResult>(owner);
    }

    private static Control CreateEditor(DemoParameterDefinition parameter)
    {
        if (parameter.Kind == DemoParameterKind.Boolean)
        {
            return new CheckBox
            {
                IsChecked = bool.TryParse(parameter.DefaultValue, out var value) && value
            };
        }

        if (parameter.Kind == DemoParameterKind.Choice)
        {
            var options = parameter.Options ?? Array.Empty<string>();
            var combo = new ComboBox
            {
                ItemsSource = options,
                MinWidth = 240,
                MinHeight = 30,
                HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch
            };
            var selected = options
                .Select((value, index) => new { value, index })
                .FirstOrDefault(item => string.Equals(item.value, parameter.DefaultValue, StringComparison.OrdinalIgnoreCase));
            combo.SelectedIndex = selected?.index ?? (options.Count > 0 ? 0 : -1);
            return combo;
        }

        return new TextBox { Text = parameter.DefaultValue };
    }

    private Dictionary<string, string> ReadValues()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _editors)
        {
            values[pair.Key] = pair.Value switch
            {
                CheckBox checkBox => checkBox.IsChecked == true ? "true" : "false",
                ComboBox comboBox => comboBox.SelectedItem?.ToString() ?? string.Empty,
                TextBox textBox => textBox.Text ?? string.Empty,
                _ => string.Empty
            };
        }
        return values;
    }
}

internal sealed record ParameterDialogResult(bool Accepted, IReadOnlyDictionary<string, string> Values)
{
    public static ParameterDialogResult Cancelled { get; } = new(false, new Dictionary<string, string>());
}

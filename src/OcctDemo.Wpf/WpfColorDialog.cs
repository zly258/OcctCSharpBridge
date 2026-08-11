using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace OcctDemo.Wpf;

internal sealed class WpfColorDialog : Window
{
    private readonly Slider _red;
    private readonly Slider _green;
    private readonly Slider _blue;
    private readonly Border _preview;
    private readonly TextBlock _hexValue;

    private WpfColorDialog(string title, DrawingColor initial)
    {
        Title = title;
        Width = 420;
        Height = 310;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        FontFamily = new FontFamily("Microsoft YaHei UI");
        Background = Brushes.White;

        _red = CreateSlider(initial.R);
        _green = CreateSlider(initial.G);
        _blue = CreateSlider(initial.B);
        _preview = new Border
        {
            Width = 92,
            Height = 58,
            BorderBrush = new SolidColorBrush(MediaColor.FromRgb(185, 190, 196)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3)
        };
        _hexValue = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 13
        };

        var root = new Grid { Margin = new Thickness(18) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        root.Children.Add(CreateChannelRow("R", _red, 0));
        root.Children.Add(CreateChannelRow("G", _green, 1));
        root.Children.Add(CreateChannelRow("B", _blue, 2));

        var previewRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 12, 0, 0)
        };
        previewRow.Children.Add(_preview);
        previewRow.Children.Add(new Border { Width = 14 });
        previewRow.Children.Add(_hexValue);
        Grid.SetRow(previewRow, 3);
        root.Children.Add(previewRow);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 18, 0, 0)
        };
        var ok = new Button
        {
            Content = "OK",
            IsDefault = true,
            MinWidth = 88,
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 0, 8, 0)
        };
        ok.Click += (_, _) => DialogResult = true;
        var cancel = new Button
        {
            Content = "Cancel",
            IsCancel = true,
            MinWidth = 88,
            Padding = new Thickness(12, 5, 12, 5)
        };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        Grid.SetRow(buttons, 5);
        root.Children.Add(buttons);

        _red.ValueChanged += (_, _) => UpdatePreview();
        _green.ValueChanged += (_, _) => UpdatePreview();
        _blue.ValueChanged += (_, _) => UpdatePreview();
        Content = root;
        UpdatePreview();
    }

    private DrawingColor SelectedColor => DrawingColor.FromArgb(
        255,
        (int)Math.Round(_red.Value),
        (int)Math.Round(_green.Value),
        (int)Math.Round(_blue.Value));

    public static bool TryPick(Window owner, string title, DrawingColor initial, out DrawingColor color)
    {
        var dialog = new WpfColorDialog(title, initial) { Owner = owner };
        if (dialog.ShowDialog() == true)
        {
            color = dialog.SelectedColor;
            return true;
        }

        color = initial;
        return false;
    }

    private static Slider CreateSlider(byte value) => new()
    {
        Minimum = 0,
        Maximum = 255,
        Value = value,
        TickFrequency = 1,
        IsSnapToTickEnabled = true,
        VerticalAlignment = VerticalAlignment.Center
    };

    private static Grid CreateChannelRow(string name, Slider slider, int row)
    {
        var grid = new Grid { Margin = new Thickness(0, 5, 0, 5) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });

        var label = new TextBlock
        {
            Text = name,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(label, 0);
        grid.Children.Add(label);

        Grid.SetColumn(slider, 1);
        grid.Children.Add(slider);

        var value = new TextBlock
        {
            Text = ((int)slider.Value).ToString(),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        slider.ValueChanged += (_, _) => value.Text = ((int)Math.Round(slider.Value)).ToString();
        Grid.SetColumn(value, 2);
        grid.Children.Add(value);

        Grid.SetRow(grid, row);
        return grid;
    }

    private void UpdatePreview()
    {
        var color = SelectedColor;
        _preview.Background = new SolidColorBrush(MediaColor.FromRgb(color.R, color.G, color.B));
        _hexValue.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}

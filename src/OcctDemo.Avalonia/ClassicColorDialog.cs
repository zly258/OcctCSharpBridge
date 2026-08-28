using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using OcctDemo.Common;
using DrawingColor = System.Drawing.Color;

namespace OcctDemo.Avalonia;

internal sealed class ClassicColorDialog : Window
{
    private static readonly DrawingColor[] BasicColors =
    {
        Hex("000000"), Hex("404040"), Hex("808080"), Hex("C0C0C0"), Hex("FFFFFF"), Hex("800000"), Hex("FF0000"), Hex("FF8080"),
        Hex("804000"), Hex("FF8000"), Hex("FFC080"), Hex("808000"), Hex("FFFF00"), Hex("FFFF80"), Hex("008000"), Hex("00FF00"),
        Hex("80FF80"), Hex("008080"), Hex("00FFFF"), Hex("80FFFF"), Hex("000080"), Hex("0000FF"), Hex("8080FF"), Hex("400080"),
        Hex("8000FF"), Hex("C080FF"), Hex("800080"), Hex("FF00FF"), Hex("FF80FF"), Hex("804040"), Hex("C06060"), Hex("FFB0B0"),
        Hex("806000"), Hex("C09000"), Hex("FFD060"), Hex("408000"), Hex("80C000"), Hex("C0FF80"), Hex("006060"), Hex("00A0A0"),
        Hex("80D0D0"), Hex("004080"), Hex("0080C0"), Hex("80C0FF"), Hex("402060"), Hex("8040A0"), Hex("C080D0"), Hex("604060")
    };

    private readonly DrawingColor _initialColor;
    private DrawingColor _selectedColor;
    private readonly Border _currentPreview;
    private readonly Border _newPreview;
    private readonly TextBox _redBox;
    private readonly TextBox _greenBox;
    private readonly TextBox _blueBox;
    private readonly TextBox _hexBox;
    private readonly List<SwatchEntry> _allSwatches = new();
    private readonly List<SwatchEntry> _customSwatches = new();
    private bool _syncingEditors;
    private int _nextCustomSlot = 1;

    private ClassicColorDialog(string title, DrawingColor initialColor)
    {
        _initialColor = Opaque(initialColor);
        _selectedColor = _initialColor;
        _currentPreview = CreatePreview(_initialColor);
        _newPreview = CreatePreview(_initialColor);
        _redBox = CreateValueBox(3);
        _greenBox = CreateValueBox(3);
        _blueBox = CreateValueBox(3);
        _hexBox = CreateValueBox(7, 104);

        Title = title;
        Width = 650;
        Height = 500;
        MinWidth = 650;
        MaxWidth = 650;
        MinHeight = 500;
        MaxHeight = 500;
        CanResize = false;
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Content = BuildContent();

        _redBox.TextChanged += (_, _) => TryApplyRgbEditors();
        _greenBox.TextChanged += (_, _) => TryApplyRgbEditors();
        _blueBox.TextChanged += (_, _) => TryApplyRgbEditors();
        _hexBox.TextChanged += (_, _) => TryApplyHexEditor();
        SyncEditors();
    }

    public static async Task<DrawingColor?> ShowAsync(Window owner, string title, DrawingColor initialColor)
    {
        var dialog = new ClassicColorDialog(title, initialColor);
        var accepted = await dialog.ShowDialog<bool>(owner);
        return accepted ? dialog._selectedColor : null;
    }

    private Control BuildContent()
    {
        var body = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 24,
            Children =
            {
                BuildPalettePanel(),
                BuildDetailPanel()
            }
        };

        var ok = CreateCommandButton(Local("OK", "确定"));
        ok.IsDefault = true;
        ok.Click += (_, _) => Close(true);

        var cancel = CreateCommandButton(Local("Cancel", "取消"));
        cancel.IsCancel = true;
        cancel.Click += (_, _) => Close(false);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { cancel, ok }
        };

        return new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 18,
            Children = { body, buttons }
        };
    }

    private Control BuildPalettePanel()
    {
        var customColors = new DrawingColor[16];
        customColors[0] = _initialColor;
        for (var i = 1; i < customColors.Length; i++)
        {
            customColors[i] = DrawingColor.White;
        }

        var addCustom = new Button
        {
            Content = Local("Add to Custom Colors", "添加到自定义颜色"),
            HorizontalAlignment = HorizontalAlignment.Left,
            MinWidth = 176,
        };
        addCustom.Click += (_, _) => AddCurrentToCustomColors();

        return new StackPanel
        {
            Width = 340,
            Spacing = 8,
            Children =
            {
                CreateSectionLabel(Local("Basic colors", "基本颜色")),
                BuildSwatchMatrix(BasicColors, isCustom: false),
                CreateSectionLabel(Local("Custom colors", "自定义颜色"), new Thickness(0, 10, 0, 0)),
                BuildSwatchMatrix(customColors, isCustom: true),
                addCustom
            }
        };
    }

    private Control BuildDetailPanel()
    {
        var previewRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                CreatePreviewColumn(Local("Current", "当前"), _currentPreview),
                CreatePreviewColumn(Local("New", "新颜色"), _newPreview)
            }
        };

        var rgbRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                CreateEditorColumn("R", _redBox),
                CreateEditorColumn("G", _greenBox),
                CreateEditorColumn("B", _blueBox)
            }
        };

        var hexRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Children =
            {
                new TextBlock
                {
                    Text = "Hex",
                    Width = 34,
                    VerticalAlignment = VerticalAlignment.Center
                },
                _hexBox
            }
        };

        return new StackPanel
        {
            Width = 226,
            Spacing = 12,
            Children =
            {
                CreateSectionLabel(Local("Color preview", "颜色预览")),
                previewRow,
                CreateSeparator(),
                CreateSectionLabel(Local("Color values", "颜色数值")),
                rgbRow,
                hexRow,
                new TextBlock
                {
                    Text = Local("Choose a swatch or enter exact RGB / Hex values.", "可直接选择色块，也可输入精确的 RGB / Hex 数值。"),
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.68,
                    Margin = new Thickness(0, 4, 0, 0)
                }
            }
        };
    }

    private Control BuildSwatchMatrix(IReadOnlyList<DrawingColor> colors, bool isCustom)
    {
        var panel = new StackPanel { Spacing = 4 };
        const int columns = 8;

        for (var rowIndex = 0; rowIndex < (colors.Count + columns - 1) / columns; rowIndex++)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4
            };

            for (var columnIndex = 0; columnIndex < columns; columnIndex++)
            {
                var index = rowIndex * columns + columnIndex;
                if (index >= colors.Count)
                {
                    break;
                }

                row.Children.Add(CreateSwatch(colors[index], isCustom));
            }

            panel.Children.Add(row);
        }

        return panel;
    }

    private Button CreateSwatch(DrawingColor color, bool isCustom)
    {
        var button = new Button
        {
            Width = 36,
            Height = 30,
            Padding = new Thickness(0),
            Background = new SolidColorBrush(ToAvaloniaColor(color)),
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };

        var entry = new SwatchEntry(button, Opaque(color));
        _allSwatches.Add(entry);
        if (isCustom)
        {
            _customSwatches.Add(entry);
        }

        button.Click += (_, _) => SetSelectedColor(entry.Color);
        return button;
    }

    private static Border CreatePreview(DrawingColor color) => new()
    {
        Width = 94,
        Height = 64,
        Background = new SolidColorBrush(ToAvaloniaColor(color)),
        BorderBrush = Brushes.Gray,
        BorderThickness = new Thickness(1)
    };

    private static Control CreatePreviewColumn(string label, Border preview) => new StackPanel
    {
        Spacing = 5,
        Children =
        {
            new TextBlock { Text = label, Opacity = 0.78 },
            preview
        }
    };

    private static Control CreateEditorColumn(string label, TextBox editor) => new StackPanel
    {
        Spacing = 4,
        Children =
        {
            new TextBlock { Text = label },
            editor
        }
    };

    private static TextBox CreateValueBox(int maxLength, double width = 58) => new()
    {
        Width = width,
        MaxLength = maxLength,
        HorizontalContentAlignment = HorizontalAlignment.Center
    };

    private static TextBlock CreateSectionLabel(string text, Thickness? margin = null) => new()
    {
        Text = text,
        FontWeight = FontWeight.SemiBold,
        Margin = margin ?? new Thickness(0)
    };

    private static Border CreateSeparator() => new()
    {
        Height = 1,
        Background = Brushes.Gray,
        Opacity = 0.25,
        Margin = new Thickness(0, 4)
    };

    private static Button CreateCommandButton(string text) => new()
    {
        Content = text,
        MinWidth = 96,
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalContentAlignment = VerticalAlignment.Center
    };

    private void AddCurrentToCustomColors()
    {
        if (_customSwatches.Count == 0)
        {
            return;
        }

        if (_nextCustomSlot >= _customSwatches.Count)
        {
            _nextCustomSlot = 0;
        }

        var entry = _customSwatches[_nextCustomSlot];
        entry.Color = _selectedColor;
        entry.Button.Background = new SolidColorBrush(ToAvaloniaColor(_selectedColor));
        _nextCustomSlot++;
        UpdateSwatchSelection();
    }

    private void TryApplyRgbEditors()
    {
        if (_syncingEditors)
        {
            return;
        }

        if (!TryByte(_redBox.Text, out var red) ||
            !TryByte(_greenBox.Text, out var green) ||
            !TryByte(_blueBox.Text, out var blue))
        {
            return;
        }

        SetSelectedColor(DrawingColor.FromArgb(red, green, blue));
    }

    private void TryApplyHexEditor()
    {
        if (_syncingEditors)
        {
            return;
        }

        var text = (_hexBox.Text ?? string.Empty).Trim();
        if (text.StartsWith('#'))
        {
            text = text[1..];
        }
        if (text.Length != 6 || !int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
        {
            return;
        }

        var red = (rgb >> 16) & 0xFF;
        var green = (rgb >> 8) & 0xFF;
        var blue = rgb & 0xFF;
        SetSelectedColor(DrawingColor.FromArgb(red, green, blue));
    }

    private static bool TryByte(string? text, out int value) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value is >= 0 and <= 255;

    private void SetSelectedColor(DrawingColor color)
    {
        _selectedColor = Opaque(color);
        _newPreview.Background = new SolidColorBrush(ToAvaloniaColor(_selectedColor));
        SyncEditors();
        UpdateSwatchSelection();
    }

    private void SyncEditors()
    {
        _syncingEditors = true;
        try
        {
            _redBox.Text = _selectedColor.R.ToString(CultureInfo.InvariantCulture);
            _greenBox.Text = _selectedColor.G.ToString(CultureInfo.InvariantCulture);
            _blueBox.Text = _selectedColor.B.ToString(CultureInfo.InvariantCulture);
            _hexBox.Text = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
        }
        finally
        {
            _syncingEditors = false;
        }
    }

    private void UpdateSwatchSelection()
    {
        var accent = new SolidColorBrush(global::Avalonia.Media.Color.FromRgb(11, 107, 203));
        foreach (var entry in _allSwatches)
        {
            var selected = entry.Color.R == _selectedColor.R && entry.Color.G == _selectedColor.G && entry.Color.B == _selectedColor.B;
            entry.Button.BorderBrush = selected ? accent : Brushes.Gray;
            entry.Button.BorderThickness = selected ? new Thickness(3) : new Thickness(1);
        }
    }

    private static DrawingColor Opaque(DrawingColor color) => DrawingColor.FromArgb(color.R, color.G, color.B);

    private static global::Avalonia.Media.Color ToAvaloniaColor(DrawingColor color) =>
        global::Avalonia.Media.Color.FromRgb(color.R, color.G, color.B);

    private static DrawingColor Hex(string value)
    {
        var rgb = int.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return DrawingColor.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
    }

    private static string Local(string english, string chinese) =>
        DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? chinese : english;

    private sealed class SwatchEntry
    {
        public SwatchEntry(Button button, DrawingColor color)
        {
            Button = button;
            Color = color;
        }

        public Button Button { get; }
        public DrawingColor Color { get; set; }
    }
}

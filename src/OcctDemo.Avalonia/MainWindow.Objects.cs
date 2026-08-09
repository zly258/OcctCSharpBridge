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
    private void RefreshObjectTree()
    {
        if (_session is null) return;
        _refreshingTree = true;
        try
        {
            _objectNodes.Clear();
            var shapeItems = new List<object>();
            var textItems = new List<object>();
            var dimensionItems = new List<object>();

            foreach (var value in Session.Engine.Objects)
            {
                var visible = new CheckBox
                {
                    Content = Session.SafeName(value),
                    IsChecked = true,
                    Tag = value,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                visible.IsCheckedChanged += (_, _) =>
                {
                    if (_refreshingTree || _session is null) return;
                    ExecuteSafe(() => Session.Engine.SetVisible(value, visible.IsChecked == true));
                };

                var item = new TreeViewItem
                {
                    Header = visible,
                    Tag = value,
                    ContextMenu = BuildObjectContextMenu(value)
                };
                _objectNodes[value.Id] = item;
                switch (value.Kind)
                {
                    case OcctObjectKind.Text:
                        textItems.Add(item);
                        break;
                    case OcctObjectKind.Dimension:
                        dimensionItems.Add(item);
                        break;
                    default:
                        shapeItems.Add(item);
                        break;
                }
            }

            var shapeRoot = TreeRoot(Local("Shapes", "形体"), shapeItems);
            var textRoot = TreeRoot(Local("Text", "文字"), textItems);
            var dimensionRoot = TreeRoot(Local("Dimensions", "尺寸"), dimensionItems);
            _objectTree.ItemsSource = new object[] { shapeRoot, textRoot, dimensionRoot };
        }
        finally
        {
            _refreshingTree = false;
        }

        ShowObjectProperties(Session.ActiveObject);
        _selectionStatus.Text = Local(
            $"Objects {Session.Engine.ObjectCount} / Shapes {Session.Engine.ShapeCount}",
            $"对象 {Session.Engine.ObjectCount} / 形体 {Session.Engine.ShapeCount}");
    }

    private ContextMenu BuildObjectContextMenu(IOcctObject value)
    {
        return new ContextMenu
        {
            ItemsSource = new object[]
            {
                MenuItem(DemoLocalization.Text("Menu.FitSelected"), () =>
                {
                    Session.ActiveObject = value;
                    if (value.Kind == OcctObjectKind.Shape) Session.Engine.Fit(Session.Engine.GetShape(value.Id));
                }),
                MenuItem(Local("Show", "显示"), () => Session.Engine.SetVisible(value, true)),
                MenuItem(Local("Hide", "隐藏"), () => Session.Engine.SetVisible(value, false)),
                MenuItem(Local("Color...", "颜色..."), () => SetObjectColor(value)),
                AsyncMenuItem(Local("Material...", "材质..."), () => SetObjectMaterialAsync(value)),
                new Separator(),
                AsyncMenuItem(DemoLocalization.CommandText(DemoCommandId.Delete), async () =>
                {
                    Session.ActiveObject = value;
                    await RunCommandAsync(DemoCommandId.Delete);
                })
            }
        };
    }

    private void ObjectTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_refreshingTree || _session is null || _objectTree.SelectedItem is not TreeViewItem { Tag: IOcctObject value }) return;
        Session.ActiveObject = value;
        Session.Engine.SelectObject(value, false);
        _viewport.RaiseSelectionChanged();
        ShowObjectProperties(value);
        _selectionStatus.Text = Local($"Current: {Session.SafeName(value)}", $"当前：{Session.SafeName(value)}");
    }

    private void ShowObjectProperties(IOcctObject? value)
    {
        _propertyPanel.Children.Clear();
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2*,3*"),
            Background = new SolidColorBrush(AvaloniaColor.Parse("#E7EAED")),
            Margin = new Thickness(0, 0, 0, 2)
        };
        var nameHeader = new TextBlock { Text = DemoLocalization.Text("Property.Name"), FontWeight = FontWeight.SemiBold, Margin = new Thickness(6, 4) };
        var valueHeader = new TextBlock { Text = DemoLocalization.Text("Property.Value"), FontWeight = FontWeight.SemiBold, Margin = new Thickness(6, 4) };
        Grid.SetColumn(valueHeader, 1);
        header.Children.Add(nameHeader);
        header.Children.Add(valueHeader);
        _propertyPanel.Children.Add(header);

        if (value is null || _session is null) return;
        foreach (var property in Session.DescribeObject(value))
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("2*,3*"),
                Background = AvaloniaBrushes.White,
                Margin = new Thickness(0, 0, 0, 1)
            };
            var name = new TextBlock { Text = property.Key, Margin = new Thickness(6, 4), TextWrapping = TextWrapping.Wrap };
            var propertyValue = new TextBlock { Text = property.Value, Margin = new Thickness(6, 4), TextWrapping = TextWrapping.Wrap };
            Grid.SetColumn(propertyValue, 1);
            row.Children.Add(name);
            row.Children.Add(propertyValue);
            _propertyPanel.Children.Add(row);
        }
    }

    private void SelectTreeNode(IOcctObject? value)
    {
        if (value is null || !_objectNodes.TryGetValue(value.Id, out var item)) return;
        item.IsSelected = true;
        item.BringIntoView();
    }

    private OcctShape? ActiveShape()
    {
        if (_session?.ActiveObject is { Kind: OcctObjectKind.Shape } active) return Session.Engine.GetShape(active.Id);
        return _session?.Engine.FirstSelected;
    }
}

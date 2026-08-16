using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using OcctDemo.Common;
using OcctNet;
using AvaloniaBrushes = Avalonia.Media.Brushes;
using AvaloniaColor = Avalonia.Media.Color;
using ContextMenu = Avalonia.Controls.ContextMenu;
using CheckBox = Avalonia.Controls.CheckBox;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private bool _syncingObjectSelection;

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
            var assemblyNodes = new Dictionary<string, (TreeViewItem Node, List<object> Children)>(StringComparer.Ordinal);
            var objects = Session.Engine.GetObjects();

            foreach (var value in objects)
            {
                var hierarchy = value.Kind == OcctObjectKind.Shape
                    ? Session.GetHierarchyPath(value)
                    : Array.Empty<string>();
                var visible = new CheckBox
                {
                    Content = hierarchy.Count > 0 ? hierarchy[^1] : Session.SafeName(value),
                    IsChecked = true,
                    Tag = value,
                    VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Center
                };
                visible.IsCheckedChanged += (_, _) =>
                {
                    if (_refreshingTree || _session is null) return;
                    ExecuteSafe(() => Session.Engine.SetObjectVisible(value, visible.IsChecked == true));
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
                        if (hierarchy.Count > 1)
                        {
                            var children = GetOrCreateAssemblyChildren(
                                shapeItems,
                                hierarchy.Take(hierarchy.Count - 1),
                                assemblyNodes);
                            children.Add(item);
                        }
                        else
                        {
                            shapeItems.Add(item);
                        }
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

        ShowSelectionProperties(Session.Engine.SelectedObjects);
        var objectCount = Session.Engine.ObjectCount;
        var shapeCount = Session.Engine.GetObjects().OfType<OcctShape>().Count();
        _selectionStatus.Text = Local(
            $"Objects {objectCount} / Shapes {shapeCount}",
            $"对象 {objectCount} / 形体 {shapeCount}");
    }

    private static List<object> GetOrCreateAssemblyChildren(
        List<object> rootItems,
        IEnumerable<string> segments,
        IDictionary<string, (TreeViewItem Node, List<object> Children)> cache)
    {
        var parentItems = rootItems;
        var key = string.Empty;
        foreach (var rawSegment in segments)
        {
            var segment = string.IsNullOrWhiteSpace(rawSegment) ? "Assembly" : rawSegment.Trim();
            key = key.Length == 0 ? segment : $"{key}\u001F{segment}";
            if (cache.TryGetValue(key, out var existing))
            {
                parentItems = existing.Children;
                continue;
            }

            var children = new List<object>();
            var node = new TreeViewItem
            {
                Header = segment,
                ItemsSource = children,
                IsExpanded = false
            };
            parentItems.Add(node);
            cache[key] = (node, children);
            parentItems = children;
        }
        return parentItems;
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
                    if (value is OcctShape shape) Session.Engine.Fit(shape);
                }),
                MenuItem(Local("Show", "显示"), () => Session.Engine.SetObjectVisible(value, true)),
                MenuItem(Local("Hide", "隐藏"), () => Session.Engine.SetObjectVisible(value, false)),
                AsyncMenuItem(Local("Color...", "颜色..."), () => SetObjectColorAsync(value)),
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
        if (_syncingObjectSelection || _refreshingTree || _session is null || _objectTree.SelectedItem is not TreeViewItem { Tag: IOcctObject value }) return;
        Session.ActiveObject = value;
        Session.Engine.SelectObject(value, false);
        _viewport.RaiseSelectionChanged();
        ShowObjectProperties(value);
        _selectionStatus.Text = Local($"Current: {Session.SafeName(value)}", $"当前：{Session.SafeName(value)}");
    }

    private void ShowSelectionProperties(IReadOnlyList<IOcctObject> selectedObjects)
    {
        if (selectedObjects.Count == 0)
        {
            ShowObjectProperties(_session?.ActiveObject);
            return;
        }
        if (selectedObjects.Count == 1)
        {
            ShowObjectProperties(selectedObjects[0]);
            return;
        }

        _propertyPanel.Children.Clear();
        AddPropertyHeader();
        AddPropertyRow(
            Local("Selection", "选择"),
            Local($"{selectedObjects.Count} objects selected", $"已选择 {selectedObjects.Count} 个对象"));
    }

    private void ShowObjectProperties(IOcctObject? value)
    {
        _propertyPanel.Children.Clear();
        AddPropertyHeader();
        if (value is null || _session is null) return;

        foreach (var property in Session.DescribeObjectLightweight(value))
        {
            AddPropertyRow(property.Key, property.Value);
        }
    }

    private void AddPropertyHeader()
    {
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
    }

    private void AddPropertyRow(string nameText, string valueText)
    {
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2*,3*"),
            Background = AvaloniaBrushes.White,
            Margin = new Thickness(0, 0, 0, 1)
        };
        var name = new TextBlock { Text = nameText, Margin = new Thickness(6, 4), TextWrapping = TextWrapping.Wrap };
        var propertyValue = new TextBlock { Text = valueText, Margin = new Thickness(6, 4), TextWrapping = TextWrapping.Wrap };
        Grid.SetColumn(propertyValue, 1);
        row.Children.Add(name);
        row.Children.Add(propertyValue);
        _propertyPanel.Children.Add(row);
    }

    private void SelectTreeNode(IOcctObject? value)
    {
        if (value is null || !_objectNodes.TryGetValue(value.Id, out var item)) return;
        _syncingObjectSelection = true;
        try
        {
            item.IsSelected = true;
            item.BringIntoView();
        }
        finally
        {
            _syncingObjectSelection = false;
        }
    }

    private OcctShape? ActiveShape()
    {
        if (_session?.ActiveObject is OcctShape active) return active;
        return _session?.Engine.FirstSelected;
    }
}

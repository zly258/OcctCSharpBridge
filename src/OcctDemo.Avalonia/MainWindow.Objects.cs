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
        if (selectedObjects.OfType<OcctShape>().Count() >= 2)
            AddPropertyRow(DemoLocalization.CommandText(DemoCommandId.AnalyzeDistance), Local("Click to run", "点击执行"), () => RunPropertyInspection(DemoCommandId.AnalyzeDistance));
    }

    private IOcctObject? _propertyTarget;
    private bool _geometryDetailsExpanded;

    private void ShowObjectProperties(IOcctObject? value)
    {
        _propertyTarget = value;
        _geometryDetailsExpanded = false;
        _propertyPanel.Children.Clear();
        AddPropertyHeader();
        if (value is null || _session is null) return;

        foreach (var property in Session.DescribeObjectLightweight(value))
        {
            var isGeometry = property.Key.Contains("Geometry Details", StringComparison.OrdinalIgnoreCase) ||
                             property.Key.Contains("几何详情", StringComparison.Ordinal);
            AddPropertyRow(property.Key, property.Value, isGeometry ? OnGeometryDetailsClick : null);
        }

        AddPropertyRow(Local("Color", "颜色"), Local("Click to change", "点击修改"), () => _ = ChangePropertyColorAsync(value));

        if (value is OcctShape)
        {
            AddPropertyRow(DemoLocalization.CommandText(DemoCommandId.AnalyzeBounds), Local("Click to run", "点击执行"), () => RunPropertyInspection(DemoCommandId.AnalyzeBounds));
            AddPropertyRow(DemoLocalization.CommandText(DemoCommandId.AnalyzeMass), Local("Click to run", "点击执行"), () => RunPropertyInspection(DemoCommandId.AnalyzeMass));
            AddPropertyRow(DemoLocalization.CommandText(DemoCommandId.AnalyzeTopology), Local("Click to run", "点击执行"), () => RunPropertyInspection(DemoCommandId.AnalyzeTopology));
            AddPropertyRow(DemoLocalization.CommandText(DemoCommandId.ValidateShape), Local("Click to run", "点击执行"), () => RunPropertyInspection(DemoCommandId.ValidateShape));
        }
    }

    private async Task ChangePropertyColorAsync(IOcctObject value)
    {
        await SetObjectColorAsync(value);
        if (_propertyTarget is not null) ShowObjectProperties(_propertyTarget);
    }

    private void RunPropertyInspection(DemoCommandId commandId)
    {
        if (_session is null) return;
        ExecuteSafe(() =>
        {
            if (commandId != DemoCommandId.AnalyzeDistance && _propertyTarget is not null)
            {
                Session.Engine.ClearSelection();
                Session.Engine.SelectObject(_propertyTarget, false);
            }
            var result = Session.Execute(commandId);
            _commandStatus.Text = result.Message;
            Log(result.Message);
            if (!string.IsNullOrWhiteSpace(result.AnalysisText))
            {
                foreach (var line in result.AnalysisText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                    Log(line);
            }

            if (commandId != DemoCommandId.AnalyzeDistance && _propertyTarget is not null)
                ShowObjectProperties(_propertyTarget);
            else
            {
                _propertyPanel.Children.Clear();
                AddPropertyHeader();
            }

            AddPropertyRow(Local("Inspection Result", "检查结果"), result.Message);
            if (!string.IsNullOrWhiteSpace(result.AnalysisText))
            {
                foreach (var line in result.AnalysisText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                    AddPropertyRow("  ", line);
            }
        });
    }

    private void OnGeometryDetailsClick()
    {
        if (_session is null || _propertyTarget is null || _geometryDetailsExpanded) return;
        ExecuteSafe(() =>
        {
            _propertyPanel.Children.Clear();
            AddPropertyHeader();
            foreach (var property in Session.DescribeObjectLightweight(_propertyTarget))
            {
                if (property.Key.Contains("Geometry Details", StringComparison.OrdinalIgnoreCase) ||
                    property.Key.Contains("几何详情", StringComparison.Ordinal))
                    continue;
                AddPropertyRow(property.Key, property.Value);
            }
            AddPropertyRow(Local("Geometry Details", "几何详情"), Local("Queried", "已查询"));
            foreach (var property in Session.QueryGeometryDetails(_propertyTarget))
                AddPropertyRow("  " + property.Key, property.Value);
            _geometryDetailsExpanded = true;
        });
    }

    private void AddPropertyHeader()
    {
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2*,3*"),
            Margin = new Thickness(0, 0, 0, 2)
        };
        var nameHeader = new TextBlock { Text = DemoLocalization.Text("Property.Name"), FontWeight = FontWeight.SemiBold, Margin = new Thickness(6, 4) };
        var valueHeader = new TextBlock { Text = DemoLocalization.Text("Property.Value"), FontWeight = FontWeight.SemiBold, Margin = new Thickness(6, 4) };
        Grid.SetColumn(valueHeader, 1);
        header.Children.Add(nameHeader);
        header.Children.Add(valueHeader);
        _propertyPanel.Children.Add(header);
    }

    private void AddPropertyRow(string nameText, string valueText, Action? onClick = null)
    {
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2*,3*"),
            Margin = new Thickness(0, 0, 0, 1)
        };
        var name = new TextBlock { Text = nameText, Margin = new Thickness(6, 4), TextWrapping = TextWrapping.Wrap };
        var propertyValue = new TextBlock { Text = valueText, Margin = new Thickness(6, 4), TextWrapping = TextWrapping.Wrap };
        Grid.SetColumn(propertyValue, 1);
        row.Children.Add(name);
        row.Children.Add(propertyValue);
        if (onClick is not null)
        {
            row.Cursor = new global::Avalonia.Input.Cursor(global::Avalonia.Input.StandardCursorType.Hand);
            row.PointerPressed += (_, _) => onClick();
        }
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

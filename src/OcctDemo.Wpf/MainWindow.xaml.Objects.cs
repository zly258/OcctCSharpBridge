using System.Globalization;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using Input = System.Windows.Input;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private const string StepPathPrefix = "step-path:";
    private const char StepPathSeparator = '\u001F';

    private void RefreshObjectTree()
    {
        if (_session is null) return;
        _refreshingTree = true;
        try
        {
            ObjectTree.Items.Clear();
            _objectNodes.Clear();
            var shapeRoot = TreeRoot(Local("Shapes", "形体"));
            var textRoot = TreeRoot(Local("Text", "文字"));
            var dimensionRoot = TreeRoot(Local("Dimensions", "尺寸"));
            ObjectTree.Items.Add(shapeRoot);
            ObjectTree.Items.Add(textRoot);
            ObjectTree.Items.Add(dimensionRoot);
            var assemblyNodes = new Dictionary<string, Controls.TreeViewItem>(StringComparer.Ordinal);

            foreach (var value in Session.Engine.Objects)
            {
                Controls.TreeViewItem parent;
                if (value.Kind == OcctObjectKind.Shape)
                {
                    var path = GetStepHierarchy(value);
                    parent = path.Count > 1
                        ? GetOrCreateAssemblyNode(shapeRoot, path.Take(path.Count - 1), assemblyNodes)
                        : shapeRoot;
                }
                else
                {
                    parent = value.Kind == OcctObjectKind.Text ? textRoot : dimensionRoot;
                }

                var visible = new Controls.CheckBox
                {
                    Content = Session.SafeName(value),
                    IsChecked = true,
                    Tag = value
                };
                visible.Checked += ObjectVisibilityChanged;
                visible.Unchecked += ObjectVisibilityChanged;
                var item = new Controls.TreeViewItem
                {
                    Header = visible,
                    Tag = value,
                    ContextMenu = BuildObjectContextMenu(value)
                };
                parent.Items.Add(item);
                _objectNodes[value.Id] = item;
            }
            shapeRoot.IsExpanded = true;
            textRoot.IsExpanded = true;
            dimensionRoot.IsExpanded = true;
        }
        finally
        {
            _refreshingTree = false;
        }

        ShowSelectionProperties(Session.Engine.SelectedObjects);
        SelectionStatus.Text = Local(
            $"Objects {Session.Engine.ObjectCount} / Shapes {Session.Engine.ShapeCount}",
            $"对象 {Session.Engine.ObjectCount} / 形体 {Session.Engine.ShapeCount}");
    }

    private Controls.TreeViewItem GetOrCreateAssemblyNode(
        Controls.TreeViewItem root,
        IEnumerable<string> segments,
        IDictionary<string, Controls.TreeViewItem> cache)
    {
        var parent = root;
        var key = string.Empty;
        foreach (var rawSegment in segments)
        {
            var segment = string.IsNullOrWhiteSpace(rawSegment) ? Local("Assembly", "装配") : rawSegment.Trim();
            key = key.Length == 0 ? segment : $"{key}{StepPathSeparator}{segment}";
            if (cache.TryGetValue(key, out var existing))
            {
                parent = existing;
                continue;
            }

            var node = new Controls.TreeViewItem
            {
                Header = segment,
                IsExpanded = false
            };
            parent.Items.Add(node);
            cache[key] = node;
            parent = node;
        }
        return parent;
    }

    private IReadOnlyList<string> GetStepHierarchy(IOcctObject value)
    {
        if (_session is null || value.Kind != OcctObjectKind.Shape) return Array.Empty<string>();
        var tag = Session.Engine.GetApplicationTag(value);
        if (!tag.StartsWith(StepPathPrefix, StringComparison.Ordinal)) return Array.Empty<string>();
        return tag[StepPathPrefix.Length..]
            .Split(StepPathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private Controls.ContextMenu BuildObjectContextMenu(IOcctObject value)
    {
        var menu = new Controls.ContextMenu();
        menu.Items.Add(MenuItem(DemoLocalization.Text("Menu.FitSelected"), (_, _) =>
        {
            Session.ActiveObject = value;
            if (value.Kind == OcctObjectKind.Shape) Session.Engine.Fit(Session.Engine.GetShape(value.Id));
        }));
        menu.Items.Add(MenuItem(Local("Show", "显示"), (_, _) => Session.Engine.SetVisible(value, true)));
        menu.Items.Add(MenuItem(Local("Hide", "隐藏"), (_, _) => Session.Engine.SetVisible(value, false)));
        menu.Items.Add(MenuItem(Local("Color...", "颜色..."), (_, _) => SetObjectColor(value)));
        menu.Items.Add(MenuItem(Local("Material...", "材质..."), (_, _) => SetObjectMaterial(value)));
        menu.Items.Add(new Controls.Separator());
        menu.Items.Add(MenuItem(DemoLocalization.CommandText(DemoCommandId.Delete), (_, _) =>
        {
            Session.ActiveObject = value;
            RunCommand(DemoCommandId.Delete);
        }));
        return menu;
    }

    private void ObjectVisibilityChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_refreshingTree || _session is null || sender is not Controls.CheckBox { Tag: IOcctObject value } checkBox) return;
        ExecuteSafe(() => Session.Engine.SetVisible(value, checkBox.IsChecked == true));
    }

    private void ObjectTreeSelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
    {
        if (_session is null || e.NewValue is not Controls.TreeViewItem { Tag: IOcctObject value }) return;
        Session.ActiveObject = value;
        Session.Engine.SelectObject(value, false);
        Viewport.RaiseSelectionChanged();
        SelectionStatus.Text = Local($"Current: {Session.SafeName(value)}", $"当前：{Session.SafeName(value)}");
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

        PropertyGrid.ItemsSource = new[]
        {
            new KeyValuePair<string, string>(
                Local("Selection", "选择"),
                Local($"{selectedObjects.Count} objects selected", $"已选择 {selectedObjects.Count} 个对象"))
        };
    }

    private void ShowObjectProperties(IOcctObject? value)
    {
        if (value is null || _session is null)
        {
            PropertyGrid.ItemsSource = null;
            return;
        }

        // Selection changes must stay O(1). Expensive B-Rep validation, bounds and
        // topology traversal belong to the explicit Analysis commands, not this UI path.
        var rows = new List<KeyValuePair<string, string>>
        {
            new(DemoLocalization.Text("Object.Id"), value.Id.ToString(CultureInfo.InvariantCulture)),
            new(DemoLocalization.Text("Object.Name"), Session.SafeName(value)),
            new(DemoLocalization.Text("Object.Kind"), DemoLocalization.ObjectKind(value.Kind))
        };

        var hierarchy = GetStepHierarchy(value);
        if (hierarchy.Count > 1)
        {
            rows.Add(new KeyValuePair<string, string>(
                Local("Assembly Path", "装配路径"),
                string.Join(" / ", hierarchy.Take(hierarchy.Count - 1))));
        }
        if (value.Kind == OcctObjectKind.Shape)
        {
            rows.Add(new KeyValuePair<string, string>(
                Local("Geometry Details", "几何详情"),
                Local("Use Analysis commands on demand", "请按需使用“分析”命令")));
        }
        PropertyGrid.ItemsSource = rows;
    }

    private void SelectTreeNode(IOcctObject? value)
    {
        if (value is null || !_objectNodes.TryGetValue(value.Id, out var item)) return;
        ExpandAncestors(item);
        item.IsSelected = true;
        item.BringIntoView();
    }

    private static void ExpandAncestors(Controls.TreeViewItem item)
    {
        var parent = ItemsControl.ItemsControlFromItemContainer(item) as Controls.TreeViewItem;
        while (parent is not null)
        {
            parent.IsExpanded = true;
            parent = ItemsControl.ItemsControlFromItemContainer(parent) as Controls.TreeViewItem;
        }
    }

    private OcctShape? ActiveShape()
    {
        if (_session?.ActiveObject is { Kind: OcctObjectKind.Shape } active) return Session.Engine.GetShape(active.Id);
        return _session?.Engine.FirstSelected;
    }
}

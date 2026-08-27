using System.Linq;
﻿using OcctDemo.Common;
using OcctNet;
using Controls = System.Windows.Controls;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private bool _syncingObjectSelection;

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
            var objects = Session.Engine.GetObjects();

            foreach (var value in objects)
            {
                var hierarchy = value.Kind == OcctObjectKind.Shape
                    ? Session.GetHierarchyPath(value)
                    : Array.Empty<string>();
                Controls.TreeViewItem parent;
                if (value.Kind == OcctObjectKind.Shape)
                {
                    parent = hierarchy.Count > 1
                        ? GetOrCreateAssemblyNode(shapeRoot, hierarchy.Take(hierarchy.Count - 1), assemblyNodes)
                        : shapeRoot;
                }
                else
                {
                    parent = value.Kind == OcctObjectKind.Text ? textRoot : dimensionRoot;
                }

                var visible = new Controls.CheckBox
                {
                    Content = hierarchy.Count > 0 ? hierarchy[^1] : Session.SafeName(value),
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
        var objectCount = Session.Engine.ObjectCount;
        var shapeCount = Session.Engine.GetObjects().OfType<OcctShape>().Count();
        SelectionStatus.Text = Local(
            $"Objects {objectCount} / Shapes {shapeCount}",
            $"对象 {objectCount} / 形体 {shapeCount}");
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
            key = key.Length == 0 ? segment : $"{key}\u001F{segment}";
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

    private Controls.ContextMenu BuildObjectContextMenu(IOcctObject value)
    {
        var menu = new Controls.ContextMenu();
        menu.Items.Add(MenuItem(DemoLocalization.Text("Menu.FitSelected"), (_, _) =>
        {
            Session.ActiveObject = value;
            if (value is OcctShape shape) Session.Engine.Fit(shape);
        }));
        menu.Items.Add(MenuItem(Local("Show", "显示"), (_, _) => Session.Engine.SetObjectVisible(value, true)));
        menu.Items.Add(MenuItem(Local("Hide", "隐藏"), (_, _) => Session.Engine.SetObjectVisible(value, false)));
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
        ExecuteSafe(() => Session.Engine.SetObjectVisible(value, checkBox.IsChecked == true));
    }

    private void ObjectTreeSelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
    {
        if (_syncingObjectSelection || _session is null || e.NewValue is not Controls.TreeViewItem { Tag: IOcctObject value }) return;
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

        var rows = new List<KeyValuePair<string, string>>
        {
            new(Local("Selection", "选择"),
                Local($"{selectedObjects.Count} objects selected", $"已选择 {selectedObjects.Count} 个对象"))
        };
        if (selectedObjects.OfType<OcctShape>().Count() >= 2)
            rows.Add(new("▶ " + DemoLocalization.CommandText(DemoCommandId.AnalyzeDistance), Local("Click to run", "点击执行")));
        PropertyGrid.ItemsSource = rows;
    }

    private IOcctObject? _propertyTarget;
    private bool _geometryDetailsExpanded;

    private void ShowObjectProperties(IOcctObject? value)
    {
        _propertyTarget = value;
        _geometryDetailsExpanded = false;
        if (value is null || _session is null)
        {
            PropertyGrid.ItemsSource = null;
        }
        else
        {
            var rows = Session.DescribeObjectLightweight(value).ToList();
            if (value is OcctShape)
            {
                rows.Add(new("▶ " + DemoLocalization.CommandText(DemoCommandId.AnalyzeBounds), Local("Click to run", "点击执行")));
                rows.Add(new("▶ " + DemoLocalization.CommandText(DemoCommandId.AnalyzeMass), Local("Click to run", "点击执行")));
                rows.Add(new("▶ " + DemoLocalization.CommandText(DemoCommandId.AnalyzeTopology), Local("Click to run", "点击执行")));
                rows.Add(new("▶ " + DemoLocalization.CommandText(DemoCommandId.ValidateShape), Local("Click to run", "点击执行")));
            }
            PropertyGrid.ItemsSource = rows;
        }
        PropertyGrid.MouseDoubleClick -= PropertyGridOnDoubleClick;
        PropertyGrid.MouseDoubleClick += PropertyGridOnDoubleClick;
        PropertyGrid.MouseLeftButtonUp -= PropertyGridOnMouseLeftButtonUp;
        PropertyGrid.MouseLeftButtonUp += PropertyGridOnMouseLeftButtonUp;
    }

    private void PropertyGridOnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_session is null || PropertyGrid.SelectedItem is not KeyValuePair<string, string> row) return;
        var key = row.Key ?? string.Empty;
        if (!key.StartsWith("▶ ", StringComparison.Ordinal)) return;

        var label = key[2..];
        var commandId =
            label == DemoLocalization.CommandText(DemoCommandId.AnalyzeBounds) ? DemoCommandId.AnalyzeBounds :
            label == DemoLocalization.CommandText(DemoCommandId.AnalyzeMass) ? DemoCommandId.AnalyzeMass :
            label == DemoLocalization.CommandText(DemoCommandId.AnalyzeTopology) ? DemoCommandId.AnalyzeTopology :
            label == DemoLocalization.CommandText(DemoCommandId.ValidateShape) ? DemoCommandId.ValidateShape :
            label == DemoLocalization.CommandText(DemoCommandId.AnalyzeDistance) ? DemoCommandId.AnalyzeDistance :
            (DemoCommandId?)null;
        if (commandId is null) return;

        ExecuteSafe(() =>
        {
            if (commandId.Value != DemoCommandId.AnalyzeDistance && _propertyTarget is not null)
            {
                Session.Engine.ClearSelection();
                Session.Engine.SelectObject(_propertyTarget, false);
            }
            var result = Session.Execute(commandId.Value);
            CommandStatus.Text = result.Message;
            Log(result.Message);
            if (!string.IsNullOrWhiteSpace(result.AnalysisText)) Log(result.AnalysisText);

            var rows = commandId.Value == DemoCommandId.AnalyzeDistance || _propertyTarget is null
                ? new List<KeyValuePair<string, string>>()
                : Session.DescribeObjectLightweight(_propertyTarget).ToList();
            rows.Add(new(Local("Inspection Result", "检查结果"), result.Message));
            if (!string.IsNullOrWhiteSpace(result.AnalysisText))
            {
                foreach (var line in result.AnalysisText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                    rows.Add(new("  ", line));
            }
            PropertyGrid.ItemsSource = rows;
        });
    }

    private void PropertyGridOnDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_session is null || _propertyTarget is null || _geometryDetailsExpanded) return;
        if (PropertyGrid.SelectedItem is not System.Collections.Generic.KeyValuePair<string, string> row) return;
        var key = row.Key ?? "";
        if (!key.Contains("Geometry Details", StringComparison.OrdinalIgnoreCase) &&
            !key.Contains("几何详情", StringComparison.Ordinal))
            return;

        ExecuteSafe(() =>
        {
            var baseRows = Session.DescribeObjectLightweight(_propertyTarget)
                .Where(p => !p.Key.Contains("Geometry Details", StringComparison.OrdinalIgnoreCase) &&
                            !p.Key.Contains("几何详情", StringComparison.Ordinal))
                .ToList();
            baseRows.Add(new(Local("Geometry Details", "几何详情"), Local("Queried", "已查询")));
            baseRows.AddRange(Session.QueryGeometryDetails(_propertyTarget));
            PropertyGrid.ItemsSource = baseRows;
            _geometryDetailsExpanded = true;
        });
    }

    private void SelectTreeNode(IOcctObject? value)
    {
        if (value is null || !_objectNodes.TryGetValue(value.Id, out var item)) return;
        _syncingObjectSelection = true;
        try
        {
            ExpandAncestors(item);
            item.IsSelected = true;
            item.BringIntoView();
        }
        finally
        {
            _syncingObjectSelection = false;
        }
    }

    private static void ExpandAncestors(Controls.TreeViewItem item)
    {
        var parent = Controls.ItemsControl.ItemsControlFromItemContainer(item) as Controls.TreeViewItem;
        while (parent is not null)
        {
            parent.IsExpanded = true;
            parent = Controls.ItemsControl.ItemsControlFromItemContainer(parent) as Controls.TreeViewItem;
        }
    }

    private OcctShape? ActiveShape()
    {
        if (_session?.ActiveObject is OcctShape active) return active;
        return _session?.Engine.FirstSelected;
    }
}

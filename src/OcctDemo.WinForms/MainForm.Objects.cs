using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private bool _syncingObjectSelection;

    private ContextMenuStrip BuildTreeContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(DemoLocalization.Text("Menu.FitSelected"), null, (_, _) => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); });
        menu.Items.Add(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "显示" : "Show", null, (_, _) => SetActiveVisibility(true));
        menu.Items.Add(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "隐藏" : "Hide", null, (_, _) => SetActiveVisibility(false));
        menu.Items.Add(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "颜色..." : "Color...", null, (_, _) => SetActiveColor());
        menu.Items.Add(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "材质..." : "Material...", null, (_, _) => SetActiveMaterial());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(DemoLocalization.CommandText(DemoCommandId.Delete), null, (_, _) => RunCommand(DemoCommandId.Delete));
        return menu;
    }

    private void RefreshObjectTree()
    {
        if (_session is null) return;
        _refreshingTree = true;
        try
        {
            _objectTree.BeginUpdate();
            _objectTree.Nodes.Clear();
            _objectNodes.Clear();
            var shapeRoot = _objectTree.Nodes.Add(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "形体" : "Shapes");
            var textRoot = _objectTree.Nodes.Add(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "文字" : "Text");
            var dimensionRoot = _objectTree.Nodes.Add(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "尺寸" : "Dimensions");
            var assemblyNodes = new Dictionary<string, TreeNode>(StringComparer.Ordinal);
            var objects = Session.Engine.GetObjects();

            foreach (var value in objects)
            {
                var hierarchy = value.Kind == OcctObjectKind.Shape
                    ? Session.GetHierarchyPath(value)
                    : Array.Empty<string>();
                TreeNode parent;
                if (value.Kind == OcctObjectKind.Shape)
                {
                    parent = hierarchy.Count > 1
                        ? GetOrCreateAssemblyNode(shapeRoot, hierarchy.Take(hierarchy.Count - 1), assemblyNodes)
                        : shapeRoot;
                }
                else
                {
                    parent = value.Kind switch
                    {
                        OcctObjectKind.Text => textRoot,
                        OcctObjectKind.Dimension => dimensionRoot,
                        _ => shapeRoot
                    };
                }

                var displayName = hierarchy.Count > 0 ? hierarchy[^1] : Session.SafeName(value);
                var node = parent.Nodes.Add(displayName);
                node.Tag = value;
                node.Checked = true;
                _objectNodes[value.Id] = node;
            }
            shapeRoot.Expand();
            textRoot.Expand();
            dimensionRoot.Expand();
        }
        finally
        {
            _objectTree.EndUpdate();
            _refreshingTree = false;
        }
        ShowObjectProperties(Session.ActiveObject);
        var objectCount = Session.Engine.ObjectCount;
        var shapeCount = Session.Engine.GetObjects().OfType<OcctShape>().Count();
        _selectionStatus.Text = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? $"对象 {objectCount} / 形体 {shapeCount}"
            : $"Objects {objectCount} / Shapes {shapeCount}";
    }

    private static TreeNode GetOrCreateAssemblyNode(
        TreeNode root,
        IEnumerable<string> segments,
        IDictionary<string, TreeNode> cache)
    {
        var parent = root;
        var key = string.Empty;
        foreach (var rawSegment in segments)
        {
            var segment = string.IsNullOrWhiteSpace(rawSegment) ? "Assembly" : rawSegment.Trim();
            key = key.Length == 0 ? segment : $"{key}\u001F{segment}";
            if (cache.TryGetValue(key, out var existing))
            {
                parent = existing;
                continue;
            }

            var node = parent.Nodes.Add(segment);
            cache[key] = node;
            parent = node;
        }
        return parent;
    }

    private void ObjectTreeAfterSelect(object? sender, TreeViewEventArgs e)
    {
        var node = e.Node;
        if (_syncingObjectSelection || _session is null || node is null || node.Tag is not IOcctObject value) return;

        Session.ActiveObject = value;
        Session.Engine.SelectObject(value, false);
        _viewport.RaiseSelectionChanged();
        ShowObjectProperties(value);
        _selectionStatus.Text = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? $"当前：{Session.SafeName(value)}"
            : $"Current: {Session.SafeName(value)}";
    }

    private void ObjectTreeAfterCheck(object? sender, TreeViewEventArgs e)
    {
        var node = e.Node;
        if (_refreshingTree || _session is null || node is null || node.Tag is not IOcctObject value) return;

        ExecuteSafe(() => Session.Engine.SetObjectVisible(value, node.Checked));
    }

    private IOcctObject? _propertyTarget;
    private bool _geometryDetailsExpanded;

    private void ShowObjectProperties(IOcctObject? value)
    {
        _propertyTarget = value;
        _geometryDetailsExpanded = false;
        _propertyGrid.Rows.Clear();
        if (_session is null || value is null) return;
        foreach (var property in Session.DescribeObjectLightweight(value))
            _propertyGrid.Rows.Add(property.Key, property.Value);
        if (value is OcctShape)
        {
            _propertyGrid.Rows.Add("▶ " + DemoLocalization.CommandText(DemoCommandId.AnalyzeBounds), Local("Click to run", "点击执行"));
            _propertyGrid.Rows.Add("▶ " + DemoLocalization.CommandText(DemoCommandId.AnalyzeMass), Local("Click to run", "点击执行"));
            _propertyGrid.Rows.Add("▶ " + DemoLocalization.CommandText(DemoCommandId.AnalyzeTopology), Local("Click to run", "点击执行"));
            _propertyGrid.Rows.Add("▶ " + DemoLocalization.CommandText(DemoCommandId.ValidateShape), Local("Click to run", "点击执行"));
        }
        EnsurePropertyGridClickHandler();
    }

    private void EnsurePropertyGridClickHandler()
    {
        _propertyGrid.CellClick -= PropertyGridOnCellClick;
        _propertyGrid.CellClick += PropertyGridOnCellClick;
    }

    private void PropertyGridOnCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _session is null || _propertyTarget is null) return;
        var key = _propertyGrid.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "";
        if (key.StartsWith("▶ ", StringComparison.Ordinal))
        {
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
                _commandStatus.Text = result.Message;
                Log(result.Message);
                if (!string.IsNullOrWhiteSpace(result.AnalysisText)) Log(result.AnalysisText);

                if (commandId.Value != DemoCommandId.AnalyzeDistance && _propertyTarget is not null)
                    ShowObjectProperties(_propertyTarget);
                else
                    _propertyGrid.Rows.Clear();
                _propertyGrid.Rows.Add(Local("Inspection Result", "检查结果"), result.Message);
                if (!string.IsNullOrWhiteSpace(result.AnalysisText))
                {
                    foreach (var line in result.AnalysisText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                        _propertyGrid.Rows.Add("  ", line);
                }
            });
            return;
        }

        var isGeometry =
            key.Contains("Geometry Details", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("几何详情", StringComparison.Ordinal);
        if (!isGeometry || _geometryDetailsExpanded) return;

        ExecuteSafe(() =>
        {
            var details = Session.QueryGeometryDetails(_propertyTarget);
            if (details.Count == 0) return;
            // Replace the placeholder row with detailed rows
            _propertyGrid.Rows.Clear();
            foreach (var property in Session.DescribeObjectLightweight(_propertyTarget))
            {
                if (property.Key.Contains("Geometry Details", StringComparison.OrdinalIgnoreCase) ||
                    property.Key.Contains("几何详情", StringComparison.Ordinal))
                    continue;
                _propertyGrid.Rows.Add(property.Key, property.Value);
            }
            _propertyGrid.Rows.Add(
                Local("Geometry Details", "几何详情"),
                Local("Queried", "已查询"));
            foreach (var property in details)
                _propertyGrid.Rows.Add("  " + property.Key, property.Value);
            _geometryDetailsExpanded = true;
            _commandStatus.Text = Local("Geometry details loaded.", "几何详情已加载。");
        });
    }

    private void SelectTreeNode(IOcctObject? value)
    {
        if (value is null || !_objectNodes.TryGetValue(value.Id, out var node)) return;
        _syncingObjectSelection = true;
        try
        {
            for (var parent = node.Parent; parent is not null; parent = parent.Parent)
                parent.Expand();
            _objectTree.SelectedNode = node;
            node.EnsureVisible();
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

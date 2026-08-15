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

        ExecuteSafe(() => Session.Engine.SetVisible(value, node.Checked));
    }

    private void ShowObjectProperties(IOcctObject? value)
    {
        _propertyGrid.Rows.Clear();
        if (_session is null || value is null) return;
        foreach (var property in Session.DescribeObjectLightweight(value))
            _propertyGrid.Rows.Add(property.Key, property.Value);
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

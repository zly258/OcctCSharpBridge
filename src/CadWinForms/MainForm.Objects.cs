using System.Globalization;
using CadCommon;
using OcctNet;

namespace CadWinForms;

public sealed partial class MainForm
{
    private ContextMenuStrip BuildTreeContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(CadLocalization.Text("Menu.FitSelected"), null, (_, _) => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); });
        menu.Items.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "显示" : "Show", null, (_, _) => SetActiveVisibility(true));
        menu.Items.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "隐藏" : "Hide", null, (_, _) => SetActiveVisibility(false));
        menu.Items.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "颜色..." : "Color...", null, (_, _) => SetActiveColor());
        menu.Items.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "材质..." : "Material...", null, (_, _) => SetActiveMaterial());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(CadLocalization.CommandText(CadCommandId.Delete), null, (_, _) => RunCommand(CadCommandId.Delete));
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
            var shapeRoot = _objectTree.Nodes.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "形体" : "Shapes");
            var textRoot = _objectTree.Nodes.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "文字" : "Text");
            var dimensionRoot = _objectTree.Nodes.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "尺寸" : "Dimensions");
            foreach (var value in Session.Engine.Objects)
            {
                var parent = value.Kind switch
                {
                    OcctObjectKind.Text => textRoot,
                    OcctObjectKind.Dimension => dimensionRoot,
                    _ => shapeRoot
                };
                var node = parent.Nodes.Add(Session.SafeName(value));
                node.Tag = value;
                node.Checked = true;
                _objectNodes[value.Id] = node;
            }
            shapeRoot.Expand(); textRoot.Expand(); dimensionRoot.Expand();
        }
        finally
        {
            _objectTree.EndUpdate();
            _refreshingTree = false;
        }
        ShowObjectProperties(Session.ActiveObject);
        _selectionStatus.Text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"对象 {Session.Engine.ObjectCount} / 形体 {Session.Engine.ShapeCount}" : $"Objects {Session.Engine.ObjectCount} / Shapes {Session.Engine.ShapeCount}";
    }

    private void ObjectTreeAfterSelect(object? sender, TreeViewEventArgs e)
    {
        var node = e.Node;
        if (_session is null || node is null || node.Tag is not IOcctObject value) return;

        Session.ActiveObject = value;
        Session.Engine.SelectObject(value, false);
        _viewport.RaiseSelectionChanged();
        ShowObjectProperties(value);
        _selectionStatus.Text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"当前：{Session.SafeName(value)}" : $"Current: {Session.SafeName(value)}";
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
        foreach (var property in Session.DescribeObject(value)) _propertyGrid.Rows.Add(property.Key, property.Value);
    }

    private void SelectTreeNode(IOcctObject? value)
    {
        if (value is null || !_objectNodes.TryGetValue(value.Id, out var node)) return;
        _objectTree.SelectedNode = node;
        node.EnsureVisible();
    }

    private OcctShape? ActiveShape()
    {
        if (_session?.ActiveObject is { Kind: OcctObjectKind.Shape } active) return Session.Engine.GetShape(active.Id);
        return _session?.Engine.FirstSelected;
    }
}

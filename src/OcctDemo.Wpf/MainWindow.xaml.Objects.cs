using System.Globalization;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using Input = System.Windows.Input;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
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

            foreach (var value in Session.Engine.Objects)
            {
                var parent = value.Kind switch
                {
                    OcctObjectKind.Text => textRoot,
                    OcctObjectKind.Dimension => dimensionRoot,
                    _ => shapeRoot
                };
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

        ShowObjectProperties(Session.ActiveObject);
        SelectionStatus.Text = Local(
            $"Objects {Session.Engine.ObjectCount} / Shapes {Session.Engine.ShapeCount}",
            $"对象 {Session.Engine.ObjectCount} / 形体 {Session.Engine.ShapeCount}");
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
        ShowObjectProperties(value);
        SelectionStatus.Text = Local($"Current: {Session.SafeName(value)}", $"当前：{Session.SafeName(value)}");
    }

    private void ShowObjectProperties(IOcctObject? value)
    {
        PropertyGrid.ItemsSource = value is null || _session is null ? null : Session.DescribeObject(value);
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

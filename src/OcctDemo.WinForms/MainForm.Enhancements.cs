using System.Globalization;
using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private void ApplyDemoEnhancements()
    {
        _selectionCombo.SelectedIndexChanged -= SelectionComboSelectedIndexChanged;
        _selectionCombo.SelectedIndexChanged += SelectionComboSelectedIndexChanged;

        var samples = _menu.Items
            .OfType<ToolStripMenuItem>()
            .FirstOrDefault(item => item.Text == DemoLocalization.Text("Menu.Samples"));
        if (samples is not null)
        {
            samples.DropDownItems.Add(new ToolStripSeparator());
            samples.DropDownItems.Add(MenuItem(
                Local("B-Spline Surface Test", "B 样条曲面测试"),
                (_, _) => RunModelingTest(Session.RunBSplineSurfaceTest)));
            samples.DropDownItems.Add(MenuItem(
                Local("Mesh Generation Test", "网格生成测试"),
                (_, _) => RunModelingTest(Session.RunMeshGenerationTest)));
            samples.DropDownItems.Add(MenuItem(
                Local("Viewer Projection Test", "Viewer 投影测试"),
                (_, _) => RunModelingTest(Session.RunViewerProjectionTest)));
        }

        var view = _menu.Items
            .OfType<ToolStripMenuItem>()
            .FirstOrDefault(item => item.Text == DemoLocalization.Text("Menu.View"));
        if (view is not null)
        {
            RebuildCompactViewMenu(view);
        }
    }

    private void RebuildCompactViewMenu(ToolStripMenuItem view)
    {
        view.DropDownItems.Clear();
        view.DropDownItems.Add(BuildCompactStandardViewsMenu());
        view.DropDownItems.Add(BuildCompactDisplayStyleMenu());
        view.DropDownItems.Add(new ToolStripSeparator());
        view.DropDownItems.Add(MenuItem(
            Local("View Settings...", "视图设置..."),
            (_, _) => ShowAdvancedViewSettingsWindow()));
    }

    private ToolStripMenuItem BuildCompactStandardViewsMenu()
    {
        var standard = new ToolStripMenuItem(DemoLocalization.Text("Menu.StandardViews"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        standard.DropDownItems.Add(new ToolStripSeparator());
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(DemoIsoView.NorthEast)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(DemoIsoView.NorthWest)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(DemoIsoView.SouthEast)));
        standard.DropDownItems.Add(MenuItem(DemoLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(DemoIsoView.SouthWest)));
        return standard;
    }

    private ToolStripMenuItem BuildCompactDisplayStyleMenu()
    {
        var display = new ToolStripMenuItem(Local("Display Style", "显示样式"));
        display.DropDownItems.Add(RadioMenuItem(DemoLocalization.Text("Menu.Shaded"), _displayMode == OcctDisplayMode.Shaded, (_, _) => SetDisplayStyle(OcctDisplayMode.Shaded)));
        display.DropDownItems.Add(RadioMenuItem(DemoLocalization.Text("Menu.Wireframe"), _displayMode == OcctDisplayMode.Wireframe, (_, _) => SetDisplayStyle(OcctDisplayMode.Wireframe)));
        return display;
    }

    private ToolStripMenuItem RadioMenuItem(string text, bool checkedState, EventHandler click)
    {
        var item = new ToolStripMenuItem(text) { Checked = checkedState };
        item.Click += click;
        return item;
    }

    private void SelectionComboSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_selectionCombo.SelectedIndex >= 0)
        {
            SetSelectionMode((OcctSelectionMode)_selectionCombo.SelectedIndex);
        }
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

        _propertyGrid.Rows.Clear();
        _propertyGrid.Rows.Add(
            Local("Selection", "选择"),
            Local($"{selectedObjects.Count} objects selected", $"已选择 {selectedObjects.Count} 个对象"));
    }

    private void RunModelingTest(Func<DemoCommandResult> test)
    {
        ExecuteSafe(() =>
        {
            var result = test();
            _commandStatus.Text = result.Message;
            Log(result.Message);
            if (!string.IsNullOrWhiteSpace(result.AnalysisText)) Log(result.AnalysisText);
            RefreshObjectTree();
        });
    }

    private void SetZoomSensitivity()
    {
        var parameters = new[]
        {
            new DemoParameterDefinition(
                "value",
                Local("Zoom Sensitivity", "缩放灵敏度"),
                DemoParameterKind.Number,
                _viewport.ZoomSensitivity.ToString("0.##", CultureInfo.InvariantCulture),
                "×")
        };
        if (!ParameterDialog.TryGetValues(this, Local("Zoom Sensitivity", "缩放灵敏度"), parameters, out var raw)) return;

        var value = new DemoValues(raw).Number("value", 1.0);
        ExecuteSafe(() =>
        {
            _viewport.ZoomSensitivity = value;
            var message = Local(
                $"Zoom sensitivity: {_viewport.ZoomSensitivity:0.##}×",
                $"缩放灵敏度：{_viewport.ZoomSensitivity:0.##}×");
            _commandStatus.Text = message;
            Log(message);
        });
    }
}

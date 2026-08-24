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
                Local("B-Spline Curve Fit Test", "B 样条曲线拟合测试"),
                (_, _) => RunModelingTest(Session.RunCurveFitTest)));
            samples.DropDownItems.Add(MenuItem(
                Local("PipeShell Sweep Test", "PipeShell 高级扫掠测试"),
                (_, _) => RunModelingTest(Session.RunPipeShellTest)));
            samples.DropDownItems.Add(MenuItem(
                Local("Edge Intersection Test", "几何边求交测试"),
                (_, _) => RunModelingTest(Session.RunEdgeIntersectionTest)));
            samples.DropDownItems.Add(MenuItem(
                Local("glTF / OBJ Exchange Test", "glTF / OBJ 数据交换测试"),
                (_, _) => RunModelingTest(Session.RunObjGltfExchangeTest)));
            samples.DropDownItems.Add(MenuItem(
                Local("Mesh Generation Test", "网格生成测试"),
                (_, _) => RunModelingTest(Session.RunMeshGenerationTest)));
            samples.DropDownItems.Add(MenuItem(
                Local("Viewer Projection Test", "Viewer 投影测试"),
                (_, _) => RunModelingTest(Session.RunViewerProjectionTest)));
        }

        // The View menu stays as built by BuildViewMenu() — fully flattened,
        // with the "View Settings..." entry at the end.
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

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
        }

        var view = _menu.Items
            .OfType<ToolStripMenuItem>()
            .FirstOrDefault(item => item.Text == DemoLocalization.Text("Menu.View"));
        if (view is not null)
        {
            view.DropDownItems.Add(new ToolStripSeparator());
            view.DropDownItems.Add(MenuItem(
                Local("Zoom Sensitivity...", "缩放灵敏度..."),
                (_, _) => SetZoomSensitivity()));
        }
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
        var current = DemoViewportSettings.GetZoomSensitivity(_viewport);
        var parameters = new[]
        {
            new DemoParameterDefinition(
                "value",
                Local("Zoom Sensitivity", "缩放灵敏度"),
                DemoParameterKind.Number,
                current.ToString("0.##", CultureInfo.InvariantCulture),
                "×")
        };
        if (!ParameterDialog.TryGetValues(this, Local("Zoom Sensitivity", "缩放灵敏度"), parameters, out var raw)) return;

        var value = new DemoValues(raw).Number("value", 1.0);
        ExecuteSafe(() =>
        {
            if (!DemoViewportSettings.TrySetZoomSensitivity(_viewport, value))
            {
                throw new InvalidOperationException(Local(
                    "The Binary SDK does not expose ZoomSensitivity yet. Publish main and sync the demo distribution first.",
                    "当前 Binary SDK 尚未提供 ZoomSensitivity。请先发布 main 并同步 demo 的二进制分发。"));
            }
            var applied = DemoViewportSettings.GetZoomSensitivity(_viewport);
            var message = Local($"Zoom sensitivity: {applied:0.##}×", $"缩放灵敏度：{applied:0.##}×");
            _commandStatus.Text = message;
            Log(message);
        });
    }
}

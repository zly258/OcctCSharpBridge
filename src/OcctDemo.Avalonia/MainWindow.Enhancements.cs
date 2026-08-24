using System.Globalization;
using Avalonia.Controls;
using OcctDemo.Common;
using OcctNet;
using MenuItem = Avalonia.Controls.MenuItem;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private void ApplyDemoEnhancements()
    {
        var rootItems = (_mainMenu.ItemsSource as IEnumerable<object>)?.ToArray() ?? Array.Empty<object>();

        var samples = rootItems
            .OfType<MenuItem>()
            .FirstOrDefault(item => Equals(item.Header, MenuHeader("Menu.Samples")));
        if (samples is not null)
        {
            var items = (samples.ItemsSource as IEnumerable<object>)?.ToList() ?? new List<object>();
            items.Add(MenuItem(
                Local("B-Spline Surface Test", "B 样条曲面测试"),
                () => RunModelingTest(Session.RunBSplineSurfaceTest)));
            items.Add(MenuItem(
                Local("B-Spline Curve Fit Test", "B 样条曲线拟合测试"),
                () => RunModelingTest(Session.RunCurveFitTest)));
            items.Add(MenuItem(
                Local("PipeShell Sweep Test", "PipeShell 高级扫掠测试"),
                () => RunModelingTest(Session.RunPipeShellTest)));
            items.Add(MenuItem(
                Local("Edge Intersection Test", "几何边求交测试"),
                () => RunModelingTest(Session.RunEdgeIntersectionTest)));
            items.Add(MenuItem(
                Local("glTF / OBJ Exchange Test", "glTF / OBJ 数据交换测试"),
                () => RunModelingTest(Session.RunObjGltfExchangeTest)));
            items.Add(MenuItem(
                Local("Mesh Generation Test", "网格生成测试"),
                () => RunModelingTest(Session.RunMeshGenerationTest)));
            items.Add(MenuItem(
                Local("Viewer Projection Test", "Viewer 投影测试"),
                () => RunModelingTest(Session.RunViewerProjectionTest)));
            samples.ItemsSource = items;
        }

        // The View menu stays as built by BuildViewMenu() — fully flattened,
        // with the "View Settings..." entry at the end.
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
            _viewport.RefreshNativeView();
        });
    }

    private async Task SetZoomSensitivityAsync()
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
        var input = await ParameterDialog.GetValuesAsync(this, Local("Zoom Sensitivity", "缩放灵敏度"), parameters);
        if (!input.Accepted) return;

        var value = new DemoValues(input.Values).Number("value", 1.0);
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

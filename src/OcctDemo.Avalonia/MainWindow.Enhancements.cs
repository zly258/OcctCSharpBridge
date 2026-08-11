using System.Globalization;
using Avalonia.Controls;
using OcctDemo.Common;

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
            items.Add(new Separator());
            items.Add(MenuItem(
                Local("B-Spline Surface Test", "B 样条曲面测试"),
                () => RunModelingTest(Session.RunBSplineSurfaceTest)));
            items.Add(MenuItem(
                Local("Mesh Generation Test", "网格生成测试"),
                () => RunModelingTest(Session.RunMeshGenerationTest)));
            samples.ItemsSource = items;
        }

        var view = rootItems
            .OfType<MenuItem>()
            .FirstOrDefault(item => Equals(item.Header, MenuHeader("Menu.View")));
        if (view is not null)
        {
            var items = (view.ItemsSource as IEnumerable<object>)?.ToList() ?? new List<object>();
            items.Add(new Separator());
            items.Add(AsyncMenuItem(
                Local("Zoom Sensitivity...", "缩放灵敏度..."),
                SetZoomSensitivityAsync));
            view.ItemsSource = items;
        }
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
        var input = await ParameterDialog.GetValuesAsync(this, Local("Zoom Sensitivity", "缩放灵敏度"), parameters);
        if (!input.Accepted) return;

        var value = new DemoValues(input.Values).Number("value", 1.0);
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

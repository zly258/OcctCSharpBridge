using System.Globalization;
using OcctDemo.Common;
using OcctNet;
using Controls = System.Windows.Controls;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private bool _enhancementsWired;

    private void ApplyDemoEnhancements()
    {
        if (!_enhancementsWired)
        {
            _enhancementsWired = true;
            Viewport.ObjectSelectionChanged += (_, args) => Dispatcher.InvokeAsync(() =>
            {
                if (_session is null) return;
                if (args.SelectedObjects.Count > 1) _session.ActiveObject = null;
                ShowSelectionProperties(args.SelectedObjects);
            });
        }

        var samples = MainMenu.Items
            .OfType<Controls.MenuItem>()
            .FirstOrDefault(item => Equals(item.Header, MenuHeader("Menu.Samples")));
        if (samples is not null)
        {
            samples.Items.Add(new Controls.Separator());
            samples.Items.Add(MenuItem(
                Local("B-Spline Surface Test", "B 样条曲面测试"),
                (_, _) => RunModelingTest(Session.RunBSplineSurfaceTest)));
            samples.Items.Add(MenuItem(
                Local("Mesh Generation Test", "网格生成测试"),
                (_, _) => RunModelingTest(Session.RunMeshGenerationTest)));
        }

        var view = MainMenu.Items
            .OfType<Controls.MenuItem>()
            .FirstOrDefault(item => Equals(item.Header, MenuHeader("Menu.View")));
        if (view is not null)
        {
            view.Items.Add(new Controls.Separator());
            view.Items.Add(MenuItem(
                Local("Zoom Sensitivity...", "缩放灵敏度..."),
                (_, _) => SetZoomSensitivity()));
        }
    }

    private void RunModelingTest(Func<DemoCommandResult> test)
    {
        ExecuteSafe(() =>
        {
            var result = test();
            CommandStatus.Text = result.Message;
            Log(result.Message);
            if (!string.IsNullOrWhiteSpace(result.AnalysisText)) Log(result.AnalysisText);
            RefreshObjectTree();
        });
    }

    private void SetZoomSensitivity()
    {
        var current = DemoViewportSettings.GetZoomSensitivity(Viewport);
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
            if (!DemoViewportSettings.TrySetZoomSensitivity(Viewport, value))
            {
                throw new InvalidOperationException(Local(
                    "The Binary SDK does not expose ZoomSensitivity yet. Publish main and sync the demo distribution first.",
                    "当前 Binary SDK 尚未提供 ZoomSensitivity。请先发布 main 并同步 demo 的二进制分发。"));
            }
            var applied = DemoViewportSettings.GetZoomSensitivity(Viewport);
            var message = Local($"Zoom sensitivity: {applied:0.##}×", $"缩放灵敏度：{applied:0.##}×");
            CommandStatus.Text = message;
            Log(message);
        });
    }
}

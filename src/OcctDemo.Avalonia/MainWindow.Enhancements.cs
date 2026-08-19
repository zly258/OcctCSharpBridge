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
            items.Add(new Separator());
            items.Add(MenuItem(
                Local("B-Spline Surface Test", "B 样条曲面测试"),
                () => RunModelingTest(Session.RunBSplineSurfaceTest)));
            items.Add(MenuItem(
                Local("Mesh Generation Test", "网格生成测试"),
                () => RunModelingTest(Session.RunMeshGenerationTest)));
            items.Add(MenuItem(
                Local("Viewer Projection Test", "Viewer 投影测试"),
                () => RunModelingTest(Session.RunViewerProjectionTest)));
            samples.ItemsSource = items;
        }

        var view = rootItems
            .OfType<MenuItem>()
            .FirstOrDefault(item => Equals(item.Header, MenuHeader("Menu.View")));
        if (view is not null)
        {
            RebuildCompactViewMenu(view);
        }
    }

    private void RebuildCompactViewMenu(MenuItem view)
    {
        view.ItemsSource = new object[]
        {
            BuildCompactStandardViewsMenu(),
            BuildCompactDisplayStyleMenu(),
            new Separator(),
            MenuItem(Local("View Settings...", "视图设置..."), ShowAdvancedViewSettingsWindow)
        };
    }

    private MenuItem BuildCompactStandardViewsMenu() => Menu(
        DemoLocalization.Text("Menu.StandardViews"),
        MenuItem(DemoLocalization.Text("Menu.Front"), () => Session.Engine.SetView(OcctViewOrientation.Front)),
        MenuItem(DemoLocalization.Text("Menu.Back"), () => Session.Engine.SetView(OcctViewOrientation.Back)),
        MenuItem(DemoLocalization.Text("Menu.Left"), () => Session.Engine.SetView(OcctViewOrientation.Left)),
        MenuItem(DemoLocalization.Text("Menu.Right"), () => Session.Engine.SetView(OcctViewOrientation.Right)),
        MenuItem(DemoLocalization.Text("Menu.Top"), () => Session.Engine.SetView(OcctViewOrientation.Top)),
        MenuItem(DemoLocalization.Text("Menu.Bottom"), () => Session.Engine.SetView(OcctViewOrientation.Bottom)),
        new Separator(),
        MenuItem(DemoLocalization.Text("Menu.Isometric"), () => Session.Engine.SetView(OcctViewOrientation.Isometric)),
        MenuItem(DemoLocalization.Text("Menu.NorthEast"), () => Session.SetIsoView(DemoIsoView.NorthEast)),
        MenuItem(DemoLocalization.Text("Menu.NorthWest"), () => Session.SetIsoView(DemoIsoView.NorthWest)),
        MenuItem(DemoLocalization.Text("Menu.SouthEast"), () => Session.SetIsoView(DemoIsoView.SouthEast)),
        MenuItem(DemoLocalization.Text("Menu.SouthWest"), () => Session.SetIsoView(DemoIsoView.SouthWest)));

    private MenuItem BuildCompactDisplayStyleMenu() => Menu(
        Local("Display Style", "显示样式"),
        CheckMenuItem(DemoLocalization.Text("Menu.Shaded"), _displayMode == OcctDisplayMode.Shaded, _ => SetDisplayStyle(OcctDisplayMode.Shaded), radio: true, groupName: "display-style"),
        CheckMenuItem(DemoLocalization.Text("Menu.Wireframe"), _displayMode == OcctDisplayMode.Wireframe, _ => SetDisplayStyle(OcctDisplayMode.Wireframe), radio: true, groupName: "display-style"));

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

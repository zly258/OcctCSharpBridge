using System.Globalization;
using OcctDemo.Common;
using OcctNet;
using Controls = System.Windows.Controls;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private void ApplyDemoEnhancements()
    {
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
            samples.Items.Add(MenuItem(
                Local("Viewer Projection Test", "Viewer 投影测试"),
                (_, _) => RunModelingTest(Session.RunViewerProjectionTest)));
        }

        var view = MainMenu.Items
            .OfType<Controls.MenuItem>()
            .FirstOrDefault(item => Equals(item.Header, MenuHeader("Menu.View")));
        if (view is not null)
        {
            RebuildCompactViewMenu(view);
        }
    }

    private void RebuildCompactViewMenu(Controls.MenuItem view)
    {
        view.Items.Clear();
        view.Items.Add(BuildCompactStandardViewsMenu());
        view.Items.Add(BuildCompactDisplayStyleMenu());
        view.Items.Add(new Controls.Separator());
        view.Items.Add(MenuItem(
            Local("View Settings...", "视图设置..."),
            (_, _) => ShowAdvancedViewSettingsWindow()));
    }

    private Controls.MenuItem BuildCompactStandardViewsMenu()
    {
        var standard = Menu(DemoLocalization.Text("Menu.StandardViews"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        standard.Items.Add(new Controls.Separator());
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(DemoIsoView.NorthEast)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(DemoIsoView.NorthWest)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(DemoIsoView.SouthEast)));
        standard.Items.Add(MenuItem(DemoLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(DemoIsoView.SouthWest)));
        return standard;
    }

    private Controls.MenuItem BuildCompactDisplayStyleMenu()
    {
        var display = Menu(Local("Display Style", "显示样式"));
        var shaded = MenuItem(DemoLocalization.Text("Menu.Shaded"), (_, _) => SetDisplayStyle(OcctDisplayMode.Shaded));
        shaded.IsCheckable = true;
        shaded.IsChecked = _displayMode == OcctDisplayMode.Shaded;
        display.Items.Add(shaded);
        var wireframe = MenuItem(DemoLocalization.Text("Menu.Wireframe"), (_, _) => SetDisplayStyle(OcctDisplayMode.Wireframe));
        wireframe.IsCheckable = true;
        wireframe.IsChecked = _displayMode == OcctDisplayMode.Wireframe;
        display.Items.Add(wireframe);
        return display;
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
        var parameters = new[]
        {
            new DemoParameterDefinition(
                "value",
                Local("Zoom Sensitivity", "缩放灵敏度"),
                DemoParameterKind.Number,
                Viewport.ZoomSensitivity.ToString("0.##", CultureInfo.InvariantCulture),
                "×")
        };
        if (!ParameterDialog.TryGetValues(this, Local("Zoom Sensitivity", "缩放灵敏度"), parameters, out var raw)) return;

        var value = new DemoValues(raw).Number("value", 1.0);
        ExecuteSafe(() =>
        {
            Viewport.ZoomSensitivity = value;
            var message = Local(
                $"Zoom sensitivity: {Viewport.ZoomSensitivity:0.##}×",
                $"缩放灵敏度：{Viewport.ZoomSensitivity:0.##}×");
            CommandStatus.Text = message;
            Log(message);
        });
    }
}

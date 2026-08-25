using System.Globalization;
using OcctDemo.Common;
using OcctNet;
using Controls = System.Windows.Controls;

namespace OcctDemo.Wpf;

public partial class MainWindow
{
    private void ApplyDemoEnhancements()
    {
        // Tests and exchange items are now built directly in BuildMenus().
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

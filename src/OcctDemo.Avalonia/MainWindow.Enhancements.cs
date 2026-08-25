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
        // Tests and exchange items are now built directly in BuildMenus().
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

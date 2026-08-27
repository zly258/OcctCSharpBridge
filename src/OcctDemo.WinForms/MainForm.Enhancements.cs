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
        // Tests and exchange items are now built directly in BuildMenus().
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
        if (selectedObjects.OfType<OcctShape>().Count() >= 2)
            _propertyGrid.Rows.Add("▶ " + DemoLocalization.CommandText(DemoCommandId.AnalyzeDistance), Local("Click to run", "点击执行"));
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

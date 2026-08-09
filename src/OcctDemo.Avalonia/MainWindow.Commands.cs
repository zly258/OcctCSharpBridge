using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using AvaloniaBrushes = Avalonia.Media.Brushes;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaFontFamily = Avalonia.Media.FontFamily;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaloniaOrientation = Avalonia.Layout.Orientation;
using AvaloniaToolTip = Avalonia.Controls.ToolTip;
using Forms = System.Windows.Forms;
using Button = Avalonia.Controls.Button;
using CheckBox = Avalonia.Controls.CheckBox;
using ContextMenu = Avalonia.Controls.ContextMenu;
using MenuItem = Avalonia.Controls.MenuItem;
using ComboBox = Avalonia.Controls.ComboBox;
using Control = Avalonia.Controls.Control;
using GroupBox = Avalonia.Controls.GroupBox;
using TextBox = Avalonia.Controls.TextBox;
using TreeView = Avalonia.Controls.TreeView;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private async Task RunCommandAsync(DemoCommandId id)
    {
        if (_session is null) return;

        var availability = _session.GetCommandAvailability(id);
        if (!availability.CanExecute)
        {
            ReportCommandPrecondition(availability.Message);
            return;
        }

        var definition = DemoLocalization.Localize(DemoCommandCatalog.Get(id));
        var input = await ParameterDialog.GetValuesAsync(this, definition.Text, definition.Parameters);
        if (!input.Accepted) return;

        ExecuteSafe(() =>
        {
            var result = Session.Execute(id, input.Values);
            if (!string.IsNullOrWhiteSpace(result.AnalysisText))
            {
                Log(result.AnalysisText);
                Forms.MessageBox.Show(result.AnalysisText, definition.Text, Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
            }
            RefreshObjectTree();
        });
    }

    private void ReportCommandPrecondition(string message)
    {
        _commandStatus.Text = message;
        Log(message);
        System.Media.SystemSounds.Asterisk.Play();
        _viewport.Focus();
    }

    private void NewDocument()
    {
        if (!ConfirmDiscardChanges()) return;
        ExecuteSafe(Session.NewDocument);
    }

    private void OpenDocument()
    {
        if (!ConfirmDiscardChanges()) return;
        using var dialog = new Forms.OpenFileDialog
        {
            Filter = CadFileFilter(),
            Title = DemoLocalization.Text("Dialog.OpenTitle"),
            Multiselect = false
        };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() => Session.Open(dialog.FileName));
    }

    private void ImportDocument()
    {
        using var dialog = new Forms.OpenFileDialog
        {
            Filter = CadFileFilter(),
            Title = DemoLocalization.Text("Dialog.ImportTitle"),
            Multiselect = true
        };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            foreach (var file in dialog.FileNames) Session.Import(file);
        });
    }

    private bool SaveDocument(bool saveAs)
    {
        var file = Session.CurrentFilePath;
        if (saveAs || string.IsNullOrWhiteSpace(file))
        {
            using var dialog = new Forms.SaveFileDialog
            {
                Filter = SaveFileFilter(),
                Title = DemoLocalization.Text("Dialog.SaveTitle"),
                DefaultExt = "step",
                AddExtension = true
            };
            if (dialog.ShowDialog() != Forms.DialogResult.OK) return false;
            file = dialog.FileName;
        }

        var succeeded = false;
        ExecuteSafe(() =>
        {
            Session.SaveAll(file!);
            succeeded = true;
        });
        return succeeded;
    }

    private void ExportSelected()
    {
        using var dialog = new Forms.SaveFileDialog
        {
            Filter = SaveFileFilter(),
            Title = DemoLocalization.Text("Dialog.ExportTitle"),
            DefaultExt = "step",
            AddExtension = true
        };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() => Session.ExportSelected(dialog.FileName));
    }

    private void ExportViewImage()
    {
        using var dialog = new Forms.SaveFileDialog
        {
            Filter = Local("PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp", "PNG 图片|*.png|JPEG 图片|*.jpg;*.jpeg|BMP 图片|*.bmp"),
            Title = DemoLocalization.Text("Dialog.ExportImageTitle"),
            DefaultExt = "png",
            AddExtension = true
        };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            Session.Engine.DumpView(dialog.FileName);
            Log(Local($"View image exported: {dialog.FileName}", $"已导出视图图片：{dialog.FileName}"));
        });
    }

    private async Task SetPerspectiveFovAsync()
    {
        var parameters = new[]
        {
            new DemoParameterDefinition("fov", Local("Vertical Field of View", "垂直视场角"), DemoParameterKind.Number, "45", "°")
        };
        var input = await ParameterDialog.GetValuesAsync(this, DemoLocalization.Text("Menu.PerspectiveFov"), parameters);
        if (!input.Accepted) return;
        ExecuteSafe(() => Session.Engine.SetPerspectiveFieldOfView(new DemoValues(input.Values).Number("fov", 45)));
    }

    private async Task SetDisplayPrecisionAsync()
    {
        var parameters = new[]
        {
            new DemoParameterDefinition("coefficient", Local("Deviation Coefficient", "离散偏差系数"), DemoParameterKind.Number, "0.001"),
            new DemoParameterDefinition("angle", Local("Angular Deflection", "角度偏差"), DemoParameterKind.Number, "12", "°"),
            new DemoParameterDefinition("existing", Local("Apply to Existing Objects", "应用到现有对象"), DemoParameterKind.Boolean, "true")
        };
        var input = await ParameterDialog.GetValuesAsync(this, DemoLocalization.Text("Menu.DisplayPrecision"), parameters);
        if (!input.Accepted) return;
        var values = new DemoValues(input.Values);
        ExecuteSafe(() => Session.Engine.SetDisplayPrecision(values.Number("coefficient"), values.Number("angle"), values.Boolean("existing", true)));
    }

    private async Task SetAdvancedLightingAsync()
    {
        static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
        var parameters = new[]
        {
            new DemoParameterDefinition("ambient", Local("Ambient Intensity", "环境光强度"), DemoParameterKind.Number, Number(_lightingSettings.AmbientIntensity)),
            new DemoParameterDefinition("cameraEnabled", Local("Camera Light", "相机直射光"), DemoParameterKind.Boolean, _lightingSettings.CameraLight.Enabled.ToString()),
            new DemoParameterDefinition("camera", Local("Camera Light Intensity", "相机直射光强度"), DemoParameterKind.Number, Number(_lightingSettings.CameraLight.Intensity)),
            new DemoParameterDefinition("sunEnabled", Local("Sun Light", "太阳光"), DemoParameterKind.Boolean, _lightingSettings.SunLight.Enabled.ToString()),
            new DemoParameterDefinition("sun", Local("Sun Intensity", "太阳光强度"), DemoParameterKind.Number, Number(_lightingSettings.SunLight.Intensity)),
            new DemoParameterDefinition("sunX", Local("Sun Direction X", "太阳光方向 X"), DemoParameterKind.Number, Number(_lightingSettings.SunLight.Direction.X)),
            new DemoParameterDefinition("sunY", Local("Sun Direction Y", "太阳光方向 Y"), DemoParameterKind.Number, Number(_lightingSettings.SunLight.Direction.Y)),
            new DemoParameterDefinition("sunZ", Local("Sun Direction Z", "太阳光方向 Z"), DemoParameterKind.Number, Number(_lightingSettings.SunLight.Direction.Z)),
            new DemoParameterDefinition("fillEnabled", Local("Fill Light", "补光"), DemoParameterKind.Boolean, _lightingSettings.FillLight.Enabled.ToString()),
            new DemoParameterDefinition("fill", Local("Fill Intensity", "补光强度"), DemoParameterKind.Number, Number(_lightingSettings.FillLight.Intensity)),
            new DemoParameterDefinition("fillX", Local("Fill Direction X", "补光方向 X"), DemoParameterKind.Number, Number(_lightingSettings.FillLight.Direction.X)),
            new DemoParameterDefinition("fillY", Local("Fill Direction Y", "补光方向 Y"), DemoParameterKind.Number, Number(_lightingSettings.FillLight.Direction.Y)),
            new DemoParameterDefinition("fillZ", Local("Fill Direction Z", "补光方向 Z"), DemoParameterKind.Number, Number(_lightingSettings.FillLight.Direction.Z))
        };
        var input = await ParameterDialog.GetValuesAsync(this, Local("Custom Lighting", "自定义灯光"), parameters);
        if (!input.Accepted) return;
        var values = new DemoValues(input.Values);
        var settings = _lightingSettings with
        {
            AmbientIntensity = values.Number("ambient"),
            CameraLight = _lightingSettings.CameraLight with
            {
                Enabled = values.Boolean("cameraEnabled", true),
                Intensity = values.Number("camera")
            },
            SunLight = _lightingSettings.SunLight with
            {
                Enabled = values.Boolean("sunEnabled", true),
                Intensity = values.Number("sun"),
                Direction = values.Vector("sunX", "sunY", "sunZ")
            },
            FillLight = _lightingSettings.FillLight with
            {
                Enabled = values.Boolean("fillEnabled", true),
                Intensity = values.Number("fill"),
                Direction = values.Vector("fillX", "fillY", "fillZ")
            }
        };
        ExecuteSafe(() =>
        {
            Session.Engine.SetSceneLighting(settings);
            _lightingSettings = settings;
            Log(Local("Custom lighting applied.", "已应用自定义灯光。"));
        });
    }

    private void ApplyLightingPreset(OcctLightingPreset preset)
    {
        ExecuteSafe(() =>
        {
            _lightingSettings = OcctLightingPresets.Create(preset);
            Session.Engine.SetSceneLighting(_lightingSettings);
            Log($"{Local("Lighting", "灯光")}: {LightingPresetName(preset)}");
        });
    }

    private void ApplyDepthBias(DemoDepthBiasPreset preset)
    {
        ExecuteSafe(() =>
        {
            var count = Session.ApplyDepthBiasToSelection(preset);
            var message = count == 0
                ? DemoLocalization.Text("Status.DepthBiasNoShape")
                : DemoLocalization.Text("Status.DepthBiasApplied", count);
            _commandStatus.Text = message;
            Log(message);
        });
    }

    private void SetSelectionHighlightColor()
    {
        using var dialog = new Forms.ColorDialog { Color = _selectionHighlightColor, FullOpen = true };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _selectionHighlightColor = dialog.Color;
            Session.Engine.SetSelectionHighlightColor(dialog.Color);
        });
    }

    private void SetHoverHighlightColor()
    {
        using var dialog = new Forms.ColorDialog { Color = _hoverHighlightColor, FullOpen = true };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _hoverHighlightColor = dialog.Color;
            Session.Engine.SetHoverHighlightColor(dialog.Color);
        });
    }

    private async Task SetSelectionToleranceAsync()
    {
        var parameters = new[]
        {
            new DemoParameterDefinition("pixels", Local("Aperture Size", "像素容差"), DemoParameterKind.Integer, "4", "px")
        };
        var input = await ParameterDialog.GetValuesAsync(this, DemoLocalization.Text("Menu.SelectionTolerance"), parameters);
        if (!input.Accepted) return;
        ExecuteSafe(() => Session.Engine.SetSelectionTolerance(new DemoValues(input.Values).Integer("pixels", 4)));
    }

    private void SetBackgroundColor()
    {
        using var dialog = new Forms.ColorDialog { Color = DrawingColor.White, FullOpen = true };
        if (dialog.ShowDialog() == Forms.DialogResult.OK)
            ExecuteSafe(() => Session.Engine.SetBackground(dialog.Color));
    }

    private void SetSelectionMode(OcctSelectionMode mode)
    {
        if (_session is null) return;
        ExecuteSafe(() =>
        {
            Session.Engine.SetSelectionMode(mode);
            if (_selectionCombo is not null && _selectionCombo.SelectedIndex != (int)mode)
                _selectionCombo.SelectedIndex = (int)mode;
            _commandStatus.Text = Local($"Selection filter: {SelectionModeName(mode)}", $"选择过滤器：{SelectionModeName(mode)}");
        });
    }

    private void SetObjectColor(IOcctObject value)
    {
        using var dialog = new Forms.ColorDialog { Color = DrawingColor.SteelBlue, FullOpen = true };
        if (dialog.ShowDialog() == Forms.DialogResult.OK)
            ExecuteSafe(() => Session.Engine.SetColor(value, dialog.Color));
    }

    private async Task SetObjectMaterialAsync(IOcctObject value)
    {
        if (value.Kind != OcctObjectKind.Shape) return;
        var options = Enum.GetValues<OcctMaterial>().Select(MaterialDisplayName).ToArray();
        var parameters = new[]
        {
            new DemoParameterDefinition("material", Local("Material", "材质"), DemoParameterKind.Choice,
                MaterialDisplayName(OcctMaterial.Steel), null, options)
        };
        var input = await ParameterDialog.GetValuesAsync(this, Local("Object Material", "对象材质"), parameters);
        if (!input.Accepted) return;
        var name = new DemoValues(input.Values).Text("material");
        var material = Enum.GetValues<OcctMaterial>().First(item => MaterialDisplayName(item) == name);
        ExecuteSafe(() => Session.Engine.SetMaterial(value, material));
    }

    private bool ConfirmDiscardChanges()
    {
        if (_session?.IsModified != true) return true;
        var answer = Forms.MessageBox.Show(
            DemoLocalization.Text("Dialog.ConfirmDiscard"),
            DemoLocalization.Text("Dialog.ConfirmDiscardTitle"),
            Forms.MessageBoxButtons.YesNoCancel,
            Forms.MessageBoxIcon.Question);
        if (answer == Forms.DialogResult.Cancel) return false;
        if (answer == Forms.DialogResult.Yes) return SaveDocument(false);
        return true;
    }

    private void Undo()
    {
        if (_session is null) return;
        ExecuteSafe(() =>
        {
            Session.Undo();
            _viewport.RaiseSelectionChanged();
            RefreshObjectTree();
        });
    }

    private void Redo()
    {
        if (_session is null) return;
        ExecuteSafe(() =>
        {
            Session.Redo();
            _viewport.RaiseSelectionChanged();
            RefreshObjectTree();
        });
    }

    private void UpdateHistoryUi()
    {
        var canUndo = _session?.CanUndo == true;
        var canRedo = _session?.CanRedo == true;
        if (_undoMenuItem is not null)
        {
            _undoMenuItem.IsEnabled = canUndo;
            _undoMenuItem.Header = canUndo
                ? DemoLocalization.Text("History.Undo", _session!.UndoDescription!)
                : DemoLocalization.Text("Menu.Undo");
        }
        if (_redoMenuItem is not null)
        {
            _redoMenuItem.IsEnabled = canRedo;
            _redoMenuItem.Header = canRedo
                ? DemoLocalization.Text("History.Redo", _session!.RedoDescription!)
                : DemoLocalization.Text("Menu.Redo");
        }
        if (_undoButton is not null) _undoButton.IsEnabled = canUndo;
        if (_redoButton is not null) _redoButton.IsEnabled = canRedo;
    }

    private void MainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!ConfirmDiscardChanges()) e.Cancel = true;
    }

    private async void MainWindowKeyDown(object? sender, KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers;
        if (modifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.Z)
        {
            Undo();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.Y)
        {
            Redo();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.N)
        {
            NewDocument();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.O)
        {
            OpenDocument();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.S)
        {
            SaveDocument(modifiers.HasFlag(KeyModifiers.Shift));
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            await RunCommandAsync(DemoCommandId.Delete);
            e.Handled = true;
        }
        else if (e.Key == Key.F && _session is not null)
        {
            Session.Engine.FitAll();
            e.Handled = true;
        }
        else if (e.Key == Key.D0 && _session is not null)
        {
            Session.Engine.SetView(OcctViewOrientation.Isometric);
            e.Handled = true;
        }
        else if (e.Key == Key.D1 && _session is not null)
        {
            Session.Engine.SetView(OcctViewOrientation.Front);
            e.Handled = true;
        }
        else if (e.Key == Key.D2 && _session is not null)
        {
            Session.Engine.SetView(OcctViewOrientation.Left);
            e.Handled = true;
        }
        else if (e.Key == Key.D3 && _session is not null)
        {
            Session.Engine.SetView(OcctViewOrientation.Top);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _session is not null)
        {
            Session.Engine.ClearSelection();
            _viewport.RaiseSelectionChanged();
            e.Handled = true;
        }
    }
}

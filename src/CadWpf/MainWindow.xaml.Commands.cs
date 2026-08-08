using System.Globalization;
using CadCommon;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using Input = System.Windows.Input;

namespace CadWpf;

public partial class MainWindow
{
    private void ApplyDepthBias(CadDepthBiasPreset preset)
    {
        ExecuteSafe(() =>
        {
            var count = Session.ApplyDepthBiasToSelection(preset);
            var message = count == 0
                ? CadLocalization.Text("Status.DepthBiasNoShape")
                : CadLocalization.Text("Status.DepthBiasApplied", count);
            CommandStatus.Text = message;
            Log(message);
        });
    }

    private void ReportCommandPrecondition(string message)
    {
        CommandStatus.Text = message;
        Log(message);
        System.Media.SystemSounds.Asterisk.Play();
        Viewport.FocusViewport();
    }

    private void RunCommand(CadCommandId id)
    {
        if (_session is null) return;

        var availability = _session.GetCommandAvailability(id);
        if (!availability.CanExecute)
        {
            ReportCommandPrecondition(availability.Message);
            return;
        }

        var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));
        if (!ParameterDialog.TryGetValues(this, definition.Text, definition.Parameters, out var values)) return;
        ExecuteSafe(() =>
        {
            var result = Session.Execute(id, values);
            if (!string.IsNullOrWhiteSpace(result.AnalysisText))
            {
                Log(result.AnalysisText);
                System.Windows.MessageBox.Show(this, result.AnalysisText, definition.Text,
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            RefreshObjectTree();
        });
    }

    private void NewDocument()
    {
        if (!ConfirmDiscardChanges()) return;
        ExecuteSafe(Session.NewDocument);
    }

    private void OpenDocument()
    {
        if (!ConfirmDiscardChanges()) return;
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = CadFileFilter(),
            Title = CadLocalization.Text("Dialog.OpenTitle"),
            Multiselect = false
        };
        if (dialog.ShowDialog(this) != true) return;
        ExecuteSafe(() => Session.Open(dialog.FileName));
    }

    private void ImportDocument()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = CadFileFilter(),
            Title = CadLocalization.Text("Dialog.ImportTitle"),
            Multiselect = true
        };
        if (dialog.ShowDialog(this) != true) return;
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
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = SaveFileFilter(),
                Title = CadLocalization.Text("Dialog.SaveTitle"),
                DefaultExt = ".step",
                AddExtension = true
            };
            if (dialog.ShowDialog(this) != true) return false;
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
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = SaveFileFilter(),
            Title = CadLocalization.Text("Dialog.ExportTitle"),
            DefaultExt = ".step",
            AddExtension = true
        };
        if (dialog.ShowDialog(this) != true) return;
        ExecuteSafe(() => Session.ExportSelected(dialog.FileName));
    }

    private void ExportViewImage()
    {
        var filter = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
            ? "PNG 图片|*.png|JPEG 图片|*.jpg;*.jpeg|BMP 图片|*.bmp"
            : "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp";
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = filter,
            Title = CadLocalization.Text("Dialog.ExportImageTitle"),
            DefaultExt = ".png",
            AddExtension = true
        };
        if (dialog.ShowDialog(this) != true) return;
        ExecuteSafe(() =>
        {
            Session.Engine.DumpView(dialog.FileName);
            Log(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
                ? $"已导出视图图片：{dialog.FileName}"
                : $"View image exported: {dialog.FileName}");
        });
    }

    private void SetPerspectiveFov()
    {
        var parameters = new[]
        {
            new CadParameterDefinition("fov",
                CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "垂直视场角" : "Vertical Field of View",
                CadParameterKind.Number, "45", "°")
        };
        if (!ParameterDialog.TryGetValues(this, CadLocalization.Text("Menu.PerspectiveFov"), parameters, out var raw)) return;
        ExecuteSafe(() => Session.Engine.SetPerspectiveFieldOfView(new CadValues(raw).Number("fov", 45)));
    }

    private void SetDisplayPrecision()
    {
        var parameters = new[]
        {
            new CadParameterDefinition("coefficient", Local("Deviation Coefficient", "离散偏差系数"), CadParameterKind.Number, "0.001"),
            new CadParameterDefinition("angle", Local("Angular Deflection", "角度偏差"), CadParameterKind.Number, "12", "°"),
            new CadParameterDefinition("existing", Local("Apply to Existing Objects", "应用到现有对象"), CadParameterKind.Boolean, "true")
        };
        if (!ParameterDialog.TryGetValues(this, CadLocalization.Text("Menu.DisplayPrecision"), parameters, out var raw)) return;
        var values = new CadValues(raw);
        ExecuteSafe(() => Session.Engine.SetDisplayPrecision(values.Number("coefficient"), values.Number("angle"), values.Boolean("existing", true)));
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

    private void SetAdvancedLighting()
    {
        static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
        var parameters = new[]
        {
            new CadParameterDefinition("ambient", Local("Ambient Intensity", "环境光强度"), CadParameterKind.Number, Number(_lightingSettings.AmbientIntensity)),
            new CadParameterDefinition("cameraEnabled", Local("Camera Light", "相机直射光"), CadParameterKind.Boolean, _lightingSettings.CameraLight.Enabled.ToString()),
            new CadParameterDefinition("camera", Local("Camera Light Intensity", "相机直射光强度"), CadParameterKind.Number, Number(_lightingSettings.CameraLight.Intensity)),
            new CadParameterDefinition("sunEnabled", Local("Sun Light", "太阳光"), CadParameterKind.Boolean, _lightingSettings.SunLight.Enabled.ToString()),
            new CadParameterDefinition("sun", Local("Sun Intensity", "太阳光强度"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Intensity)),
            new CadParameterDefinition("sunX", Local("Sun Direction X", "太阳光方向 X"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Direction.X)),
            new CadParameterDefinition("sunY", Local("Sun Direction Y", "太阳光方向 Y"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Direction.Y)),
            new CadParameterDefinition("sunZ", Local("Sun Direction Z", "太阳光方向 Z"), CadParameterKind.Number, Number(_lightingSettings.SunLight.Direction.Z)),
            new CadParameterDefinition("fillEnabled", Local("Fill Light", "补光"), CadParameterKind.Boolean, _lightingSettings.FillLight.Enabled.ToString()),
            new CadParameterDefinition("fill", Local("Fill Intensity", "补光强度"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Intensity)),
            new CadParameterDefinition("fillX", Local("Fill Direction X", "补光方向 X"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Direction.X)),
            new CadParameterDefinition("fillY", Local("Fill Direction Y", "补光方向 Y"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Direction.Y)),
            new CadParameterDefinition("fillZ", Local("Fill Direction Z", "补光方向 Z"), CadParameterKind.Number, Number(_lightingSettings.FillLight.Direction.Z))
        };
        if (!ParameterDialog.TryGetValues(this, Local("Custom Lighting", "自定义灯光"), parameters, out var raw)) return;
        var values = new CadValues(raw);
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

    private void SetSelectionHighlightColor()
    {
        using var dialog = new System.Windows.Forms.ColorDialog { Color = _selectionHighlightColor, FullOpen = true };
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _selectionHighlightColor = dialog.Color;
            Session.Engine.SetSelectionHighlightColor(dialog.Color);
        });
    }

    private void SetHoverHighlightColor()
    {
        using var dialog = new System.Windows.Forms.ColorDialog { Color = _hoverHighlightColor, FullOpen = true };
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _hoverHighlightColor = dialog.Color;
            Session.Engine.SetHoverHighlightColor(dialog.Color);
        });
    }

    private void SetSelectionTolerance()
    {
        var parameters = new[]
        {
            new CadParameterDefinition("pixels", Local("Aperture Size", "像素容差"), CadParameterKind.Integer, "4", "px")
        };
        if (!ParameterDialog.TryGetValues(this, CadLocalization.Text("Menu.SelectionTolerance"), parameters, out var raw)) return;
        ExecuteSafe(() => Session.Engine.SetSelectionTolerance(new CadValues(raw).Integer("pixels", 4)));
    }

    private void SetBackgroundColor()
    {
        using var dialog = new System.Windows.Forms.ColorDialog { Color = DrawingColor.White, FullOpen = true };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ExecuteSafe(() => Session.Engine.SetBackground(dialog.Color));
        }
    }

    private void SetSelectionMode(OcctSelectionMode mode)
    {
        if (_session is null) return;
        ExecuteSafe(() =>
        {
            Session.Engine.SetSelectionMode(mode);
            if (_selectionCombo is not null && _selectionCombo.SelectedIndex != (int)mode)
            {
                _selectionCombo.SelectedIndex = (int)mode;
            }
            CommandStatus.Text = Local($"Selection filter: {SelectionModeName(mode)}", $"选择过滤器：{SelectionModeName(mode)}");
        });
    }

    private void SetObjectColor(IOcctObject value)
    {
        using var dialog = new System.Windows.Forms.ColorDialog { Color = DrawingColor.SteelBlue, FullOpen = true };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ExecuteSafe(() => Session.Engine.SetColor(value, dialog.Color));
        }
    }

    private void SetObjectMaterial(IOcctObject value)
    {
        if (value.Kind != OcctObjectKind.Shape) return;
        var options = Enum.GetValues<OcctMaterial>().Select(MaterialDisplayName).ToArray();
        var parameters = new[]
        {
            new CadParameterDefinition("material", Local("Material", "材质"), CadParameterKind.Choice,
                MaterialDisplayName(OcctMaterial.Steel), null, options)
        };
        if (!ParameterDialog.TryGetValues(this, Local("Object Material", "对象材质"), parameters, out var raw)) return;
        var name = new CadValues(raw).Text("material");
        var material = Enum.GetValues<OcctMaterial>().First(item => MaterialDisplayName(item) == name);
        ExecuteSafe(() => Session.Engine.SetMaterial(value, material));
    }

    private bool ConfirmDiscardChanges()
    {
        if (_session?.IsModified != true) return true;
        var answer = System.Windows.MessageBox.Show(this,
            CadLocalization.Text("Dialog.ConfirmDiscard"),
            CadLocalization.Text("Dialog.ConfirmDiscardTitle"),
            System.Windows.MessageBoxButton.YesNoCancel,
            System.Windows.MessageBoxImage.Question);
        if (answer == System.Windows.MessageBoxResult.Cancel) return false;
        if (answer == System.Windows.MessageBoxResult.Yes) return SaveDocument(false);
        return true;
    }

    private void Undo()
    {
        if (_session is null) return;
        ExecuteSafe(() =>
        {
            Session.Undo();
            Viewport.RaiseSelectionChanged();
            RefreshObjectTree();
        });
    }

    private void Redo()
    {
        if (_session is null) return;
        ExecuteSafe(() =>
        {
            Session.Redo();
            Viewport.RaiseSelectionChanged();
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
                ? CadLocalization.Text("History.Undo", _session!.UndoDescription!)
                : CadLocalization.Text("Menu.Undo");
        }
        if (_redoMenuItem is not null)
        {
            _redoMenuItem.IsEnabled = canRedo;
            _redoMenuItem.Header = canRedo
                ? CadLocalization.Text("History.Redo", _session!.RedoDescription!)
                : CadLocalization.Text("Menu.Redo");
        }
        if (_undoButton is not null) _undoButton.IsEnabled = canUndo;
        if (_redoButton is not null) _redoButton.IsEnabled = canRedo;
    }

    private void MainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!ConfirmDiscardChanges()) e.Cancel = true;
    }

    private void MainWindowPreviewKeyDown(object sender, Input.KeyEventArgs e)
    {
        var modifiers = Input.Keyboard.Modifiers;
        if (modifiers.HasFlag(Input.ModifierKeys.Control) && e.Key == Input.Key.Z)
        {
            Undo();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(Input.ModifierKeys.Control) && e.Key == Input.Key.Y)
        {
            Redo();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(Input.ModifierKeys.Control) && e.Key == Input.Key.N)
        {
            NewDocument();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(Input.ModifierKeys.Control) && e.Key == Input.Key.O)
        {
            OpenDocument();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(Input.ModifierKeys.Control) && e.Key == Input.Key.S)
        {
            SaveDocument(modifiers.HasFlag(Input.ModifierKeys.Shift));
            e.Handled = true;
        }
        else if (e.Key == Input.Key.Delete)
        {
            RunCommand(CadCommandId.Delete);
            e.Handled = true;
        }
        else if (e.Key == Input.Key.F && _session is not null)
        {
            Session.Engine.FitAll();
            e.Handled = true;
        }
        else if (e.Key == Input.Key.D0 && _session is not null)
        {
            Session.Engine.SetView(OcctViewOrientation.Isometric);
            e.Handled = true;
        }
        else if (e.Key == Input.Key.D1 && _session is not null)
        {
            Session.Engine.SetView(OcctViewOrientation.Front);
            e.Handled = true;
        }
        else if (e.Key == Input.Key.D2 && _session is not null)
        {
            Session.Engine.SetView(OcctViewOrientation.Left);
            e.Handled = true;
        }
        else if (e.Key == Input.Key.D3 && _session is not null)
        {
            Session.Engine.SetView(OcctViewOrientation.Top);
            e.Handled = true;
        }
        else if (e.Key == Input.Key.Escape && _session is not null)
        {
            Session.Engine.ClearSelection();
            Viewport.RaiseSelectionChanged();
            e.Handled = true;
        }
    }
}

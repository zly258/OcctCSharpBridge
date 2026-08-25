using System.Globalization;
using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
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

    private void ReportCommandPrecondition(string message)
    {
        _commandStatus.Text = message;
        Log(message);
        System.Media.SystemSounds.Asterisk.Play();
        _viewport.Focus();
    }

    private void RunCommand(DemoCommandId id)
    {
        if (_session is null) return;

        var availability = _session.GetCommandAvailability(id);
        if (!availability.CanExecute)
        {
            ReportCommandPrecondition(availability.Message);
            return;
        }

        var definition = DemoLocalization.Localize(DemoCommandCatalog.Get(id));
        if (!ParameterDialog.TryGetValues(this, definition.Text, definition.Parameters, out var values)) return;
        ExecuteSafe(() =>
        {
            var result = Session.Execute(id, values);
            if (!string.IsNullOrWhiteSpace(result.AnalysisText))
            {
                Log(result.AnalysisText);
                MessageBox.Show(this, result.AnalysisText, definition.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        using var dialog = new OpenFileDialog { Filter = CadFileFilter(), Title = DemoLocalization.Text("Dialog.OpenTitle"), Multiselect = false };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() => Session.Open(dialog.FileName));
    }

    private void ImportDocument()
    {
        using var dialog = new OpenFileDialog { Filter = CadFileFilter(), Title = DemoLocalization.Text("Dialog.ImportTitle"), Multiselect = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() => { foreach (var file in dialog.FileNames) Session.Import(file); });
    }

    private bool SaveDocument(bool saveAs)
    {
        var file = Session.CurrentFilePath;
        if (saveAs || string.IsNullOrWhiteSpace(file))
        {
            using var dialog = new SaveFileDialog { Filter = SaveFileFilter(), Title = DemoLocalization.Text("Dialog.SaveTitle"), DefaultExt = "step", AddExtension = true };
            if (dialog.ShowDialog(this) != DialogResult.OK) return false;
            file = dialog.FileName;
        }
        var succeeded = false;
        ExecuteSafe(() => { Session.SaveAll(file!); succeeded = true; });
        return succeeded;
    }

    private void ExportSelected()
    {
        using var dialog = new SaveFileDialog { Filter = SaveFileFilter(), Title = DemoLocalization.Text("Dialog.ExportTitle"), DefaultExt = "step", AddExtension = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() => Session.ExportSelected(dialog.FileName));
    }

    private void ExportViewImage()
    {
        using var dialog = new SaveFileDialog { Filter = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "PNG 图片|*.png|JPEG 图片|*.jpg;*.jpeg|BMP 图片|*.bmp" : "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp", Title = DemoLocalization.Text("Dialog.ExportImageTitle"), DefaultExt = "png", AddExtension = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() => { Session.Engine.DumpView(dialog.FileName); Log(DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? $"已导出视图图片：{dialog.FileName}" : $"View image exported: {dialog.FileName}"); });
    }

    private void ExportSelectedAs(string format)
    {
        var (filter, ext) = format.ToLowerInvariant() switch
        {
            "obj" => (DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "OBJ 文件|*.obj" : "OBJ Files|*.obj", "obj"),
            "gltf" => (DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "glTF 文件|*.gltf;*.glb" : "glTF Files|*.gltf;*.glb", "gltf"),
            "stl" => (DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "STL 文件|*.stl" : "STL Files|*.stl", "stl"),
            _ => (SaveFileFilter(), "step")
        };
        using var dialog = new SaveFileDialog { Filter = filter, Title = DemoLocalization.Text("Dialog.ExportTitle"), DefaultExt = ext, AddExtension = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() => Session.ExportSelected(dialog.FileName));
    }

    private void SetPerspectiveFov()
    {
        var parameters = new[] { new DemoParameterDefinition("fov", DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "垂直视场角" : "Vertical Field of View", DemoParameterKind.Number, "45", "°") };
        if (!ParameterDialog.TryGetValues(this, DemoLocalization.Text("Menu.PerspectiveFov"), parameters, out var values)) return;
        ExecuteSafe(() => Session.Engine.SetPerspectiveFieldOfView(new DemoValues(values).Number("fov", 45)));
    }

    private void SetDisplayPrecision()
    {
        var parameters = new[]
        {
            new DemoParameterDefinition("coefficient", DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "离散偏差系数" : "Deviation Coefficient", DemoParameterKind.Number, "0.001"),
            new DemoParameterDefinition("angle", DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "角度偏差" : "Angular Deflection", DemoParameterKind.Number, "12", "°"),
            new DemoParameterDefinition("existing", DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "应用到现有对象" : "Apply to Existing Objects", DemoParameterKind.Boolean, "true")
        };
        if (!ParameterDialog.TryGetValues(this, DemoLocalization.Text("Menu.DisplayPrecision"), parameters, out var raw)) return;
        var values = new DemoValues(raw);
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
        if (!ParameterDialog.TryGetValues(this, Local("Custom Lighting", "自定义灯光"), parameters, out var raw)) return;
        var values = new DemoValues(raw);
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
        using var dialog = new ColorDialog { Color = _selectionHighlightColor, FullOpen = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _selectionHighlightColor = dialog.Color;
            Session.Engine.SetSelectionHighlightColor(dialog.Color);
        });
    }

    private void SetHoverHighlightColor()
    {
        using var dialog = new ColorDialog { Color = _hoverHighlightColor, FullOpen = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _hoverHighlightColor = dialog.Color;
            Session.Engine.SetHoverHighlightColor(dialog.Color);
        });
    }

    private void SetSelectionTolerance()
    {
        var parameters = new[] { new DemoParameterDefinition("pixels", DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "像素容差" : "Aperture Size", DemoParameterKind.Integer, "4", "px") };
        if (!ParameterDialog.TryGetValues(this, DemoLocalization.Text("Menu.SelectionTolerance"), parameters, out var raw)) return;
        ExecuteSafe(() => Session.Engine.SetSelectionTolerance(new DemoValues(raw).Integer("pixels", 4)));
    }

    private void SetBackgroundColor()
    {
        using var dialog = new ColorDialog { Color = Color.White, FullOpen = true };
        if (dialog.ShowDialog(this) == DialogResult.OK) ExecuteSafe(() => Session.Engine.SetBackground(dialog.Color));
    }

    private void SetSelectionMode(OcctSelectionMode mode)
    {
        if (_session is null) return;
        ExecuteSafe(() =>
        {
            Session.Engine.SetSelectionMode(mode);
            _selectionCombo.SelectedIndex = (int)mode;
            _commandStatus.Text = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? $"选择过滤器：{SelectionModeName(mode)}" : $"Selection filter: {SelectionModeName(mode)}";
        });
    }

    private void SetActiveVisibility(bool visible)
    {
        if (Session.ActiveObject is not { } active) return;
        ExecuteSafe(() => { Session.Engine.SetObjectVisible(active, visible); RefreshObjectTree(); });
    }

    private void SetActiveColor()
    {
        if (Session.ActiveObject is not { } active) return;
        using var dialog = new ColorDialog { Color = Color.SteelBlue, FullOpen = true };
        if (dialog.ShowDialog(this) == DialogResult.OK) ExecuteSafe(() => Session.Engine.SetObjectColor(active, dialog.Color));
    }

    private void SetActiveMaterial()
    {
        if (Session.ActiveObject is not { Kind: OcctObjectKind.Shape } active) return;
        var parameters = new[] { new DemoParameterDefinition("material", DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "材质" : "Material", DemoParameterKind.Choice, MaterialDisplayName(OcctMaterial.Steel), null, Enum.GetValues<OcctMaterial>().Select(MaterialDisplayName).ToArray()) };
        if (!ParameterDialog.TryGetValues(this, DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? "对象材质" : "Object Material", parameters, out var raw)) return;
        var selectedName = new DemoValues(raw).Text("material");
        var material = Enum.GetValues<OcctMaterial>().First(item => MaterialDisplayName(item) == selectedName);
        ExecuteSafe(() => Session.Engine.SetObjectMaterial(active, material));
    }

    private bool ConfirmDiscardChanges()
    {
        if (_session?.IsModified != true) return true;
        var answer = MessageBox.Show(this, DemoLocalization.Text("Dialog.ConfirmDiscard"), DemoLocalization.Text("Dialog.ConfirmDiscardTitle"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (answer == DialogResult.Cancel) return false;
        if (answer == DialogResult.Yes) return SaveDocument(false);
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
            _undoMenuItem.Enabled = canUndo;
            _undoMenuItem.Text = canUndo ? DemoLocalization.Text("History.Undo", _session!.UndoDescription!) : DemoLocalization.Text("Menu.Undo");
        }
        if (_redoMenuItem is not null)
        {
            _redoMenuItem.Enabled = canRedo;
            _redoMenuItem.Text = canRedo ? DemoLocalization.Text("History.Redo", _session!.RedoDescription!) : DemoLocalization.Text("Menu.Redo");
        }
        if (_undoButton is not null) _undoButton.Enabled = canUndo;
        if (_redoButton is not null) _redoButton.Enabled = canRedo;
    }

    private void MainFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!ConfirmDiscardChanges()) e.Cancel = true;
    }

    private void MainFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Z) { Undo(); e.Handled = true; }
        else if (e.Control && e.KeyCode == Keys.Y) { Redo(); e.Handled = true; }
        else if (e.Control && e.KeyCode == Keys.N) { NewDocument(); e.Handled = true; }
        else if (e.Control && e.KeyCode == Keys.O) { OpenDocument(); e.Handled = true; }
        else if (e.Control && e.KeyCode == Keys.S) { SaveDocument(e.Shift); e.Handled = true; }
        else if (e.KeyCode == Keys.Delete) { RunCommand(DemoCommandId.Delete); e.Handled = true; }
        else if (e.KeyCode == Keys.F) { Session.Engine.FitAll(); e.Handled = true; }
        else if (e.KeyCode == Keys.D0) { Session.Engine.SetView(OcctViewOrientation.Isometric); e.Handled = true; }
        else if (e.KeyCode == Keys.D1) { Session.Engine.SetView(OcctViewOrientation.Front); e.Handled = true; }
        else if (e.KeyCode == Keys.D2) { Session.Engine.SetView(OcctViewOrientation.Left); e.Handled = true; }
        else if (e.KeyCode == Keys.D3) { Session.Engine.SetView(OcctViewOrientation.Top); e.Handled = true; }
        else if (e.KeyCode == Keys.Escape && _session is not null) { Session.Engine.ClearSelection(); _viewport.RaiseSelectionChanged(); e.Handled = true; }
    }
}

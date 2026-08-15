using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;

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

        DemoCommandResult? result = null;
        ExecuteSafe(() => result = Session.Execute(id, input.Values));
        if (result is null) return;

        if (!string.IsNullOrWhiteSpace(result.AnalysisText))
        {
            Log(result.AnalysisText);
            await DialogService.ShowMessageAsync(this, definition.Text, result.AnalysisText);
        }
        RefreshObjectTree();
    }

    private void ReportCommandPrecondition(string message)
    {
        _commandStatus.Text = message;
        Log(message);
        _viewport.Focus();
    }

    private async Task NewDocumentAsync()
    {
        if (!await ConfirmDiscardChangesAsync()) return;
        ExecuteSafe(Session.NewDocument);
    }

    private async Task OpenDocumentAsync()
    {
        if (!await ConfirmDiscardChangesAsync()) return;
        if (!StorageProvider.CanOpen)
        {
            await DialogService.ShowMessageAsync(this, DemoLocalization.Text("Dialog.OpenTitle"),
                Local("The current platform does not provide a file-open picker.", "当前平台不支持文件打开选择器。"));
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = DemoLocalization.Text("Dialog.OpenTitle"),
            AllowMultiple = false,
            FileTypeFilter = CadOpenFileTypes()
        });
        if (files.Count == 0) return;

        var path = await RequireLocalPathAsync(files[0]);
        if (path is null) return;
        ExecuteSafe(() => Session.Open(path));
    }

    private async Task ImportDocumentAsync()
    {
        if (!StorageProvider.CanOpen)
        {
            await DialogService.ShowMessageAsync(this, DemoLocalization.Text("Dialog.ImportTitle"),
                Local("The current platform does not provide a file-open picker.", "当前平台不支持文件打开选择器。"));
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = DemoLocalization.Text("Dialog.ImportTitle"),
            AllowMultiple = true,
            FileTypeFilter = CadOpenFileTypes()
        });
        if (files.Count == 0) return;

        var paths = new List<string>(files.Count);
        foreach (var file in files)
        {
            var path = await RequireLocalPathAsync(file);
            if (path is null) return;
            paths.Add(path);
        }

        ExecuteSafe(() =>
        {
            foreach (var path in paths) Session.Import(path);
        });
    }

    private async Task<bool> SaveDocumentAsync(bool saveAs)
    {
        var file = Session.CurrentFilePath;
        if (saveAs || string.IsNullOrWhiteSpace(file))
        {
            if (!StorageProvider.CanSave)
            {
                await DialogService.ShowMessageAsync(this, DemoLocalization.Text("Dialog.SaveTitle"),
                    Local("The current platform does not provide a file-save picker.", "当前平台不支持文件保存选择器。"));
                return false;
            }

            var picked = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = DemoLocalization.Text("Dialog.SaveTitle"),
                SuggestedFileName = string.IsNullOrWhiteSpace(file) ? "model.step" : Path.GetFileName(file),
                DefaultExtension = "step",
                ShowOverwritePrompt = true,
                FileTypeChoices = CadSaveFileTypes()
            });
            if (picked is null) return false;
            file = await RequireLocalPathAsync(picked);
            if (file is null) return false;
        }

        var succeeded = false;
        ExecuteSafe(() =>
        {
            Session.SaveAll(file!);
            succeeded = true;
        });
        return succeeded;
    }

    private async Task ExportSelectedAsync()
    {
        if (!StorageProvider.CanSave)
        {
            await DialogService.ShowMessageAsync(this, DemoLocalization.Text("Dialog.ExportTitle"),
                Local("The current platform does not provide a file-save picker.", "当前平台不支持文件保存选择器。"));
            return;
        }

        var picked = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = DemoLocalization.Text("Dialog.ExportTitle"),
            SuggestedFileName = "selection.step",
            DefaultExtension = "step",
            ShowOverwritePrompt = true,
            FileTypeChoices = CadSaveFileTypes()
        });
        if (picked is null) return;
        var path = await RequireLocalPathAsync(picked);
        if (path is null) return;
        ExecuteSafe(() => Session.ExportSelected(path));
    }

    private async Task ExportViewImageAsync()
    {
        if (!StorageProvider.CanSave)
        {
            await DialogService.ShowMessageAsync(this, DemoLocalization.Text("Dialog.ExportImageTitle"),
                Local("The current platform does not provide a file-save picker.", "当前平台不支持文件保存选择器。"));
            return;
        }

        var picked = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = DemoLocalization.Text("Dialog.ExportImageTitle"),
            SuggestedFileName = "view.png",
            DefaultExtension = "png",
            ShowOverwritePrompt = true,
            FileTypeChoices = ImageFileTypes()
        });
        if (picked is null) return;
        var path = await RequireLocalPathAsync(picked);
        if (path is null) return;

        ExecuteSafe(() =>
        {
            Session.Engine.DumpView(path);
            Log(Local($"View image exported: {path}", $"已导出视图图片：{path}"));
        });
    }

    private async Task<string?> RequireLocalPathAsync(IStorageItem item)
    {
        var path = item.TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(path)) return path;

        await DialogService.ShowMessageAsync(this, Local("Unsupported storage item", "不支持的存储项"),
            Local(
                "The selected item does not expose a local filesystem path. The desktop CAD demo currently supports local Windows/Linux files only.",
                "所选项目没有可用的本地文件系统路径。当前桌面 CAD Demo 仅支持 Windows/Linux 本地文件。"));
        return null;
    }

    private static IReadOnlyList<FilePickerFileType> CadOpenFileTypes() => new FilePickerFileType[]
    {
        new(Local("All supported CAD files", "所有支持的 CAD 文件")) { Patterns = new[] { "*.step", "*.stp", "*.iges", "*.igs", "*.brep", "*.rle", "*.stl" } },
        new("STEP") { Patterns = new[] { "*.step", "*.stp" } },
        new("IGES") { Patterns = new[] { "*.iges", "*.igs" } },
        new("BREP") { Patterns = new[] { "*.brep", "*.rle" } },
        new("STL") { Patterns = new[] { "*.stl" } },
        FilePickerFileTypes.All
    };

    private static IReadOnlyList<FilePickerFileType> CadSaveFileTypes() => new FilePickerFileType[]
    {
        new("STEP") { Patterns = new[] { "*.step", "*.stp" } },
        new("IGES") { Patterns = new[] { "*.iges", "*.igs" } },
        new("BREP") { Patterns = new[] { "*.brep" } },
        new("STL") { Patterns = new[] { "*.stl" } }
    };

    private static IReadOnlyList<FilePickerFileType> ImageFileTypes() => new FilePickerFileType[]
    {
        FilePickerFileTypes.ImagePng,
        FilePickerFileTypes.ImageJpg,
        new("BMP") { Patterns = new[] { "*.bmp" }, MimeTypes = new[] { "image/bmp" } }
    };

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

    private async Task SetSelectionHighlightColorAsync()
    {
        var color = await DialogService.PickColorAsync(this, Local("Selected highlight color", "选中高亮颜色"), _selectionHighlightColor);
        if (color is null) return;
        ExecuteSafe(() =>
        {
            _selectionHighlightColor = color.Value;
            Session.Engine.SetSelectionHighlightColor(color.Value);
        });
    }

    private async Task SetHoverHighlightColorAsync()
    {
        var color = await DialogService.PickColorAsync(this, Local("Hover highlight color", "悬浮高亮颜色"), _hoverHighlightColor);
        if (color is null) return;
        ExecuteSafe(() =>
        {
            _hoverHighlightColor = color.Value;
            Session.Engine.SetHoverHighlightColor(color.Value);
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

    private async Task SetBackgroundColorAsync()
    {
        var color = await DialogService.PickColorAsync(this, DemoLocalization.Text("Menu.Background"), DrawingColor.White);
        if (color is not null)
            ExecuteSafe(() => Session.Engine.SetBackground(color.Value));
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

    private async Task SetObjectColorAsync(IOcctObject value)
    {
        var color = await DialogService.PickColorAsync(this, Local("Object color", "对象颜色"), DrawingColor.SteelBlue);
        if (color is not null)
            ExecuteSafe(() => Session.Engine.SetColor(value, color.Value));
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

    private async Task<bool> ConfirmDiscardChangesAsync()
    {
        if (_session?.IsModified != true) return true;

        var answer = await DialogService.ShowQuestionAsync(
            this,
            DemoLocalization.Text("Dialog.ConfirmDiscardTitle"),
            DemoLocalization.Text("Dialog.ConfirmDiscard"),
            includeCancel: true);
        if (answer == DemoDialogChoice.Cancel) return false;
        if (answer == DemoDialogChoice.Yes) return await SaveDocumentAsync(false);
        return answer == DemoDialogChoice.No;
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

    private async void MainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_closeApproved || _session?.IsModified != true) return;

        e.Cancel = true;
        if (_closePromptActive) return;
        _closePromptActive = true;
        try
        {
            if (await ConfirmDiscardChangesAsync())
            {
                _closeApproved = true;
                Close();
            }
        }
        finally
        {
            _closePromptActive = false;
        }
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
            await NewDocumentAsync();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.O)
        {
            await OpenDocumentAsync();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.S)
        {
            await SaveDocumentAsync(modifiers.HasFlag(KeyModifiers.Shift));
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

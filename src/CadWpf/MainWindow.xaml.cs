using System.Globalization;
using CadCommon;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using Input = System.Windows.Input;

namespace CadWpf;

public partial class MainWindow : System.Windows.Window
{
    private readonly Dictionary<long, Controls.TreeViewItem> _objectNodes = new();
    private CadSession? _session;
    private bool _refreshingTree;
    private Controls.ComboBox? _selectionCombo;
    private Controls.MenuItem? _undoMenuItem;
    private Controls.MenuItem? _redoMenuItem;
    private Controls.Button? _undoButton;
    private Controls.Button? _redoButton;
    private bool _autoZFitEnabled = true;
    private DrawingColor _selectionHighlightColor = DrawingColor.FromArgb(255, 155, 0);
    private DrawingColor _hoverHighlightColor = DrawingColor.FromArgb(0, 185, 255);
    private OcctSceneLightingSettings _lightingSettings = OcctLightingPresets.Create(OcctLightingPreset.Studio);

    public MainWindow()
    {
        InitializeComponent();
        BuildMenus();
        BuildToolbar();
        WireEvents();
        ApplyLanguage();
        Loaded += (_, _) => InitializeSession();
    }

    private CadSession Session => _session ?? throw new InvalidOperationException(
        CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
            ? "OCCT 视口尚未初始化。"
            : "The OCCT viewport has not been initialized.");

    private void WireEvents()
    {
        Viewport.EngineInitialized += (_, _) => Dispatcher.InvokeAsync(InitializeSession);
        Viewport.ObjectSelectionChanged += (_, args) => Dispatcher.InvokeAsync(() =>
        {
            if (_session is null) return;
            _session.ActiveObject = args.SelectedObject;
            SelectionStatus.Text = args.SelectedObjects.Count == 0
                ? CadLocalization.Text("Status.NoneSelected")
                : CadLocalization.Text("Status.Selected", args.SelectedObjects.Count);
            SelectTreeNode(args.SelectedObject);
            ShowObjectProperties(args.SelectedObject);
        });
        Viewport.WorldPointChanged += (_, args) => Dispatcher.InvokeAsync(() =>
            CoordinateStatus.Text = $"X {args.WorldPoint.X:F3}  Y {args.WorldPoint.Y:F3}  Z {args.WorldPoint.Z:F3}");
        ObjectTree.SelectedItemChanged += ObjectTreeSelectedItemChanged;
        Closing += MainWindowClosing;
        PreviewKeyDown += MainWindowPreviewKeyDown;
    }

    private void InitializeSession()
    {
        if (_session is not null) return;
        try
        {
            _session = new CadSession(Viewport.Engine);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        _session.ModelChanged += (_, _) => Dispatcher.InvokeAsync(RefreshObjectTree);
        _session.HistoryChanged += (_, _) => Dispatcher.InvokeAsync(UpdateHistoryUi);
        _session.StatusChanged += (_, message) => Dispatcher.InvokeAsync(() =>
        {
            CommandStatus.Text = message;
            Log(message);
        });

        ExecuteSafe(() =>
        {
            _session.Engine.SetGradientBackground(DrawingColor.White, DrawingColor.FromArgb(202, 221, 238));
            _session.Engine.SetTriedronVisible(true);
            _session.Engine.SetViewCubeVisible(true);
            _session.Engine.SetAntialiasing(true);
            _session.Engine.SetAutoZFitMode(true, 1.0);
            _session.Engine.SetSelectionTolerance(4);
            _session.Engine.SetDefaultMaterial(OcctMaterial.Plastified);
            _session.Engine.SetSelectionHighlightColor(_selectionHighlightColor);
            _session.Engine.SetHoverHighlightColor(_hoverHighlightColor);
            _session.Engine.SetSceneLighting(_lightingSettings);
        });

        CommandStatus.Text = CadLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        SelectionStatus.Text = CadLocalization.Text("Status.NoneSelected");
        RefreshObjectTree();
        UpdateHistoryUi();
    }

    private void BuildMenus()
    {
        MainMenu.Items.Clear();

        var file = Menu(MenuHeader("Menu.File"));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.New"), (_, _) => NewDocument(), "Ctrl+N"));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.Open"), (_, _) => OpenDocument(), "Ctrl+O"));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.Save"), (_, _) => SaveDocument(false), "Ctrl+S"));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.SaveAs"), (_, _) => SaveDocument(true), "Ctrl+Shift+S"));
        file.Items.Add(new Controls.Separator());
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.Import"), (_, _) => ImportDocument()));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.ExportSelected"), (_, _) => ExportSelected()));
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.ExportImage"), (_, _) => ExportViewImage()));
        file.Items.Add(new Controls.Separator());
        file.Items.Add(MenuItem(CadLocalization.Text("Menu.Exit"), (_, _) => Close(), "Alt+F4"));

        var edit = Menu(MenuHeader("Menu.Edit"));
        _undoMenuItem = MenuItem(CadLocalization.Text("Menu.Undo"), (_, _) => Undo(), "Ctrl+Z");
        _redoMenuItem = MenuItem(CadLocalization.Text("Menu.Redo"), (_, _) => Redo(), "Ctrl+Y");
        edit.Items.Add(_undoMenuItem);
        edit.Items.Add(_redoMenuItem);
        edit.Items.Add(new Controls.Separator());
        AddCommands(edit, CadCommandId.Translate, CadCommandId.Rotate, CadCommandId.Scale, CadCommandId.Mirror, CadCommandId.Copy);
        edit.Items.Add(new Controls.Separator());
        AddCommands(edit, CadCommandId.Delete);
        edit.Items.Add(MenuItem(CadLocalization.Text("Menu.ClearSelection"), (_, _) =>
        {
            Session.Engine.ClearSelection();
            Viewport.RaiseSelectionChanged();
        }));
        edit.Items.Add(MenuItem(CadLocalization.Text("Menu.ShowAll"), (_, _) => Session.Engine.ShowAll()));
        edit.Items.Add(MenuItem(CadLocalization.Text("Menu.HideAll"), (_, _) => Session.Engine.HideAll()));

        var draw = Menu(MenuHeader("Menu.Draw"));
        AddCommands(draw, CadCommandId.Point, CadCommandId.Line, CadCommandId.Polyline);
        draw.Items.Add(new Controls.Separator());
        AddCommands(draw, CadCommandId.Circle, CadCommandId.ArcThreePoints, CadCommandId.ArcCenter, CadCommandId.Ellipse);
        draw.Items.Add(new Controls.Separator());
        AddCommands(draw, CadCommandId.Rectangle, CadCommandId.Polygon, CadCommandId.Bezier, CadCommandId.BSpline);

        var solid = Menu(MenuHeader("Menu.Solid"));
        var primitives = Menu(MenuHeader("Menu.Primitives"));
        AddCommands(primitives, CadCommandId.Box, CadCommandId.Cylinder, CadCommandId.Frustum, CadCommandId.Cone,
            CadCommandId.Torus, CadCommandId.Sphere, CadCommandId.Wedge, CadCommandId.Pipe);
        var features = Menu(MenuHeader("Menu.Features"));
        AddCommands(features, CadCommandId.Extrude, CadCommandId.Revolve, CadCommandId.Sweep, CadCommandId.Loft);
        var booleans = Menu(MenuHeader("Menu.Boolean"));
        AddCommands(booleans, CadCommandId.Fuse, CadCommandId.Cut, CadCommandId.Common, CadCommandId.Section);
        var details = Menu(MenuHeader("Menu.Details"));
        AddCommands(details, CadCommandId.Fillet, CadCommandId.Chamfer, CadCommandId.Offset, CadCommandId.Shell, CadCommandId.Drill);
        solid.Items.Add(primitives);
        solid.Items.Add(features);
        solid.Items.Add(booleans);
        solid.Items.Add(details);

        var annotate = Menu(MenuHeader("Menu.Annotate"));
        AddCommands(annotate, CadCommandId.Text);
        annotate.Items.Add(new Controls.Separator());
        AddCommands(annotate, CadCommandId.LengthDimension, CadCommandId.AngleDimension,
            CadCommandId.RadiusDimension, CadCommandId.DiameterDimension);

        var tools = Menu(MenuHeader("Menu.Tools"));
        AddCommands(tools, CadCommandId.AnalyzeBounds, CadCommandId.AnalyzeMass, CadCommandId.AnalyzeTopology,
            CadCommandId.AnalyzeDistance, CadCommandId.ValidateShape);

        var samples = Menu(MenuHeader("Menu.Samples"));
        AddCommands(samples, CadCommandId.DemoElements, CadCommandId.DemoGear, CadCommandId.DemoManifold,
            CadCommandId.DemoTwistedDuct);
        samples.Items.Add(new Controls.Separator());
        AddCommands(samples, CadCommandId.DemoBracket, CadCommandId.DemoFlange, CadCommandId.DemoAnnotations);

        var language = Menu(MenuHeader("Menu.Language"));
        var english = MenuItem(CadLocalization.Text("Menu.English"), (_, _) => SetLanguage(CadLanguage.English));
        var chinese = MenuItem(CadLocalization.Text("Menu.Chinese"), (_, _) => SetLanguage(CadLanguage.ChineseSimplified));
        english.IsCheckable = true;
        english.IsChecked = CadLocalization.CurrentLanguage == CadLanguage.English;
        chinese.IsCheckable = true;
        chinese.IsChecked = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified;
        language.Items.Add(english);
        language.Items.Add(chinese);

        var help = Menu(MenuHeader("Menu.Help"));
        help.Items.Add(MenuItem(CadLocalization.Text("Menu.MouseHelp"), (_, _) => ShowMouseHelp()));
        help.Items.Add(MenuItem(CadLocalization.Text("Menu.About"), (_, _) => ShowAbout()));

        MainMenu.Items.Add(file);
        MainMenu.Items.Add(edit);
        MainMenu.Items.Add(draw);
        MainMenu.Items.Add(solid);
        MainMenu.Items.Add(annotate);
        MainMenu.Items.Add(BuildViewMenu());
        MainMenu.Items.Add(tools);
        MainMenu.Items.Add(samples);
        MainMenu.Items.Add(language);
        MainMenu.Items.Add(help);
        UpdateHistoryUi();
    }

    private Controls.MenuItem BuildViewMenu()
    {
        var view = Menu(MenuHeader("Menu.View"));
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.FitAll"), (_, _) => Session.Engine.FitAll(), "F"));
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.FitSelected"), (_, _) =>
        {
            var shape = ActiveShape();
            if (shape is not null) Session.Engine.Fit(shape.Value);
        }));
        view.Items.Add(new Controls.Separator());

        var display = Menu(MenuHeader("Menu.Display"));
        display.Items.Add(MenuItem(CadLocalization.Text("Menu.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        display.Items.Add(MenuItem(CadLocalization.Text("Menu.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        display.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.Hlr"), false, item => Session.Engine.SetComputedHlr(item.IsChecked)));
        display.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.Antialiasing"), true, item => Session.Engine.SetAntialiasing(item.IsChecked)));
        display.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.Triedron"), true, item => ExecuteSafe(() => Session.Engine.SetTriedronVisible(item.IsChecked))));
        display.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.ViewCube"), true, item => ExecuteSafe(() => Session.Engine.SetViewCubeVisible(item.IsChecked))));
        view.Items.Add(display);
        view.Items.Add(BuildDepthMenu());

        var standard = Menu(MenuHeader("Menu.StandardViews"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        standard.Items.Add(new Controls.Separator());
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(CadIsoView.NorthEast)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(CadIsoView.NorthWest)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(CadIsoView.SouthEast)));
        standard.Items.Add(MenuItem(CadLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(CadIsoView.SouthWest)));
        view.Items.Add(standard);

        var projection = Menu(MenuHeader("Menu.Projection"));
        projection.Items.Add(MenuItem(CadLocalization.Text("Menu.Orthographic"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Orthographic)));
        projection.Items.Add(MenuItem(CadLocalization.Text("Menu.Perspective"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Perspective)));
        projection.Items.Add(MenuItem(CadLocalization.Text("Menu.PerspectiveFov"), (_, _) => SetPerspectiveFov()));
        view.Items.Add(projection);

        view.Items.Add(MenuItem(CadLocalization.Text("Menu.DisplayPrecision"), (_, _) => SetDisplayPrecision()));
        view.Items.Add(BuildLightingMenu());
        view.Items.Add(BuildMaterialMenu());
        view.Items.Add(BuildSelectionMenu());
        view.Items.Add(BuildSelectionAppearanceMenu());
        view.Items.Add(CheckMenuItem(CadLocalization.Text("Menu.WindowSelection"), Viewport.EnableRectangleSelection, item =>
        {
            Viewport.EnableRectangleSelection = item.IsChecked;
            CommandStatus.Text = CadLocalization.Text(item.IsChecked ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
        }));
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.SelectionTolerance"), (_, _) => SetSelectionTolerance()));
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.Background"), (_, _) => SetBackgroundColor()));
        view.Items.Add(MenuItem(CadLocalization.Text("Menu.GradientBackground"), (_, _) =>
            Session.Engine.SetGradientBackground(DrawingColor.White, DrawingColor.LightSteelBlue)));
        return view;
    }

    private Controls.MenuItem BuildDepthMenu()
    {
        var menu = Menu(MenuHeader("Menu.DepthHandling"));
        menu.Items.Add(CheckMenuItem(
            CadLocalization.Text("Menu.AutoZFit"),
            _autoZFitEnabled,
            item => ExecuteSafe(() =>
            {
                _autoZFitEnabled = item.IsChecked;
                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                var message = CadLocalization.Text(
                    _autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                CommandStatus.Text = message;
                Log(message);
            })));
        menu.Items.Add(MenuItem(
            CadLocalization.Text("Menu.AutoZFitNow"),
            (_, _) => ExecuteSafe(Session.Engine.AutoZFit)));
        menu.Items.Add(new Controls.Separator());
        menu.Items.Add(MenuItem(
            CadLocalization.Text("Menu.DepthForward"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Forward)));
        menu.Items.Add(MenuItem(
            CadLocalization.Text("Menu.DepthBackward"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Backward)));
        menu.Items.Add(MenuItem(
            CadLocalization.Text("Menu.DepthReset"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Default)));
        return menu;
    }

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

    private Controls.MenuItem BuildMaterialMenu()
    {
        var menu = Menu(MenuHeader("Menu.Material"));
        foreach (var material in Enum.GetValues<OcctMaterial>())
        {
            var captured = material;
            var item = MenuItem(MaterialDisplayName(captured), (_, _) =>
            {
                var apply = System.Windows.MessageBox.Show(this,
                    CadLocalization.Text("Dialog.ApplyExistingMaterial"),
                    CadLocalization.Text("Menu.Material"),
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;
                Session.Engine.SetDefaultMaterial(captured, apply);
                Log($"{CadLocalization.Text("Menu.Material")}: {MaterialDisplayName(captured)}");
            });
            menu.Items.Add(item);
        }
        return menu;
    }

    private Controls.MenuItem BuildSelectionMenu()
    {
        var menu = Menu(MenuHeader("Menu.SelectionMode"));
        foreach (var mode in Enum.GetValues<OcctSelectionMode>())
        {
            var captured = mode;
            menu.Items.Add(MenuItem(SelectionModeName(captured), (_, _) => SetSelectionMode(captured)));
        }
        return menu;
    }

    private void BuildToolbar()
    {
        var selectedIndex = _selectionCombo?.SelectedIndex ?? 0;
        MainToolBar.Items.Clear();
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.New"), (_, _) => NewDocument()));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Open"), (_, _) => OpenDocument()));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Save"), (_, _) => SaveDocument(false)));
        MainToolBar.Items.Add(new Controls.Separator());
        _undoButton = ToolButton(CadLocalization.Text("Toolbar.Undo"), (_, _) => Undo());
        _redoButton = ToolButton(CadLocalization.Text("Toolbar.Redo"), (_, _) => Redo());
        MainToolBar.Items.Add(_undoButton);
        MainToolBar.Items.Add(_redoButton);
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Line"), CadCommandId.Line));
        MainToolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Circle"), CadCommandId.Circle));
        MainToolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Box"), CadCommandId.Box));
        MainToolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Cylinder"), CadCommandId.Cylinder));
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Extents"), (_, _) => Session.Engine.FitAll()));
        MainToolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        MainToolBar.Items.Add(new Controls.Separator());
        MainToolBar.Items.Add(new Controls.TextBlock
        {
            Text = CadLocalization.Text("Toolbar.Selection"),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Margin = new System.Windows.Thickness(6, 0, 2, 0)
        });
        _selectionCombo = new Controls.ComboBox { Width = 125, Margin = new System.Windows.Thickness(2) };
        foreach (var mode in Enum.GetValues<OcctSelectionMode>()) _selectionCombo.Items.Add(SelectionModeName(mode));
        _selectionCombo.SelectedIndex = Math.Clamp(selectedIndex, 0, _selectionCombo.Items.Count - 1);
        _selectionCombo.SelectionChanged += (_, _) =>
        {
            if (_selectionCombo.SelectedIndex >= 0) SetSelectionMode((OcctSelectionMode)_selectionCombo.SelectedIndex);
        };
        MainToolBar.Items.Add(_selectionCombo);
        UpdateHistoryUi();
    }

    private void AddCommands(Controls.MenuItem parent, params CadCommandId[] commands)
    {
        foreach (var id in commands)
        {
            var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));
            var item = MenuItem(definition.Text, (_, _) => RunCommand(id), definition.Shortcut);
            item.ToolTip = definition.Description;
            parent.Items.Add(item);
        }
    }

    private void RunCommand(CadCommandId id)
    {
        if (_session is null) return;
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


    private Controls.MenuItem BuildLightingMenu()
    {
        var menu = Menu(Local("Lighting", "灯光"));
        foreach (var preset in Enum.GetValues<OcctLightingPreset>())
        {
            var captured = preset;
            menu.Items.Add(MenuItem(LightingPresetName(captured), (_, _) => ApplyLightingPreset(captured)));
        }
        menu.Items.Add(new Controls.Separator());
        menu.Items.Add(MenuItem(Local("Custom Lighting...", "自定义灯光..."), (_, _) => SetAdvancedLighting()));
        menu.Items.Add(MenuItem(Local("OCCT Default Lights", "恢复 OCCT 默认灯光"), (_, _) => ExecuteSafe(Session.Engine.ResetSceneLighting)));
        return menu;
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

    private Controls.MenuItem BuildSelectionAppearanceMenu()
    {
        var menu = Menu(Local("Selection Appearance", "选择外观"));
        menu.Items.Add(MenuItem(Local("Selected Color...", "选中高亮颜色..."), (_, _) => SetSelectionHighlightColor()));
        menu.Items.Add(MenuItem(Local("Hover Color...", "悬浮高亮颜色..."), (_, _) => SetHoverHighlightColor()));
        return menu;
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

    private void RefreshObjectTree()
    {
        if (_session is null) return;
        _refreshingTree = true;
        try
        {
            ObjectTree.Items.Clear();
            _objectNodes.Clear();
            var shapeRoot = TreeRoot(Local("Shapes", "形体"));
            var textRoot = TreeRoot(Local("Text", "文字"));
            var dimensionRoot = TreeRoot(Local("Dimensions", "尺寸"));
            ObjectTree.Items.Add(shapeRoot);
            ObjectTree.Items.Add(textRoot);
            ObjectTree.Items.Add(dimensionRoot);

            foreach (var value in Session.Engine.Objects)
            {
                var parent = value.Kind switch
                {
                    OcctObjectKind.Text => textRoot,
                    OcctObjectKind.Dimension => dimensionRoot,
                    _ => shapeRoot
                };
                var visible = new Controls.CheckBox
                {
                    Content = Session.SafeName(value),
                    IsChecked = true,
                    Tag = value
                };
                visible.Checked += ObjectVisibilityChanged;
                visible.Unchecked += ObjectVisibilityChanged;
                var item = new Controls.TreeViewItem
                {
                    Header = visible,
                    Tag = value,
                    ContextMenu = BuildObjectContextMenu(value)
                };
                parent.Items.Add(item);
                _objectNodes[value.Id] = item;
            }
            shapeRoot.IsExpanded = true;
            textRoot.IsExpanded = true;
            dimensionRoot.IsExpanded = true;
        }
        finally
        {
            _refreshingTree = false;
        }

        ShowObjectProperties(Session.ActiveObject);
        SelectionStatus.Text = Local(
            $"Objects {Session.Engine.ObjectCount} / Shapes {Session.Engine.ShapeCount}",
            $"对象 {Session.Engine.ObjectCount} / 形体 {Session.Engine.ShapeCount}");
    }

    private Controls.ContextMenu BuildObjectContextMenu(OcctObject value)
    {
        var menu = new Controls.ContextMenu();
        menu.Items.Add(MenuItem(CadLocalization.Text("Menu.FitSelected"), (_, _) =>
        {
            Session.ActiveObject = value;
            if (value.Kind == OcctObjectKind.Shape) Session.Engine.Fit(new OcctShape(value.Id));
        }));
        menu.Items.Add(MenuItem(Local("Show", "显示"), (_, _) => Session.Engine.SetVisible(value, true)));
        menu.Items.Add(MenuItem(Local("Hide", "隐藏"), (_, _) => Session.Engine.SetVisible(value, false)));
        menu.Items.Add(MenuItem(Local("Color...", "颜色..."), (_, _) => SetObjectColor(value)));
        menu.Items.Add(MenuItem(Local("Material...", "材质..."), (_, _) => SetObjectMaterial(value)));
        menu.Items.Add(new Controls.Separator());
        menu.Items.Add(MenuItem(CadLocalization.CommandText(CadCommandId.Delete), (_, _) =>
        {
            Session.ActiveObject = value;
            RunCommand(CadCommandId.Delete);
        }));
        return menu;
    }

    private void ObjectVisibilityChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_refreshingTree || _session is null || sender is not Controls.CheckBox { Tag: OcctObject value } checkBox) return;
        ExecuteSafe(() => Session.Engine.SetVisible(value, checkBox.IsChecked == true));
    }

    private void ObjectTreeSelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
    {
        if (_session is null || e.NewValue is not Controls.TreeViewItem { Tag: OcctObject value }) return;
        Session.ActiveObject = value;
        Session.Engine.SelectObject(value, false);
        Viewport.RaiseSelectionChanged();
        ShowObjectProperties(value);
        SelectionStatus.Text = Local($"Current: {Session.SafeName(value)}", $"当前：{Session.SafeName(value)}");
    }

    private void ShowObjectProperties(OcctObject? value)
    {
        PropertyGrid.ItemsSource = value is null || _session is null ? null : Session.DescribeObject(value.Value);
    }

    private void SelectTreeNode(OcctObject? value)
    {
        if (value is null || !_objectNodes.TryGetValue(value.Value.Id, out var item)) return;
        item.IsSelected = true;
        item.BringIntoView();
    }

    private void SetObjectColor(OcctObject value)
    {
        using var dialog = new System.Windows.Forms.ColorDialog { Color = DrawingColor.SteelBlue, FullOpen = true };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ExecuteSafe(() => Session.Engine.SetColor(value, dialog.Color));
        }
    }

    private void SetObjectMaterial(OcctObject value)
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

    private OcctShape? ActiveShape()
    {
        if (_session?.ActiveObject is { Kind: OcctObjectKind.Shape } active) return new OcctShape(active.Id);
        return _session?.Engine.FirstSelected;
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

    private void SetLanguage(CadLanguage language)
    {
        CadLocalization.CurrentLanguage = language;
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        FontFamily = new System.Windows.Media.FontFamily(
            CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "Microsoft YaHei UI" : "Segoe UI");
        Title = CadLocalization.Text("AppTitle.Wpf");
        ModelExplorerGroup.Header = CadLocalization.Text("Panel.ModelExplorer");
        PropertiesGroup.Header = CadLocalization.Text("Panel.Properties");
        CommandLineGroup.Header = CadLocalization.Text("Panel.CommandLine");
        PropertyNameColumn.Header = CadLocalization.Text("Property.Name");
        PropertyValueColumn.Header = CadLocalization.Text("Property.Value");
        if (_session is null)
        {
            CommandStatus.Text = CadLocalization.Text("Status.Initializing");
            SelectionStatus.Text = CadLocalization.Text("Status.NoneSelected");
        }
        else
        {
            CommandStatus.Text = CadLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        }
        BuildMenus();
        BuildToolbar();
        RefreshObjectTree();
        ShowObjectProperties(_session?.ActiveObject);
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

    private void ExecuteSafe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            var logPath = CrashReporter.Write("CAD-WPF", exception, "MainWindow.ExecuteSafe");
            var logMessage = string.IsNullOrWhiteSpace(logPath)
                ? string.Empty
                : $"{Environment.NewLine}{Environment.NewLine}Log: {logPath}";
            Log($"ERROR: {exception.Message}");
            System.Windows.MessageBox.Show(this, exception.Message + logMessage,
                CadLocalization.Text("Dialog.ErrorTitle"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void Log(string message)
    {
        LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        LogBox.ScrollToEnd();
    }

    private void ShowMouseHelp()
    {
        System.Windows.MessageBox.Show(this, CadLocalization.Text("Dialog.MouseText"),
            CadLocalization.Text("Menu.MouseHelp"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    private void ShowAbout()
    {
        System.Windows.MessageBox.Show(this, CadLocalization.Text("Dialog.AboutText"),
            CadLocalization.Text("Menu.About"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    private static string MenuHeader(string key) =>
        CadLocalization.Text(key).Replace("&", "_", StringComparison.Ordinal);

    private static Controls.MenuItem Menu(string header) => new() { Header = header };

    private static Controls.MenuItem MenuItem(string header, System.Windows.RoutedEventHandler handler, string? shortcut = null)
    {
        var item = new Controls.MenuItem { Header = header, InputGestureText = shortcut ?? string.Empty };
        item.Click += handler;
        return item;
    }

    private static Controls.MenuItem CheckMenuItem(string header, bool checkedValue, Action<Controls.MenuItem> handler)
    {
        var item = new Controls.MenuItem { Header = header, IsCheckable = true, IsChecked = checkedValue };
        item.Click += (_, _) => handler(item);
        return item;
    }

    private static Controls.Button ToolButton(string text, System.Windows.RoutedEventHandler handler)
    {
        var button = new Controls.Button { Content = text, ToolTip = text };
        button.Click += handler;
        return button;
    }

    private Controls.Button CommandButton(string text, CadCommandId command)
    {
        var button = new Controls.Button { Content = text, Tag = command, ToolTip = text };
        button.Click += (_, _) => RunCommand((CadCommandId)button.Tag!);
        return button;
    }

    private static Controls.TreeViewItem TreeRoot(string header) => new() { Header = header };

    private static string CadFileFilter() => CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
        ? "所有支持格式|*.step;*.stp;*.iges;*.igs;*.brep;*.rle;*.stl|STEP 文件|*.step;*.stp|IGES 文件|*.iges;*.igs|BREP 文件|*.brep;*.rle|STL 文件|*.stl|所有文件|*.*"
        : "All Supported Files|*.step;*.stp;*.iges;*.igs;*.brep;*.rle;*.stl|STEP Files|*.step;*.stp|IGES Files|*.iges;*.igs|BREP Files|*.brep;*.rle|STL Files|*.stl|All Files|*.*";

    private static string SaveFileFilter() => CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
        ? "STEP 文件|*.step;*.stp|IGES 文件|*.iges;*.igs|BREP 文件|*.brep|STL 文件|*.stl"
        : "STEP Files|*.step;*.stp|IGES Files|*.iges;*.igs|BREP Files|*.brep|STL Files|*.stl";

    private static string SelectionModeName(OcctSelectionMode mode) => CadLocalization.SelectionMode(mode);
    private static string LightingPresetName(OcctLightingPreset preset) => preset switch
    {
        OcctLightingPreset.Neutral => Local("Neutral", "中性"),
        OcctLightingPreset.Sunlight => Local("Sunlight", "日光"),
        OcctLightingPreset.Flat => Local("Flat", "平光"),
        _ => Local("Studio", "摄影棚")
    };

    private static string MaterialDisplayName(OcctMaterial material)
    {
        if (CadLocalization.CurrentLanguage == CadLanguage.English) return material.ToString();
        return material switch
        {
            OcctMaterial.Brass => "黄铜", OcctMaterial.Bronze => "青铜", OcctMaterial.Copper => "铜", OcctMaterial.Gold => "金",
            OcctMaterial.Pewter => "锡合金", OcctMaterial.Plastered => "石膏", OcctMaterial.Plastified => "塑料", OcctMaterial.Silver => "银",
            OcctMaterial.Steel => "钢", OcctMaterial.Stone => "石材", OcctMaterial.ShinyPlastified => "高光塑料", OcctMaterial.Satin => "缎面",
            OcctMaterial.Metalized => "金属化", OcctMaterial.Ionized => "离子化", OcctMaterial.Chrome => "铬", OcctMaterial.Aluminum => "铝",
            OcctMaterial.Obsidian => "黑曜石", OcctMaterial.Neon => "霓虹", OcctMaterial.Jade => "玉石", OcctMaterial.Charcoal => "木炭",
            OcctMaterial.Water => "水", OcctMaterial.Glass => "玻璃", OcctMaterial.Diamond => "钻石", OcctMaterial.Transparent => "透明",
            OcctMaterial.Default => "OCCT 默认", _ => material.ToString()
        };
    }

    private static string Local(string english, string chinese) =>
        CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? chinese : english;
}

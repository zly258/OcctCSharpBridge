using System.Globalization;
using CadCommon;
using OcctNet;

namespace CadWinForms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<long, TreeNode> _objectNodes = new();
    private CadSession? _session;
    private bool _refreshingTree;
    private ToolStripMenuItem? _undoMenuItem;
    private ToolStripMenuItem? _redoMenuItem;
    private ToolStripButton? _undoButton;
    private ToolStripButton? _redoButton;
    private bool _autoZFitEnabled = true;
    private bool _initialPanelLayoutApplied;
    private bool _initialPanelLayoutScheduled;
    private Color _selectionHighlightColor = Color.FromArgb(255, 155, 0);
    private Color _hoverHighlightColor = Color.FromArgb(0, 185, 255);
    private OcctSceneLightingSettings _lightingSettings = OcctLightingPresets.Create(OcctLightingPreset.Studio);

    public MainForm()
    {
        InitializeComponent();
        BuildMenus();
        BuildToolBar();
        WireEvents();
        ApplyLanguage();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ScheduleInitialPanelLayout();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState != FormWindowState.Minimized)
        {
            ScheduleInitialPanelLayout();
        }
    }

    private void ScheduleInitialPanelLayout()
    {
        if (_initialPanelLayoutApplied
            || _initialPanelLayoutScheduled
            || IsDisposed
            || Disposing
            || !IsHandleCreated)
        {
            return;
        }

        _initialPanelLayoutScheduled = true;
        BeginInvoke((Action)(() =>
        {
            _initialPanelLayoutScheduled = false;
            if (IsDisposed
                || Disposing
                || !IsHandleCreated
                || WindowState == FormWindowState.Minimized)
            {
                return;
            }

            _initialPanelLayoutApplied = ApplyInitialPanelLayout();
        }));
    }

    private bool ApplyInitialPanelLayout()
    {
        var mainApplied = TrySetSplitterDistance(
            _mainSplitContainer,
            270,
            keepSecondPanel: false);
        var centerRightApplied = TrySetSplitterDistance(
            _centerRightSplitContainer,
            330,
            keepSecondPanel: true);

        var preferredPropertyHeight = Math.Max(
            260,
            (int)(_rightSplitContainer.ClientSize.Height * 0.62));
        var rightApplied = TrySetSplitterDistance(
            _rightSplitContainer,
            preferredPropertyHeight,
            keepSecondPanel: false);

        return mainApplied && centerRightApplied && rightApplied;
    }

    private static bool TrySetSplitterDistance(
        SplitContainer container,
        int preferredSize,
        bool keepSecondPanel)
    {
        if (container.IsDisposed || !container.IsHandleCreated)
        {
            return false;
        }

        var available = container.Orientation == Orientation.Vertical
            ? container.ClientSize.Width
            : container.ClientSize.Height;
        var minimum = container.Panel1MinSize;
        var maximum = available - container.Panel2MinSize - container.SplitterWidth;

        // During startup, DPI scaling and maximization can temporarily leave less
        // space than both panel minimums require. In that state no legal splitter
        // distance exists, so defer the layout instead of assigning an invalid value.
        if (available <= 0 || maximum < minimum)
        {
            return false;
        }

        var requested = keepSecondPanel
            ? available - preferredSize - container.SplitterWidth
            : preferredSize;
        var distance = Math.Clamp(requested, minimum, maximum);

        if (container.SplitterDistance != distance)
        {
            container.SplitterDistance = distance;
        }

        return true;
    }

    private CadSession Session => _session ?? throw new InvalidOperationException(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "OCCT 视口尚未初始化。" : "The OCCT viewport has not been initialized.");

    private void BuildMenus()
    {
        _menu.Items.Clear();

        var file = new ToolStripMenuItem(CadLocalization.Text("Menu.File"));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.New"), (_, _) => NewDocument(), "Ctrl+N"));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Open"), (_, _) => OpenDocument(), "Ctrl+O"));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Save"), (_, _) => SaveDocument(false), "Ctrl+S"));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.SaveAs"), (_, _) => SaveDocument(true), "Ctrl+Shift+S"));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Import"), (_, _) => ImportDocument()));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.ExportSelected"), (_, _) => ExportSelected()));
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.ExportImage"), (_, _) => ExportViewImage()));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Exit"), (_, _) => Close(), "Alt+F4"));

        var edit = new ToolStripMenuItem(CadLocalization.Text("Menu.Edit"));
        _undoMenuItem = MenuItem(CadLocalization.Text("Menu.Undo"), (_, _) => Undo(), "Ctrl+Z");
        _redoMenuItem = MenuItem(CadLocalization.Text("Menu.Redo"), (_, _) => Redo(), "Ctrl+Y");
        edit.DropDownItems.Add(_undoMenuItem);
        edit.DropDownItems.Add(_redoMenuItem);
        edit.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(edit, CadCommandId.Translate, CadCommandId.Rotate, CadCommandId.Scale, CadCommandId.Mirror, CadCommandId.Copy);
        edit.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(edit, CadCommandId.Delete);
        edit.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.ClearSelection"), (_, _) => { Session.Engine.ClearSelection(); _viewport.RaiseSelectionChanged(); }));
        edit.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.ShowAll"), (_, _) => Session.Engine.ShowAll()));
        edit.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.HideAll"), (_, _) => Session.Engine.HideAll()));

        var draw = new ToolStripMenuItem(CadLocalization.Text("Menu.Draw"));
        AddCommands(draw, CadCommandId.Point, CadCommandId.Line, CadCommandId.Polyline);
        draw.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(draw, CadCommandId.Circle, CadCommandId.ArcThreePoints, CadCommandId.ArcCenter, CadCommandId.Ellipse);
        draw.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(draw, CadCommandId.Rectangle, CadCommandId.Polygon, CadCommandId.Bezier, CadCommandId.BSpline);

        var solid = new ToolStripMenuItem(CadLocalization.Text("Menu.Solid"));
        var primitives = new ToolStripMenuItem(CadLocalization.Text("Menu.Primitives"));
        AddCommands(primitives, CadCommandId.Box, CadCommandId.Cylinder, CadCommandId.Frustum, CadCommandId.Cone, CadCommandId.Torus, CadCommandId.Sphere, CadCommandId.Wedge, CadCommandId.Pipe);
        var features = new ToolStripMenuItem(CadLocalization.Text("Menu.Features"));
        AddCommands(features, CadCommandId.Extrude, CadCommandId.Revolve, CadCommandId.Sweep, CadCommandId.Loft);
        var booleans = new ToolStripMenuItem(CadLocalization.Text("Menu.Boolean"));
        AddCommands(booleans, CadCommandId.Fuse, CadCommandId.Cut, CadCommandId.Common, CadCommandId.Section);
        var details = new ToolStripMenuItem(CadLocalization.Text("Menu.Details"));
        AddCommands(details, CadCommandId.Fillet, CadCommandId.Chamfer, CadCommandId.Offset, CadCommandId.Shell, CadCommandId.Drill);
        solid.DropDownItems.AddRange(new ToolStripItem[] { primitives, features, booleans, details });

        var annotate = new ToolStripMenuItem(CadLocalization.Text("Menu.Annotate"));
        AddCommands(annotate, CadCommandId.Text);
        annotate.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(annotate, CadCommandId.LengthDimension, CadCommandId.AngleDimension, CadCommandId.RadiusDimension, CadCommandId.DiameterDimension);

        var tools = new ToolStripMenuItem(CadLocalization.Text("Menu.Tools"));
        AddCommands(tools, CadCommandId.AnalyzeBounds, CadCommandId.AnalyzeMass, CadCommandId.AnalyzeTopology, CadCommandId.AnalyzeDistance, CadCommandId.ValidateShape);

        var samples = new ToolStripMenuItem(CadLocalization.Text("Menu.Samples"));
        AddCommands(samples, CadCommandId.DemoElements, CadCommandId.DemoGear, CadCommandId.DemoManifold, CadCommandId.DemoTwistedDuct);
        samples.DropDownItems.Add(new ToolStripSeparator());
        AddCommands(samples, CadCommandId.DemoBracket, CadCommandId.DemoFlange, CadCommandId.DemoAnnotations);

        var language = new ToolStripMenuItem(CadLocalization.Text("Menu.Language"));
        var english = new ToolStripMenuItem(CadLocalization.Text("Menu.English")) { Checked = CadLocalization.CurrentLanguage == CadLanguage.English };
        var chinese = new ToolStripMenuItem(CadLocalization.Text("Menu.Chinese")) { Checked = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified };
        english.Click += (_, _) => SetLanguage(CadLanguage.English);
        chinese.Click += (_, _) => SetLanguage(CadLanguage.ChineseSimplified);
        language.DropDownItems.Add(english);
        language.DropDownItems.Add(chinese);

        var help = new ToolStripMenuItem(CadLocalization.Text("Menu.Help"));
        help.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.MouseHelp"), (_, _) => ShowMouseHelp()));
        help.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.About"), (_, _) => ShowAbout()));

        _menu.Items.AddRange(new ToolStripItem[] { file, edit, draw, solid, annotate, BuildViewMenu(), tools, samples, language, help });
        UpdateHistoryUi();
    }

    private ToolStripMenuItem BuildViewMenu()
    {
        var view = new ToolStripMenuItem(CadLocalization.Text("Menu.View"));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.FitAll"), (_, _) => Session.Engine.FitAll(), "F"));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.FitSelected"), (_, _) => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); }));
        view.DropDownItems.Add(new ToolStripSeparator());

        var display = new ToolStripMenuItem(CadLocalization.Text("Menu.Display"));
        display.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        display.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        display.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.Hlr"), false, (_, item) => Session.Engine.SetComputedHlr(item.Checked)));
        display.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.Antialiasing"), true, (_, item) => Session.Engine.SetAntialiasing(item.Checked)));
        display.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.Triedron"), true, (_, item) => ExecuteSafe(() => Session.Engine.SetTriedronVisible(item.Checked))));
        display.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.ViewCube"), true, (_, item) => ExecuteSafe(() => Session.Engine.SetViewCubeVisible(item.Checked))));
        view.DropDownItems.Add(display);
        view.DropDownItems.Add(BuildDepthMenu());

        var standard = new ToolStripMenuItem(CadLocalization.Text("Menu.StandardViews"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Front"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Front), "1"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Back"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Back)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Left"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Left), "2"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Right"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Right)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Top"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Top), "3"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Bottom"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Bottom)));
        standard.DropDownItems.Add(new ToolStripSeparator());
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric), "0"));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.NorthEast"), (_, _) => Session.SetIsoView(CadIsoView.NorthEast)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.NorthWest"), (_, _) => Session.SetIsoView(CadIsoView.NorthWest)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.SouthEast"), (_, _) => Session.SetIsoView(CadIsoView.SouthEast)));
        standard.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.SouthWest"), (_, _) => Session.SetIsoView(CadIsoView.SouthWest)));
        view.DropDownItems.Add(standard);

        var projection = new ToolStripMenuItem(CadLocalization.Text("Menu.Projection"));
        projection.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Orthographic"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Orthographic)));
        projection.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Perspective"), (_, _) => Session.Engine.SetProjection(OcctProjectionType.Perspective)));
        projection.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.PerspectiveFov"), (_, _) => SetPerspectiveFov()));
        view.DropDownItems.Add(projection);

        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.DisplayPrecision"), (_, _) => SetDisplayPrecision()));
        view.DropDownItems.Add(BuildLightingMenu());
        view.DropDownItems.Add(BuildMaterialMenu());
        view.DropDownItems.Add(BuildSelectionMenu());
        view.DropDownItems.Add(BuildSelectionAppearanceMenu());
        view.DropDownItems.Add(CheckMenuItem(CadLocalization.Text("Menu.WindowSelection"), _viewport.EnableRectangleSelection, (_, item) =>
        {
            _viewport.EnableRectangleSelection = item.Checked;
            _commandStatus.Text = CadLocalization.Text(item.Checked ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
        }));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.SelectionTolerance"), (_, _) => SetSelectionTolerance()));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.Background"), (_, _) => SetBackgroundColor()));
        view.DropDownItems.Add(MenuItem(CadLocalization.Text("Menu.GradientBackground"), (_, _) => Session.Engine.SetGradientBackground(Color.White, Color.LightSteelBlue)));
        return view;
    }

    private ToolStripMenuItem BuildDepthMenu()
    {
        var menu = new ToolStripMenuItem(CadLocalization.Text("Menu.DepthHandling"));
        menu.DropDownItems.Add(CheckMenuItem(
            CadLocalization.Text("Menu.AutoZFit"),
            _autoZFitEnabled,
            (_, item) => ExecuteSafe(() =>
            {
                _autoZFitEnabled = item.Checked;
                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                var message = CadLocalization.Text(
                    _autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                _commandStatus.Text = message;
                Log(message);
            })));
        menu.DropDownItems.Add(MenuItem(
            CadLocalization.Text("Menu.AutoZFitNow"),
            (_, _) => ExecuteSafe(Session.Engine.AutoZFit)));
        menu.DropDownItems.Add(new ToolStripSeparator());
        menu.DropDownItems.Add(MenuItem(
            CadLocalization.Text("Menu.DepthForward"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Forward)));
        menu.DropDownItems.Add(MenuItem(
            CadLocalization.Text("Menu.DepthBackward"),
            (_, _) => ApplyDepthBias(CadDepthBiasPreset.Backward)));
        menu.DropDownItems.Add(MenuItem(
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
            _commandStatus.Text = message;
            Log(message);
        });
    }

    private ToolStripMenuItem BuildMaterialMenu()
    {
        var menu = new ToolStripMenuItem(CadLocalization.Text("Menu.Material"));
        foreach (OcctMaterial material in Enum.GetValues<OcctMaterial>())
        {
            var item = new ToolStripMenuItem(MaterialDisplayName(material)) { Tag = material };
            item.Click += (_, _) =>
            {
                var apply = MessageBox.Show(this, CadLocalization.Text("Dialog.ApplyExistingMaterial"), CadLocalization.Text("Menu.Material"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                Session.Engine.SetDefaultMaterial((OcctMaterial)item.Tag!, apply);
                Log($"{CadLocalization.Text("Menu.Material")}: {item.Text}");
            };
            menu.DropDownItems.Add(item);
        }
        return menu;
    }

    private ToolStripMenuItem BuildSelectionMenu()
    {
        var menu = new ToolStripMenuItem(CadLocalization.Text("Menu.SelectionMode"));
        foreach (OcctSelectionMode mode in Enum.GetValues<OcctSelectionMode>())
        {
            var item = new ToolStripMenuItem(SelectionModeName(mode)) { Tag = mode, CheckOnClick = true };
            item.Click += (_, _) => SetSelectionMode((OcctSelectionMode)item.Tag!);
            menu.DropDownItems.Add(item);
        }
        return menu;
    }

    private void BuildToolBar()
    {
        var selectedIndex = Math.Max(_selectionCombo.SelectedIndex, 0);
        _toolBar.Items.Clear();
        _selectionCombo.Items.Clear();
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.New"), (_, _) => NewDocument()));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Open"), (_, _) => OpenDocument()));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Save"), (_, _) => SaveDocument(false)));
        _toolBar.Items.Add(new ToolStripSeparator());
        _undoButton = ToolButton(CadLocalization.Text("Toolbar.Undo"), (_, _) => Undo());
        _redoButton = ToolButton(CadLocalization.Text("Toolbar.Redo"), (_, _) => Redo());
        _toolBar.Items.Add(_undoButton);
        _toolBar.Items.Add(_redoButton);
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Line"), CadCommandId.Line));
        _toolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Circle"), CadCommandId.Circle));
        _toolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Box"), CadCommandId.Box));
        _toolBar.Items.Add(CommandButton(CadLocalization.Text("Toolbar.Cylinder"), CadCommandId.Cylinder));
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Shaded"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Wireframe"), (_, _) => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Extents"), (_, _) => Session.Engine.FitAll()));
        _toolBar.Items.Add(ToolButton(CadLocalization.Text("Toolbar.Isometric"), (_, _) => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        _toolBar.Items.Add(new ToolStripSeparator());
        _toolBar.Items.Add(new ToolStripLabel(CadLocalization.Text("Toolbar.Selection")));
        foreach (var mode in Enum.GetValues<OcctSelectionMode>()) _selectionCombo.Items.Add(SelectionModeName(mode));
        _selectionCombo.SelectedIndex = Math.Min(selectedIndex, _selectionCombo.Items.Count - 1);
        _toolBar.Items.Add(_selectionCombo);
        UpdateHistoryUi();
    }

    private void WireEvents()
    {
        _viewport.EngineInitialized += (_, _) => InitializeSession();
        _viewport.ObjectSelectionChanged += (_, args) =>
        {
            if (_session is null) return;
            _session.ActiveObject = args.SelectedObject;
            _selectionStatus.Text = args.SelectedObjects.Count == 0
                ? CadLocalization.Text("Status.NoneSelected")
                : CadLocalization.Text("Status.Selected", args.SelectedObjects.Count);
            SelectTreeNode(args.SelectedObject);
            ShowObjectProperties(args.SelectedObject);
        };
        _viewport.WorldPointChanged += (_, args) => _coordinateStatus.Text = $"X {args.WorldPoint.X:F3}  Y {args.WorldPoint.Y:F3}  Z {args.WorldPoint.Z:F3}";
        _objectTree.AfterSelect += ObjectTreeAfterSelect;
        _objectTree.AfterCheck += ObjectTreeAfterCheck;
        _objectTree.NodeMouseClick += (_, args) => { if (args.Button == MouseButtons.Right) _objectTree.SelectedNode = args.Node; };
        _objectTree.ContextMenuStrip = BuildTreeContextMenu();
        FormClosing += MainFormClosing;
        KeyDown += MainFormKeyDown;
    }

    private void InitializeSession()
    {
        _session = new CadSession(_viewport.Engine);
        _session.ModelChanged += (_, _) => RefreshObjectTree();
        _session.HistoryChanged += (_, _) => UpdateHistoryUi();
        _session.StatusChanged += (_, message) => { _commandStatus.Text = message; Log(message); };
        _session.Engine.SetGradientBackground(Color.White, Color.FromArgb(202, 221, 238));
        _session.Engine.SetTriedronVisible(true);
        _session.Engine.SetViewCubeVisible(true);
        _session.Engine.SetAntialiasing(true);
        _session.Engine.SetAutoZFitMode(true, 1.0);
        _session.Engine.SetSelectionTolerance(4);
        _session.Engine.SetDefaultMaterial(OcctMaterial.Plastified);
        _session.Engine.SetSelectionHighlightColor(_selectionHighlightColor);
        _session.Engine.SetHoverHighlightColor(_hoverHighlightColor);
        _session.Engine.SetSceneLighting(_lightingSettings);
        _commandStatus.Text = CadLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        _selectionStatus.Text = CadLocalization.Text("Status.NoneSelected");
        RefreshObjectTree();
        UpdateHistoryUi();
    }

    private ContextMenuStrip BuildTreeContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(CadLocalization.Text("Menu.FitSelected"), null, (_, _) => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); });
        menu.Items.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "显示" : "Show", null, (_, _) => SetActiveVisibility(true));
        menu.Items.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "隐藏" : "Hide", null, (_, _) => SetActiveVisibility(false));
        menu.Items.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "颜色..." : "Color...", null, (_, _) => SetActiveColor());
        menu.Items.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "材质..." : "Material...", null, (_, _) => SetActiveMaterial());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(CadLocalization.CommandText(CadCommandId.Delete), null, (_, _) => RunCommand(CadCommandId.Delete));
        return menu;
    }

    private void AddCommands(ToolStripMenuItem parent, params CadCommandId[] commandIds)
    {
        foreach (var id in commandIds)
        {
            var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));
            var item = new ToolStripMenuItem(definition.Text) { Tag = id, ToolTipText = definition.Description };
            if (!string.IsNullOrWhiteSpace(definition.Shortcut)) item.ShortcutKeyDisplayString = definition.Shortcut;
            item.Click += (_, _) => RunCommand((CadCommandId)item.Tag!);
            parent.DropDownItems.Add(item);
        }
    }

    private void ReportCommandPrecondition(string message)
    {
        _commandStatus.Text = message;
        Log(message);
        System.Media.SystemSounds.Asterisk.Play();
        _viewport.Focus();
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
        using var dialog = new OpenFileDialog { Filter = CadFileFilter(), Title = CadLocalization.Text("Dialog.OpenTitle"), Multiselect = false };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() => Session.Open(dialog.FileName));
    }

    private void ImportDocument()
    {
        using var dialog = new OpenFileDialog { Filter = CadFileFilter(), Title = CadLocalization.Text("Dialog.ImportTitle"), Multiselect = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() => { foreach (var file in dialog.FileNames) Session.Import(file); });
    }

    private bool SaveDocument(bool saveAs)
    {
        var file = Session.CurrentFilePath;
        if (saveAs || string.IsNullOrWhiteSpace(file))
        {
            using var dialog = new SaveFileDialog { Filter = SaveFileFilter(), Title = CadLocalization.Text("Dialog.SaveTitle"), DefaultExt = "step", AddExtension = true };
            if (dialog.ShowDialog(this) != DialogResult.OK) return false;
            file = dialog.FileName;
        }
        var succeeded = false;
        ExecuteSafe(() => { Session.SaveAll(file!); succeeded = true; });
        return succeeded;
    }

    private void ExportSelected()
    {
        using var dialog = new SaveFileDialog { Filter = SaveFileFilter(), Title = CadLocalization.Text("Dialog.ExportTitle"), DefaultExt = "step", AddExtension = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() => Session.ExportSelected(dialog.FileName));
    }

    private void ExportViewImage()
    {
        using var dialog = new SaveFileDialog { Filter = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "PNG 图片|*.png|JPEG 图片|*.jpg;*.jpeg|BMP 图片|*.bmp" : "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp", Title = CadLocalization.Text("Dialog.ExportImageTitle"), DefaultExt = "png", AddExtension = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ExecuteSafe(() => { Session.Engine.DumpView(dialog.FileName); Log(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"已导出视图图片：{dialog.FileName}" : $"View image exported: {dialog.FileName}"); });
    }

    private void SetPerspectiveFov()
    {
        var parameters = new[] { new CadParameterDefinition("fov", CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "垂直视场角" : "Vertical Field of View", CadParameterKind.Number, "45", "°") };
        if (!ParameterDialog.TryGetValues(this, CadLocalization.Text("Menu.PerspectiveFov"), parameters, out var values)) return;
        ExecuteSafe(() => Session.Engine.SetPerspectiveFieldOfView(new CadValues(values).Number("fov", 45)));
    }

    private void SetDisplayPrecision()
    {
        var parameters = new[]
        {
            new CadParameterDefinition("coefficient", CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "离散偏差系数" : "Deviation Coefficient", CadParameterKind.Number, "0.001"),
            new CadParameterDefinition("angle", CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "角度偏差" : "Angular Deflection", CadParameterKind.Number, "12", "°"),
            new CadParameterDefinition("existing", CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "应用到现有对象" : "Apply to Existing Objects", CadParameterKind.Boolean, "true")
        };
        if (!ParameterDialog.TryGetValues(this, CadLocalization.Text("Menu.DisplayPrecision"), parameters, out var raw)) return;
        var values = new CadValues(raw);
        ExecuteSafe(() => Session.Engine.SetDisplayPrecision(values.Number("coefficient"), values.Number("angle"), values.Boolean("existing", true)));
    }


    private ToolStripMenuItem BuildLightingMenu()
    {
        var menu = new ToolStripMenuItem(Local("Lighting", "灯光"));
        foreach (var preset in Enum.GetValues<OcctLightingPreset>())
        {
            var captured = preset;
            menu.DropDownItems.Add(MenuItem(LightingPresetName(captured), (_, _) => ApplyLightingPreset(captured)));
        }
        menu.DropDownItems.Add(new ToolStripSeparator());
        menu.DropDownItems.Add(MenuItem(Local("Custom Lighting...", "自定义灯光..."), (_, _) => SetAdvancedLighting()));
        menu.DropDownItems.Add(MenuItem(Local("OCCT Default Lights", "恢复 OCCT 默认灯光"), (_, _) => ExecuteSafe(Session.Engine.ResetSceneLighting)));
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

    private ToolStripMenuItem BuildSelectionAppearanceMenu()
    {
        var menu = new ToolStripMenuItem(Local("Selection Appearance", "选择外观"));
        menu.DropDownItems.Add(MenuItem(Local("Selected Color...", "选中高亮颜色..."), (_, _) => SetSelectionHighlightColor()));
        menu.DropDownItems.Add(MenuItem(Local("Hover Color...", "悬浮高亮颜色..."), (_, _) => SetHoverHighlightColor()));
        return menu;
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
        var parameters = new[] { new CadParameterDefinition("pixels", CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "像素容差" : "Aperture Size", CadParameterKind.Integer, "4", "px") };
        if (!ParameterDialog.TryGetValues(this, CadLocalization.Text("Menu.SelectionTolerance"), parameters, out var raw)) return;
        ExecuteSafe(() => Session.Engine.SetSelectionTolerance(new CadValues(raw).Integer("pixels", 4)));
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
            _commandStatus.Text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"选择过滤器：{SelectionModeName(mode)}" : $"Selection filter: {SelectionModeName(mode)}";
        });
    }

    private void SetActiveVisibility(bool visible)
    {
        if (Session.ActiveObject is not { } active) return;
        ExecuteSafe(() => { Session.Engine.SetVisible(active, visible); RefreshObjectTree(); });
    }

    private void SetActiveColor()
    {
        if (Session.ActiveObject is not { } active) return;
        using var dialog = new ColorDialog { Color = Color.SteelBlue, FullOpen = true };
        if (dialog.ShowDialog(this) == DialogResult.OK) ExecuteSafe(() => Session.Engine.SetColor(active, dialog.Color));
    }

    private void SetActiveMaterial()
    {
        if (Session.ActiveObject is not { Kind: OcctObjectKind.Shape } active) return;
        var parameters = new[] { new CadParameterDefinition("material", CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "材质" : "Material", CadParameterKind.Choice, MaterialDisplayName(OcctMaterial.Steel), null, Enum.GetValues<OcctMaterial>().Select(MaterialDisplayName).ToArray()) };
        if (!ParameterDialog.TryGetValues(this, CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "对象材质" : "Object Material", parameters, out var raw)) return;
        var selectedName = new CadValues(raw).Text("material");
        var material = Enum.GetValues<OcctMaterial>().First(item => MaterialDisplayName(item) == selectedName);
        ExecuteSafe(() => Session.Engine.SetMaterial(active, material));
    }

    private void RefreshObjectTree()
    {
        if (_session is null) return;
        _refreshingTree = true;
        try
        {
            _objectTree.BeginUpdate();
            _objectTree.Nodes.Clear();
            _objectNodes.Clear();
            var shapeRoot = _objectTree.Nodes.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "形体" : "Shapes");
            var textRoot = _objectTree.Nodes.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "文字" : "Text");
            var dimensionRoot = _objectTree.Nodes.Add(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "尺寸" : "Dimensions");
            foreach (var value in Session.Engine.Objects)
            {
                var parent = value.Kind switch
                {
                    OcctObjectKind.Text => textRoot,
                    OcctObjectKind.Dimension => dimensionRoot,
                    _ => shapeRoot
                };
                var node = parent.Nodes.Add(Session.SafeName(value));
                node.Tag = value;
                node.Checked = true;
                _objectNodes[value.Id] = node;
            }
            shapeRoot.Expand(); textRoot.Expand(); dimensionRoot.Expand();
        }
        finally
        {
            _objectTree.EndUpdate();
            _refreshingTree = false;
        }
        ShowObjectProperties(Session.ActiveObject);
        _selectionStatus.Text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"对象 {Session.Engine.ObjectCount} / 形体 {Session.Engine.ShapeCount}" : $"Objects {Session.Engine.ObjectCount} / Shapes {Session.Engine.ShapeCount}";
    }

    private void ObjectTreeAfterSelect(object? sender, TreeViewEventArgs e)
    {
        var node = e.Node;
        if (_session is null || node is null || node.Tag is not OcctObject value) return;

        Session.ActiveObject = value;
        Session.Engine.SelectObject(value, false);
        _viewport.RaiseSelectionChanged();
        ShowObjectProperties(value);
        _selectionStatus.Text = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? $"当前：{Session.SafeName(value)}" : $"Current: {Session.SafeName(value)}";
    }

    private void ObjectTreeAfterCheck(object? sender, TreeViewEventArgs e)
    {
        var node = e.Node;
        if (_refreshingTree || _session is null || node is null || node.Tag is not OcctObject value) return;

        ExecuteSafe(() => Session.Engine.SetVisible(value, node.Checked));
    }

    private void ShowObjectProperties(OcctObject? value)
    {
        _propertyGrid.Rows.Clear();
        if (_session is null || value is null) return;
        foreach (var property in Session.DescribeObject(value.Value)) _propertyGrid.Rows.Add(property.Key, property.Value);
    }

    private void SelectTreeNode(OcctObject? value)
    {
        if (value is null || !_objectNodes.TryGetValue(value.Value.Id, out var node)) return;
        _objectTree.SelectedNode = node;
        node.EnsureVisible();
    }

    private OcctShape? ActiveShape()
    {
        if (_session?.ActiveObject is { Kind: OcctObjectKind.Shape } active) return new OcctShape(active.Id);
        return _session?.Engine.FirstSelected;
    }

    private bool ConfirmDiscardChanges()
    {
        if (_session?.IsModified != true) return true;
        var answer = MessageBox.Show(this, CadLocalization.Text("Dialog.ConfirmDiscard"), CadLocalization.Text("Dialog.ConfirmDiscardTitle"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
            _undoMenuItem.Text = canUndo ? CadLocalization.Text("History.Undo", _session!.UndoDescription!) : CadLocalization.Text("Menu.Undo");
        }
        if (_redoMenuItem is not null)
        {
            _redoMenuItem.Enabled = canRedo;
            _redoMenuItem.Text = canRedo ? CadLocalization.Text("History.Redo", _session!.RedoDescription!) : CadLocalization.Text("Menu.Redo");
        }
        if (_undoButton is not null) _undoButton.Enabled = canUndo;
        if (_redoButton is not null) _redoButton.Enabled = canRedo;
    }

    private void SetLanguage(CadLanguage language)
    {
        CadLocalization.CurrentLanguage = language;
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        Font = new Font(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "Microsoft YaHei UI" : "Segoe UI", 9F);
        Text = CadLocalization.Text("AppTitle.WinForms");
        _objectGroup.Text = CadLocalization.Text("Panel.ModelExplorer");
        _propertyGroup.Text = CadLocalization.Text("Panel.Properties");
        _logGroup.Text = CadLocalization.Text("Panel.CommandLine");
        _propertyNameColumn.HeaderText = CadLocalization.Text("Property.Name");
        _propertyValueColumn.HeaderText = CadLocalization.Text("Property.Value");
        if (_session is null)
        {
            _commandStatus.Text = CadLocalization.Text("Status.Initializing");
            _selectionStatus.Text = CadLocalization.Text("Status.NoneSelected");
        }
        else
        {
            _commandStatus.Text = CadLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        }
        BuildMenus();
        BuildToolBar();
        _objectTree.ContextMenuStrip?.Dispose();
        _objectTree.ContextMenuStrip = BuildTreeContextMenu();
        RefreshObjectTree();
        ShowObjectProperties(_session?.ActiveObject);
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
        else if (e.KeyCode == Keys.Delete) { RunCommand(CadCommandId.Delete); e.Handled = true; }
        else if (e.KeyCode == Keys.F) { Session.Engine.FitAll(); e.Handled = true; }
        else if (e.KeyCode == Keys.D0) { Session.Engine.SetView(OcctViewOrientation.Isometric); e.Handled = true; }
        else if (e.KeyCode == Keys.D1) { Session.Engine.SetView(OcctViewOrientation.Front); e.Handled = true; }
        else if (e.KeyCode == Keys.D2) { Session.Engine.SetView(OcctViewOrientation.Left); e.Handled = true; }
        else if (e.KeyCode == Keys.D3) { Session.Engine.SetView(OcctViewOrientation.Top); e.Handled = true; }
        else if (e.KeyCode == Keys.Escape && _session is not null) { Session.Engine.ClearSelection(); _viewport.RaiseSelectionChanged(); e.Handled = true; }
    }

    private void ExecuteSafe(Action action)
    {
        try { action(); }
        catch (Exception exception)
        {
            var logPath = CrashReporter.Write("CAD-Winform", exception, "MainForm.ExecuteSafe");
            var logMessage = string.IsNullOrWhiteSpace(logPath) ? string.Empty : $"{Environment.NewLine}{Environment.NewLine}Log: {logPath}";
            Log($"ERROR: {exception.Message}");
            MessageBox.Show(this, exception.Message + logMessage, CadLocalization.Text("Dialog.ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Log(string message)
    {
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void ShowMouseHelp()
    {
        MessageBox.Show(this, CadLocalization.Text("Dialog.MouseText"), CadLocalization.Text("Menu.MouseHelp"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowAbout()
    {
        MessageBox.Show(this, CadLocalization.Text("Dialog.AboutText"), CadLocalization.Text("Menu.About"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static ToolStripMenuItem MenuItem(string text, EventHandler handler, string? shortcut = null)
    {
        var item = new ToolStripMenuItem(text);
        if (!string.IsNullOrWhiteSpace(shortcut)) item.ShortcutKeyDisplayString = shortcut;
        item.Click += handler;
        return item;
    }

    private static ToolStripMenuItem CheckMenuItem(string text, bool checkedValue, Action<object?, ToolStripMenuItem> handler)
    {
        var item = new ToolStripMenuItem(text) { Checked = checkedValue, CheckOnClick = true };
        item.Click += (sender, _) => handler(sender, item);
        return item;
    }

    private static ToolStripButton ToolButton(string text, EventHandler handler)
    {
        var button = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        button.Click += handler;
        return button;
    }

    private ToolStripButton CommandButton(string text, CadCommandId command)
    {
        var button = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text, Tag = command };
        button.Click += (_, _) => RunCommand((CadCommandId)button.Tag!);
        return button;
    }

    private static string CadFileFilter() => CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "所有支持格式|*.step;*.stp;*.iges;*.igs;*.brep;*.rle;*.stl|STEP 文件|*.step;*.stp|IGES 文件|*.iges;*.igs|BREP 文件|*.brep;*.rle|STL 文件|*.stl|所有文件|*.*" : "All Supported Files|*.step;*.stp;*.iges;*.igs;*.brep;*.rle;*.stl|STEP Files|*.step;*.stp|IGES Files|*.iges;*.igs|BREP Files|*.brep;*.rle|STL Files|*.stl|All Files|*.*";
    private static string SaveFileFilter() => CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "STEP 文件|*.step;*.stp|IGES 文件|*.iges;*.igs|BREP 文件|*.brep|STL 文件|*.stl" : "STEP Files|*.step;*.stp|IGES Files|*.iges;*.igs|BREP Files|*.brep|STL Files|*.stl";

    private static string SelectionModeName(OcctSelectionMode mode) => CadLocalization.SelectionMode(mode);
    private static string LightingPresetName(OcctLightingPreset preset) => preset switch
    {
        OcctLightingPreset.Neutral => Local("Neutral", "中性"),
        OcctLightingPreset.Sunlight => Local("Sunlight", "日光"),
        OcctLightingPreset.Flat => Local("Flat", "平光"),
        _ => Local("Studio", "摄影棚")
    };
    private static string Local(string english, string chinese) =>
        CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? chinese : english;

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
}

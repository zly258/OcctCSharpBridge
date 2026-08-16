using System.Globalization;
using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<long, TreeNode> _objectNodes = new();
    private DemoSession? _session;
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
        ConfigureViewportContract();
        BuildMenus();
        BuildToolBar();
        WireEvents();
        ApplyLanguage();
    }

    private DemoSession Session => _session ?? throw new InvalidOperationException(
        DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? "OCCT 视口尚未初始化。"
            : "The OCCT viewport has not been initialized.");

    private void ConfigureViewportContract()
    {
        _viewport.InteractionFeatures = OcctViewportInteractionFeatures.Default;
        _viewport.InitialOptions = new OcctViewportInitializationOptions
        {
            BackgroundColor = Color.White,
            ViewOrientation = OcctViewOrientation.Isometric,
            Projection = OcctProjectionType.Orthographic,
            TriedronVisible = true,
            ViewCubeVisible = true
        };
    }

    private void WireEvents()
    {
        _viewport.EngineRecreated += (_, args) => InitializeSession(args.Engine, args.Generation);
        _viewport.EngineDisposing += (_, args) =>
        {
            if (_session?.Engine == args.Engine) _session = null;
        };
        _viewport.FirstFrameRendered += (_, args) =>
            Log($"Viewport generation {args.Generation} first frame rendered.");
        _viewport.NativeHandleChanged += (_, args) =>
            Log($"Viewport native handle changed: 0x{args.PreviousHandle.ToInt64():X} -> 0x{args.NativeHandle.ToInt64():X} (generation {args.Generation}).");
        _viewport.Faulted += (_, args) =>
        {
            _commandStatus.Text = args.Exception.Message;
            Log($"VIEWPORT ERROR: {args.Exception.Message}");
        };
        _viewport.PreviewKeyInput += ViewportPreviewKeyInput;
        _viewport.ObjectSelectionChanged += (_, args) =>
        {
            if (_session is null) return;
            var singleSelection = args.SelectedObjects.Count == 1 ? args.SelectedObject : null;
            _session.ActiveObject = singleSelection;
            _selectionStatus.Text = args.SelectedObjects.Count == 0
                ? DemoLocalization.Text("Status.NoneSelected")
                : DemoLocalization.Text("Status.Selected", args.SelectedObjects.Count);
            SelectTreeNode(singleSelection);
            ShowSelectionProperties(args.SelectedObjects);
        };
        _viewport.WorldPointChanged += (_, args) =>
            _coordinateStatus.Text = $"X {args.WorldPoint.X:F3}  Y {args.WorldPoint.Y:F3}  Z {args.WorldPoint.Z:F3}";
        _objectTree.AfterSelect += ObjectTreeAfterSelect;
        _objectTree.AfterCheck += ObjectTreeAfterCheck;
        _objectTree.NodeMouseClick += (_, args) =>
        {
            if (args.Button == MouseButtons.Right) _objectTree.SelectedNode = args.Node;
        };
        _objectTree.ContextMenuStrip = BuildTreeContextMenu();
        FormClosing += MainFormClosing;
        KeyDown += MainFormKeyDown;
    }

    private void InitializeSession(OcctEngine engine, long generation)
    {
        _depthDefaultsApplied = false;
        _session = new DemoSession(engine);
        _session.ModelChanged += (_, _) => RefreshObjectTree();
        _session.HistoryChanged += (_, _) => UpdateHistoryUi();
        _session.StatusChanged += (_, message) =>
        {
            _commandStatus.Text = message;
            Log(message);
        };

        using (engine.BeginDisplayBatch())
        {
            engine.SetGradientBackground(Color.White, Color.FromArgb(202, 221, 238));
            engine.SetTriedronVisible(true);
            engine.SetViewCubeVisible(true);
            ApplyViewCubeLanguage();
            engine.SetAntialiasing(true);
            engine.SetAutoZFitMode(true, 1.0);
            engine.SetSelectionTolerance(4);
            engine.SetDefaultMaterial(OcctMaterial.Plastified);
            engine.SetSelectionHighlightColor(_selectionHighlightColor);
            engine.SetHoverHighlightColor(_hoverHighlightColor);
            engine.SetSceneLighting(_lightingSettings);
            ApplyDepthDisplayDefaults();
        }

        _commandStatus.Text = DemoLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        _selectionStatus.Text = DemoLocalization.Text("Status.NoneSelected");
        Log($"Viewport ready on engine generation {generation}.");
        RefreshObjectTree();
        UpdateHistoryUi();
    }

    private void ExecuteSafe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            var logPath = CrashReporter.Write("CAD-Winform", exception, "MainForm.ExecuteSafe");
            var logMessage = string.IsNullOrWhiteSpace(logPath)
                ? string.Empty
                : $"{Environment.NewLine}{Environment.NewLine}Log: {logPath}";
            Log($"ERROR: {exception.Message}");
            MessageBox.Show(this, exception.Message + logMessage, DemoLocalization.Text("Dialog.ErrorTitle"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Log(string message)
    {
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
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

    private ToolStripButton CommandButton(string text, DemoCommandId command)
    {
        var button = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text, Tag = command };
        button.Click += (_, _) => RunCommand((DemoCommandId)button.Tag!);
        return button;
    }

    private static string CadFileFilter() => DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
        ? "所有支持格式|*.step;*.stp;*.iges;*.igs;*.brep;*.rle;*.stl|STEP 文件|*.step;*.stp|IGES 文件|*.iges;*.igs|BREP 文件|*.brep;*.rle|STL 文件|*.stl|所有文件|*.*"
        : "All Supported Files|*.step;*.stp;*.iges;*.igs;*.brep;*.rle;*.stl|STEP Files|*.step;*.stp|IGES Files|*.iges;*.igs|BREP Files|*.brep;*.rle|STL Files|*.stl|All Files|*.*";

    private static string SaveFileFilter() => DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
        ? "STEP 文件|*.step;*.stp|IGES 文件|*.iges;*.igs|BREP 文件|*.brep|STL 文件|*.stl"
        : "STEP Files|*.step;*.stp|IGES Files|*.iges;*.igs|BREP Files|*.brep|STL Files|*.stl";

    private static string SelectionModeName(OcctSelectionMode mode) => DemoLocalization.SelectionMode(mode);

    private static string LightingPresetName(OcctLightingPreset preset) => preset switch
    {
        OcctLightingPreset.Neutral => Local("Neutral", "中性"),
        OcctLightingPreset.Sunlight => Local("Sunlight", "日光"),
        OcctLightingPreset.Flat => Local("Flat", "平光"),
        _ => Local("Studio", "摄影棚")
    };

    private static string Local(string english, string chinese) =>
        DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? chinese : english;
}

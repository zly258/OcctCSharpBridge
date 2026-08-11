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
        BuildMenus();
        BuildToolBar();
        WireEvents();
        ApplyLanguage();
    }

    private DemoSession Session => _session ?? throw new InvalidOperationException(
        DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? "OCCT 视口尚未初始化。"
            : "The OCCT viewport has not been initialized.");

    private void WireEvents()
    {
        _viewport.EngineInitialized += (_, _) => InitializeSession();
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

    private void InitializeSession()
    {
        _session = new DemoSession(_viewport.Engine);
        _session.ModelChanged += (_, _) => RefreshObjectTree();
        _session.HistoryChanged += (_, _) => UpdateHistoryUi();
        _session.StatusChanged += (_, message) =>
        {
            _commandStatus.Text = message;
            Log(message);
        };
        _session.Engine.SetGradientBackground(Color.White, Color.FromArgb(202, 221, 238));
        _session.Engine.SetTriedronVisible(true);
        _session.Engine.SetViewCubeVisible(true);
        ApplyViewCubeLanguage();
        _session.Engine.SetAntialiasing(true);
        _session.Engine.SetAutoZFitMode(true, 1.0);
        _session.Engine.SetSelectionTolerance(4);
        _session.Engine.SetDefaultMaterial(OcctMaterial.Plastified);
        _session.Engine.SetSelectionHighlightColor(_selectionHighlightColor);
        _session.Engine.SetHoverHighlightColor(_hoverHighlightColor);
        _session.Engine.SetSceneLighting(_lightingSettings);
        _commandStatus.Text = DemoLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        _selectionStatus.Text = DemoLocalization.Text("Status.NoneSelected");
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

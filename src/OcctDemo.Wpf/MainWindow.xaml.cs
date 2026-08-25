using System.Globalization;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using Controls = System.Windows.Controls;
using Input = System.Windows.Input;

namespace OcctDemo.Wpf;

public partial class MainWindow : System.Windows.Window
{
    private readonly Dictionary<long, Controls.TreeViewItem> _objectNodes = new();
    private DemoSession? _session;
    private bool _refreshingTree;
    private Controls.ComboBox? _selectionCombo;
    private Controls.MenuItem? _undoMenuItem;
    private Controls.MenuItem? _redoMenuItem;
    private Controls.Button? _undoButton;
    private Controls.Button? _redoButton;
    private bool _autoZFitEnabled = true;
    private DrawingColor _selectionHighlightColor = DrawingColor.FromArgb(255, 155, 0);
    private DrawingColor _hoverHighlightColor = DrawingColor.FromArgb(0, 185, 255);
    private OcctSceneLightingSettings _lightingSettings = OcctLightingPresets.Create(OcctLightingPreset.Neutral);

    public MainWindow()
    {
        InitializeComponent();
        ConfigureViewportContract();
        BuildMenus();
        BuildToolbar();
        WireEvents();
        ApplyLanguage();
    }

    private DemoSession Session => _session ?? throw new InvalidOperationException(
        DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
            ? "OCCT 视口尚未初始化。"
            : "The OCCT viewport has not been initialized.");

    private void ConfigureViewportContract()
    {
        Viewport.InteractionFeatures = OcctViewportInteractionFeatures.Default;
        Viewport.InitialOptions = new OcctViewportInitializationOptions
        {
            BackgroundColor = DrawingColor.White,
            ViewOrientation = OcctViewOrientation.Isometric,
            Projection = OcctProjectionType.Orthographic,
            TriedronVisible = true,
                            ViewCubeVisible = true
        };
    }

    private void WireEvents()
    {
        Viewport.EngineRecreated += (_, args) => InitializeSession(args.Engine, args.Generation);
        Viewport.EngineDisposing += (_, args) =>
        {
            if (_session?.Engine == args.Engine) _session = null;
        };
        Viewport.FirstFrameRendered += (_, args) =>
            Log($"Viewport generation {args.Generation} first frame rendered.");
        Viewport.NativeHandleChanged += (_, args) =>
            Log($"Viewport native handle changed: 0x{args.PreviousHandle.ToInt64():X} -> 0x{args.NativeHandle.ToInt64():X} (generation {args.Generation}).");
        Viewport.Faulted += (_, args) =>
        {
            CommandStatus.Text = args.Exception.Message;
            Log($"VIEWPORT ERROR: {args.Exception.Message}");
        };
        Viewport.PreviewKeyInput += ViewportPreviewKeyInput;
        Viewport.ObjectSelectionChanged += (_, args) => Dispatcher.InvokeAsync(() =>
        {
            if (_session is null) return;
            var singleSelection = args.SelectedObjects.Count == 1 ? args.SelectedObject : null;
            _session.ActiveObject = singleSelection;
            SelectionStatus.Text = args.SelectedObjects.Count == 0
                ? DemoLocalization.Text("Status.NoneSelected")
                : DemoLocalization.Text("Status.Selected", args.SelectedObjects.Count);
            SelectTreeNode(singleSelection);
            ShowSelectionProperties(args.SelectedObjects);
        });
        Viewport.WorldPointChanged += (_, args) => Dispatcher.InvokeAsync(() =>
            CoordinateStatus.Text = $"X {args.WorldPoint.X:F3}  Y {args.WorldPoint.Y:F3}  Z {args.WorldPoint.Z:F3}");
        ObjectTree.SelectedItemChanged += ObjectTreeSelectedItemChanged;
        Closing += MainWindowClosing;
        PreviewKeyDown += MainWindowPreviewKeyDown;
    }

    private void InitializeSession(OcctEngine engine, long generation)
    {
        _session = new DemoSession(engine);
        _session.ModelChanged += (_, _) => Dispatcher.InvokeAsync(RefreshObjectTree);
        _session.HistoryChanged += (_, _) => Dispatcher.InvokeAsync(UpdateHistoryUi);
        _session.StatusChanged += (_, message) => Dispatcher.InvokeAsync(() =>
        {
            CommandStatus.Text = message;
            Log(message);
        });

        using (engine.BeginDisplayBatch())
        {
            engine.SetGradientBackground(DrawingColor.White, DrawingColor.FromArgb(202, 221, 238));
            engine.SetTriedronVisible(true);
            engine.SetTriedronPosition(OcctCornerPosition.LeftLower);
            // Apply the full ViewCube options so the scene starts in sync with the
            // values shown in the View Settings window (size/offset/position).
            ApplyViewCubeOptions(refresh: false);
            ApplyViewCubeLanguage();
            engine.SetAntialiasing(true);
            engine.SetFaceBoundariesVisible(true, applyExisting: true);
            engine.SetAutoZFitMode(true, 1.0);
            engine.SetSelectionTolerance(4);
            engine.SetDefaultMaterial(OcctMaterial.Plastified);
            engine.SetSelectionHighlightColor(_selectionHighlightColor);
            engine.SetHoverHighlightColor(_hoverHighlightColor);
            engine.SetSceneLighting(_lightingSettings);
        }

        CommandStatus.Text = DemoLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        SelectionStatus.Text = DemoLocalization.Text("Status.NoneSelected");
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
            var logPath = CrashReporter.Write("CAD-WPF", exception, "MainWindow.ExecuteSafe");
            var logMessage = string.IsNullOrWhiteSpace(logPath)
                ? string.Empty
                : $"{Environment.NewLine}{Environment.NewLine}Log: {logPath}";
            Log($"ERROR: {exception.Message}");
            System.Windows.MessageBox.Show(this, exception.Message + logMessage,
                DemoLocalization.Text("Dialog.ErrorTitle"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void Log(string message)
    {
        LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        LogBox.ScrollToEnd();
    }

    private static string MenuHeader(string key) =>
        DemoLocalization.Text(key).Replace("&", "_", StringComparison.Ordinal);

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

    private Controls.Button CommandButton(string text, DemoCommandId command)
    {
        var button = new Controls.Button { Content = text, Tag = command, ToolTip = text };
        button.Click += (_, _) => RunCommand((DemoCommandId)button.Tag!);
        return button;
    }

    private static Controls.TreeViewItem TreeRoot(string header) => new() { Header = header };

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

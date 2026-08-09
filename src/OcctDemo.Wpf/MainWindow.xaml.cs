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

    private DemoSession Session => _session ?? throw new InvalidOperationException(
        DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified
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
                ? DemoLocalization.Text("Status.NoneSelected")
                : DemoLocalization.Text("Status.Selected", args.SelectedObjects.Count);
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
            _session = new DemoSession(Viewport.Engine);
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
            ApplyViewCubeLanguage();
            _session.Engine.SetAntialiasing(true);
            _session.Engine.SetFaceBoundariesVisible(true, applyExisting: true);
            _session.Engine.SetAutoZFitMode(true, 1.0);
            _session.Engine.SetSelectionTolerance(4);
            _session.Engine.SetDefaultMaterial(OcctMaterial.Plastified);
            _session.Engine.SetSelectionHighlightColor(_selectionHighlightColor);
            _session.Engine.SetHoverHighlightColor(_hoverHighlightColor);
            _session.Engine.SetSceneLighting(_lightingSettings);
        });

        CommandStatus.Text = DemoLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        SelectionStatus.Text = DemoLocalization.Text("Status.NoneSelected");
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

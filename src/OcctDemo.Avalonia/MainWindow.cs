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
using Button = Avalonia.Controls.Button;
using CheckBox = Avalonia.Controls.CheckBox;
using ContextMenu = Avalonia.Controls.ContextMenu;
using Menu = Avalonia.Controls.Menu;
using MenuItem = Avalonia.Controls.MenuItem;
using ComboBox = Avalonia.Controls.ComboBox;
using Control = Avalonia.Controls.Control;
using GroupBox = Avalonia.Controls.GroupBox;
using TextBox = Avalonia.Controls.TextBox;
using TreeView = Avalonia.Controls.TreeView;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow : Window
{
    private static readonly AvaloniaFontFamily UiFontFamily = OperatingSystem.IsWindows()
        ? new AvaloniaFontFamily("Microsoft YaHei UI")
        : new AvaloniaFontFamily("Inter");

    private readonly Dictionary<long, TreeViewItem> _objectNodes = new();
    private readonly OcctAvaloniaViewport _viewport;
    private readonly Menu _mainMenu;
    private readonly StackPanel _toolbar;
    private readonly TreeView _objectTree;
    private readonly StackPanel _propertyPanel;
    private readonly TextBox _logBox;
    private readonly TextBlock _commandStatus;
    private readonly TextBlock _selectionStatus;
    private readonly TextBlock _coordinateStatus;
    private readonly GroupBox _modelExplorerGroup;
    private readonly GroupBox _propertiesGroup;
    private readonly GroupBox _commandLineGroup;

    private DemoSession? _session;
    private bool _refreshingTree;
    private ComboBox? _selectionCombo;
    private MenuItem? _undoMenuItem;
    private MenuItem? _redoMenuItem;
    private Button? _undoButton;
    private Button? _redoButton;
    private bool _autoZFitEnabled = true;
    private bool _closeApproved;
    private bool _closePromptActive;
    private DrawingColor _selectionHighlightColor = DrawingColor.FromArgb(255, 155, 0);
    private DrawingColor _hoverHighlightColor = DrawingColor.FromArgb(0, 185, 255);
    private OcctSceneLightingSettings _lightingSettings = OcctLightingPresets.Create(OcctLightingPreset.Studio);

    public MainWindow()
    {
        Title = "OCCT CAD - Avalonia";
        Width = 1450;
        Height = 850;
        MinWidth = 1180;
        MinHeight = 720;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowState = WindowState.Maximized;
        Background = new SolidColorBrush(AvaloniaColor.Parse("#EEF1F4"));
        FontFamily = UiFontFamily;

        _mainMenu = new Menu();
        _toolbar = new StackPanel
        {
            Orientation = AvaloniaOrientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(6, 4)
        };
        _viewport = new OcctAvaloniaViewport
        {
            EnableDefaultInteraction = true,
            EnableRectangleSelection = true,
            RectangleSelectionThreshold = 5,
            RectangleSelectionBehavior = OcctRectangleSelectionBehavior.Directional,
            SynchronizeRenderDpi = true
        };
        _objectTree = new TreeView();
        _propertyPanel = new StackPanel { Spacing = 1, Margin = new Thickness(2) };
        _logBox = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            FontFamily = UiFontFamily,
            FontSize = 12,
            Background = AvaloniaBrushes.White,
            Foreground = new SolidColorBrush(AvaloniaColor.Parse("#20262C")),
            BorderBrush = new SolidColorBrush(AvaloniaColor.Parse("#CBD1D6"))
        };
        _commandStatus = new TextBlock { MinWidth = 320, VerticalAlignment = VerticalAlignment.Center };
        _selectionStatus = new TextBlock { MinWidth = 170, VerticalAlignment = VerticalAlignment.Center };
        _coordinateStatus = new TextBlock
        {
            Text = "X 0.000  Y 0.000  Z 0.000",
            FontFamily = UiFontFamily,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        _modelExplorerGroup = new GroupBox();
        _propertiesGroup = new GroupBox();
        _commandLineGroup = new GroupBox();

        Content = BuildLayout();
        BuildMenus();
        BuildToolbar();
        WireEvents();
        ApplyLanguage();
    }

    private DemoSession Session => _session ?? throw new InvalidOperationException(
        Local("The OCCT viewport has not been initialized.", "OCCT 视口尚未初始化。"));

    private void WireEvents()
    {
        _viewport.EngineInitialized += (_, _) => Dispatcher.UIThread.Post(InitializeSession, DispatcherPriority.Background);
        _viewport.ObjectSelectionChanged += (_, args) => Dispatcher.UIThread.Post(() =>
        {
            if (_session is null) return;
            var singleSelection = args.SelectedObjects.Count == 1 ? args.SelectedObject : null;
            _session.ActiveObject = singleSelection;
            _selectionStatus.Text = args.SelectedObjects.Count == 0
                ? DemoLocalization.Text("Status.NoneSelected")
                : DemoLocalization.Text("Status.Selected", args.SelectedObjects.Count);
            SelectTreeNode(singleSelection);
            ShowSelectionProperties(args.SelectedObjects);
        });
        _viewport.WorldPointChanged += (_, args) => Dispatcher.UIThread.Post(() =>
            _coordinateStatus.Text = $"X {args.WorldPoint.X:F3}  Y {args.WorldPoint.Y:F3}  Z {args.WorldPoint.Z:F3}");
        _viewport.ErrorOccurred += (_, args) => Dispatcher.UIThread.Post(() =>
        {
            _commandStatus.Text = args.Exception.Message;
            Log($"ERROR: {args.Exception.Message}");
        });
        _objectTree.SelectionChanged += ObjectTreeSelectionChanged;
        Closing += MainWindowClosing;
        KeyDown += MainWindowKeyDown;
        Opened += (_, _) => Dispatcher.UIThread.Post(_viewport.RefreshNativeView, DispatcherPriority.Background);
    }

    private void InitializeSession()
    {
        if (_session is not null) return;
        try
        {
            _session = new DemoSession(_viewport.Engine);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        _session.ModelChanged += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            RefreshObjectTree();
            UpdateHistoryUi();
            _viewport.RefreshNativeView();
        });
        _session.HistoryChanged += (_, _) => Dispatcher.UIThread.Post(UpdateHistoryUi);
        _session.StatusChanged += (_, message) => Dispatcher.UIThread.Post(() =>
        {
            _commandStatus.Text = message;
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
            var logPath = CrashReporter.Write("CAD-Avalonia", exception, "MainWindow.ExecuteSafe");
            var logMessage = string.IsNullOrWhiteSpace(logPath) ? string.Empty : $"{Environment.NewLine}{Environment.NewLine}Log: {logPath}";
            Log($"ERROR: {exception.Message}");
            if (IsVisible)
                _ = DialogService.ShowMessageAsync(this, DemoLocalization.Text("Dialog.ErrorTitle"), exception.Message + logMessage);
            else
                Program.Trace($"UI error before window became visible: {exception}");
        }
    }

    private void Log(string message)
    {
        _logBox.Text = (_logBox.Text ?? string.Empty) + $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
        _logBox.CaretIndex = _logBox.Text.Length;
    }

    private static MenuItem Menu(string header, params object[] items) => new()
    {
        Header = header,
        ItemsSource = items
    };

    private MenuItem MenuItem(string header, Action action, KeyGesture? shortcut = null)
    {
        var item = new MenuItem { Header = header, InputGesture = shortcut };
        item.Click += (_, _) => ExecuteSafe(action);
        return item;
    }

    private MenuItem AsyncMenuItem(string header, Func<Task> action, KeyGesture? shortcut = null)
    {
        var item = new MenuItem { Header = header, InputGesture = shortcut };
        item.Click += async (_, _) =>
        {
            try
            {
                await action();
            }
            catch (Exception exception)
            {
                ExecuteSafe(() => throw exception);
            }
        };
        return item;
    }

    private static MenuItem CheckMenuItem(string header, bool checkedValue, Action<MenuItem> handler, bool radio = false, string? groupName = null)
    {
        var item = new MenuItem
        {
            Header = header,
            ToggleType = radio ? MenuItemToggleType.Radio : MenuItemToggleType.CheckBox,
            IsChecked = checkedValue,
            GroupName = groupName
        };
        item.Click += (_, _) => handler(item);
        return item;
    }

    private static Button CreateToolbarButton(string text) => new()
    {
        Content = text,
        MinWidth = 72,
        Padding = new Thickness(10, 4),
        Margin = new Thickness(1),
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalContentAlignment = VerticalAlignment.Center
    };

    private Button ToolButton(string text, Action action)
    {
        var button = CreateToolbarButton(text);
        AvaloniaToolTip.SetTip(button, text);
        button.Click += (_, _) => ExecuteSafe(action);
        return button;
    }

    private Button AsyncToolButton(string text, Func<Task> action)
    {
        var button = CreateToolbarButton(text);
        AvaloniaToolTip.SetTip(button, text);
        button.Click += async (_, _) =>
        {
            try
            {
                await action();
            }
            catch (Exception exception)
            {
                ExecuteSafe(() => throw exception);
            }
        };
        return button;
    }

    private Button CommandButton(string text, DemoCommandId command)
    {
        var button = CreateToolbarButton(text);
        button.Tag = command;
        AvaloniaToolTip.SetTip(button, text);
        button.Click += async (_, _) => await RunCommandAsync((DemoCommandId)button.Tag!);
        return button;
    }

    private static Border ToolSeparator() => new()
    {
        Width = 1,
        Height = 24,
        Margin = new Thickness(4, 2),
        Background = new SolidColorBrush(AvaloniaColor.Parse("#B8C0C8"))
    };

    private static TreeViewItem TreeRoot(string header, IReadOnlyList<object> items) => new()
    {
        Header = header,
        ItemsSource = items,
        IsExpanded = true
    };

    private static KeyGesture Shortcut(Key key, KeyModifiers modifiers = KeyModifiers.None) => new(key, modifiers);

    private static KeyGesture? ShortcutFromText(string? shortcut)
    {
        if (string.IsNullOrWhiteSpace(shortcut)) return null;
        return shortcut.Trim().ToUpperInvariant() switch
        {
            "CTRL+N" => Shortcut(Key.N, KeyModifiers.Control),
            "CTRL+O" => Shortcut(Key.O, KeyModifiers.Control),
            "CTRL+S" => Shortcut(Key.S, KeyModifiers.Control),
            "CTRL+SHIFT+S" => Shortcut(Key.S, KeyModifiers.Control | KeyModifiers.Shift),
            "CTRL+Z" => Shortcut(Key.Z, KeyModifiers.Control),
            "CTRL+Y" => Shortcut(Key.Y, KeyModifiers.Control),
            "DELETE" => Shortcut(Key.Delete),
            "F" => Shortcut(Key.F),
            "0" => Shortcut(Key.D0),
            "1" => Shortcut(Key.D1),
            "2" => Shortcut(Key.D2),
            "3" => Shortcut(Key.D3),
            _ => null
        };
    }

    private static string MenuHeader(string key) => DemoLocalization.Text(key).Replace("&", "_", StringComparison.Ordinal);
}

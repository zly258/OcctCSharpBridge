using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CadCommon;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using AvaloniaBrushes = Avalonia.Media.Brushes;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaFontFamily = Avalonia.Media.FontFamily;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaloniaOrientation = Avalonia.Layout.Orientation;
using AvaloniaToolTip = Avalonia.Controls.ToolTip;
using Forms = System.Windows.Forms;
using Button = Avalonia.Controls.Button;
using CheckBox = Avalonia.Controls.CheckBox;
using ContextMenu = Avalonia.Controls.ContextMenu;
using MenuItem = Avalonia.Controls.MenuItem;
using ComboBox = Avalonia.Controls.ComboBox;
using Control = Avalonia.Controls.Control;
using GroupBox = Avalonia.Controls.GroupBox;
using TextBox = Avalonia.Controls.TextBox;
using TreeView = Avalonia.Controls.TreeView;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace CadAvalonia;

public sealed class MainWindow : Window
{
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

    private CadSession? _session;
    private bool _refreshingTree;
    private ComboBox? _selectionCombo;
    private MenuItem? _undoMenuItem;
    private MenuItem? _redoMenuItem;
    private Button? _undoButton;
    private Button? _redoButton;
    private bool _autoZFitEnabled = true;
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
            FontFamily = new AvaloniaFontFamily("Consolas"),
            FontSize = 12,
            Background = new SolidColorBrush(AvaloniaColor.Parse("#101820")),
            Foreground = new SolidColorBrush(AvaloniaColor.Parse("#D8E2EA")),
            BorderBrush = new SolidColorBrush(AvaloniaColor.Parse("#48525B"))
        };
        _commandStatus = new TextBlock { MinWidth = 320, VerticalAlignment = VerticalAlignment.Center };
        _selectionStatus = new TextBlock { MinWidth = 170, VerticalAlignment = VerticalAlignment.Center };
        _coordinateStatus = new TextBlock
        {
            Text = "X 0.000  Y 0.000  Z 0.000",
            FontFamily = new AvaloniaFontFamily("Consolas"),
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

    private CadSession Session => _session ?? throw new InvalidOperationException(
        Local("The OCCT viewport has not been initialized.", "OCCT 视口尚未初始化。"));

    private Control BuildLayout()
    {
        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*,Auto")
        };

        Grid.SetRow(_mainMenu, 0);
        root.Children.Add(_mainMenu);

        var toolbarBorder = new Border
        {
            Background = new SolidColorBrush(AvaloniaColor.Parse("#E7EAED")),
            BorderBrush = new SolidColorBrush(AvaloniaColor.Parse("#C7CDD3")),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = new ScrollViewer
            {
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                Content = _toolbar
            }
        };
        Grid.SetRow(toolbarBorder, 1);
        root.Children.Add(toolbarBorder);

        var workspace = new Grid
        {
            Margin = new Thickness(2),
            ColumnDefinitions = new ColumnDefinitions("260,5,*,5,330")
        };
        workspace.ColumnDefinitions[0].MinWidth = 220;
        workspace.ColumnDefinitions[2].MinWidth = 520;
        workspace.ColumnDefinitions[4].MinWidth = 280;

        _modelExplorerGroup.Margin = new Thickness(4);
        _modelExplorerGroup.Padding = new Thickness(4);
        _modelExplorerGroup.Content = _objectTree;
        Grid.SetColumn(_modelExplorerGroup, 0);
        workspace.Children.Add(_modelExplorerGroup);

        var leftSplitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
            Background = new SolidColorBrush(AvaloniaColor.Parse("#C7CDD3"))
        };
        Grid.SetColumn(leftSplitter, 1);
        workspace.Children.Add(leftSplitter);

        var viewportBorder = new Border
        {
            Margin = new Thickness(4),
            BorderBrush = new SolidColorBrush(AvaloniaColor.Parse("#AEB6BE")),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(AvaloniaColor.Parse("#E8EDF2")),
            Child = _viewport
        };
        Grid.SetColumn(viewportBorder, 2);
        workspace.Children.Add(viewportBorder);

        var rightSplitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
            Background = new SolidColorBrush(AvaloniaColor.Parse("#C7CDD3"))
        };
        Grid.SetColumn(rightSplitter, 3);
        workspace.Children.Add(rightSplitter);

        var right = new Grid
        {
            RowDefinitions = new RowDefinitions("3*,5,2*")
        };
        right.RowDefinitions[0].MinHeight = 250;
        right.RowDefinitions[2].MinHeight = 170;

        _propertiesGroup.Margin = new Thickness(4);
        _propertiesGroup.Padding = new Thickness(4);
        _propertiesGroup.Content = new ScrollViewer
        {
            Content = _propertyPanel,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        Grid.SetRow(_propertiesGroup, 0);
        right.Children.Add(_propertiesGroup);

        var horizontalSplitter = new GridSplitter
        {
            Height = 5,
            HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
            Background = new SolidColorBrush(AvaloniaColor.Parse("#C7CDD3"))
        };
        Grid.SetRow(horizontalSplitter, 1);
        right.Children.Add(horizontalSplitter);

        _commandLineGroup.Margin = new Thickness(4);
        _commandLineGroup.Padding = new Thickness(4);
        _commandLineGroup.Content = _logBox;
        Grid.SetRow(_commandLineGroup, 2);
        right.Children.Add(_commandLineGroup);

        Grid.SetColumn(right, 4);
        workspace.Children.Add(right);
        Grid.SetRow(workspace, 2);
        root.Children.Add(workspace);

        var statusGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
            Background = new SolidColorBrush(AvaloniaColor.Parse("#2F3B46")),
            Margin = new Thickness(0),
            MinHeight = 26
        };
        _commandStatus.Foreground = AvaloniaBrushes.White;
        _commandStatus.Margin = new Thickness(8, 3);
        _selectionStatus.Foreground = AvaloniaBrushes.White;
        _selectionStatus.Margin = new Thickness(8, 3);
        _coordinateStatus.Foreground = AvaloniaBrushes.White;
        _coordinateStatus.Margin = new Thickness(8, 3);
        Grid.SetColumn(_commandStatus, 0);
        Grid.SetColumn(_selectionStatus, 1);
        Grid.SetColumn(_coordinateStatus, 2);
        statusGrid.Children.Add(_commandStatus);
        statusGrid.Children.Add(_selectionStatus);
        statusGrid.Children.Add(_coordinateStatus);
        Grid.SetRow(statusGrid, 3);
        root.Children.Add(statusGrid);

        return root;
    }

    private void WireEvents()
    {
        _viewport.EngineInitialized += (_, _) => Dispatcher.UIThread.Post(InitializeSession, DispatcherPriority.Background);
        _viewport.ObjectSelectionChanged += (_, args) => Dispatcher.UIThread.Post(() =>
        {
            if (_session is null) return;
            _session.ActiveObject = args.SelectedObject;
            _selectionStatus.Text = args.SelectedObjects.Count == 0
                ? CadLocalization.Text("Status.NoneSelected")
                : CadLocalization.Text("Status.Selected", args.SelectedObjects.Count);
            SelectTreeNode(args.SelectedObject);
            ShowObjectProperties(args.SelectedObject);
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
            _session = new CadSession(_viewport.Engine);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        _session.ModelChanged += (_, _) => Dispatcher.UIThread.Post(RefreshObjectTree);
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

        _commandStatus.Text = CadLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
        _selectionStatus.Text = CadLocalization.Text("Status.NoneSelected");
        RefreshObjectTree();
        UpdateHistoryUi();
    }

    private void BuildMenus()
    {
        var file = Menu(MenuHeader("Menu.File"),
            MenuItem(CadLocalization.Text("Menu.New"), NewDocument, Shortcut(Key.N, KeyModifiers.Control)),
            MenuItem(CadLocalization.Text("Menu.Open"), OpenDocument, Shortcut(Key.O, KeyModifiers.Control)),
            MenuItem(CadLocalization.Text("Menu.Save"), () => SaveDocument(false), Shortcut(Key.S, KeyModifiers.Control)),
            MenuItem(CadLocalization.Text("Menu.SaveAs"), () => SaveDocument(true), Shortcut(Key.S, KeyModifiers.Control | KeyModifiers.Shift)),
            new Separator(),
            MenuItem(CadLocalization.Text("Menu.Import"), ImportDocument),
            MenuItem(CadLocalization.Text("Menu.ExportSelected"), ExportSelected),
            MenuItem(CadLocalization.Text("Menu.ExportImage"), ExportViewImage),
            new Separator(),
            MenuItem(CadLocalization.Text("Menu.Exit"), Close));

        _undoMenuItem = MenuItem(CadLocalization.Text("Menu.Undo"), Undo, Shortcut(Key.Z, KeyModifiers.Control));
        _redoMenuItem = MenuItem(CadLocalization.Text("Menu.Redo"), Redo, Shortcut(Key.Y, KeyModifiers.Control));
        var editItems = new List<object>
        {
            _undoMenuItem,
            _redoMenuItem,
            new Separator()
        };
        AddCommands(editItems, CadCommandId.Translate, CadCommandId.Rotate, CadCommandId.Scale, CadCommandId.Mirror, CadCommandId.Copy);
        editItems.Add(new Separator());
        AddCommands(editItems, CadCommandId.Delete);
        editItems.Add(MenuItem(CadLocalization.Text("Menu.ClearSelection"), () =>
        {
            Session.Engine.ClearSelection();
            _viewport.RaiseSelectionChanged();
        }));
        editItems.Add(MenuItem(CadLocalization.Text("Menu.ShowAll"), () => Session.Engine.ShowAll()));
        editItems.Add(MenuItem(CadLocalization.Text("Menu.HideAll"), () => Session.Engine.HideAll()));
        var edit = Menu(MenuHeader("Menu.Edit"), editItems.ToArray());

        var drawItems = new List<object>();
        AddCommands(drawItems, CadCommandId.Point, CadCommandId.Line, CadCommandId.Polyline);
        drawItems.Add(new Separator());
        AddCommands(drawItems, CadCommandId.Circle, CadCommandId.ArcThreePoints, CadCommandId.ArcCenter, CadCommandId.Ellipse);
        drawItems.Add(new Separator());
        AddCommands(drawItems, CadCommandId.Rectangle, CadCommandId.Polygon, CadCommandId.Bezier, CadCommandId.BSpline);
        var draw = Menu(MenuHeader("Menu.Draw"), drawItems.ToArray());

        var primitives = new List<object>();
        AddCommands(primitives, CadCommandId.Box, CadCommandId.Cylinder, CadCommandId.Frustum, CadCommandId.Cone,
            CadCommandId.Torus, CadCommandId.Sphere, CadCommandId.Wedge, CadCommandId.Pipe);
        var features = new List<object>();
        AddCommands(features, CadCommandId.Extrude, CadCommandId.Revolve, CadCommandId.Sweep, CadCommandId.Loft);
        var booleans = new List<object>();
        AddCommands(booleans, CadCommandId.Fuse, CadCommandId.Cut, CadCommandId.Common, CadCommandId.Section);
        var details = new List<object>();
        AddCommands(details, CadCommandId.Fillet, CadCommandId.Chamfer, CadCommandId.Offset, CadCommandId.Shell, CadCommandId.Drill);
        var solid = Menu(MenuHeader("Menu.Solid"),
            Menu(MenuHeader("Menu.Primitives"), primitives.ToArray()),
            Menu(MenuHeader("Menu.Features"), features.ToArray()),
            Menu(MenuHeader("Menu.Boolean"), booleans.ToArray()),
            Menu(MenuHeader("Menu.Details"), details.ToArray()));

        var annotateItems = new List<object>();
        AddCommands(annotateItems, CadCommandId.Text);
        annotateItems.Add(new Separator());
        AddCommands(annotateItems, CadCommandId.LengthDimension, CadCommandId.AngleDimension,
            CadCommandId.RadiusDimension, CadCommandId.DiameterDimension);
        var annotate = Menu(MenuHeader("Menu.Annotate"), annotateItems.ToArray());

        var toolItems = new List<object>();
        AddCommands(toolItems, CadCommandId.AnalyzeBounds, CadCommandId.AnalyzeMass, CadCommandId.AnalyzeTopology,
            CadCommandId.AnalyzeDistance, CadCommandId.ValidateShape);
        var tools = Menu(MenuHeader("Menu.Tools"), toolItems.ToArray());

        var sampleItems = new List<object>();
        AddCommands(sampleItems, CadCommandId.DemoElements, CadCommandId.DemoGear, CadCommandId.DemoManifold,
            CadCommandId.DemoTwistedDuct);
        sampleItems.Add(new Separator());
        AddCommands(sampleItems, CadCommandId.DemoBracket, CadCommandId.DemoFlange, CadCommandId.DemoAnnotations);
        var samples = Menu(MenuHeader("Menu.Samples"), sampleItems.ToArray());

        var language = Menu(MenuHeader("Menu.Language"),
            CheckMenuItem(CadLocalization.Text("Menu.English"), CadLocalization.CurrentLanguage == CadLanguage.English,
                _ => SetLanguage(CadLanguage.English), radio: true, groupName: "language"),
            CheckMenuItem(CadLocalization.Text("Menu.Chinese"), CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified,
                _ => SetLanguage(CadLanguage.ChineseSimplified), radio: true, groupName: "language"));

        var help = Menu(MenuHeader("Menu.Help"),
            MenuItem(CadLocalization.Text("Menu.MouseHelp"), ShowMouseHelp),
            MenuItem(CadLocalization.Text("Menu.About"), ShowAbout));

        _mainMenu.ItemsSource = new object[]
        {
            file, edit, draw, solid, annotate, BuildViewMenu(), tools, samples, language, help
        };
        UpdateHistoryUi();
    }

    private MenuItem BuildViewMenu()
    {
        var display = Menu(MenuHeader("Menu.Display"),
            MenuItem(CadLocalization.Text("Menu.Shaded"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)),
            CheckMenuItem(CadLocalization.Text("Menu.ShadedEdges"), true,
                item => ExecuteSafe(() => Session.Engine.SetFaceBoundariesVisible(item.IsChecked))),
            MenuItem(CadLocalization.Text("Menu.Wireframe"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)),
            CheckMenuItem(CadLocalization.Text("Menu.Hlr"), false,
                item => ExecuteSafe(() => Session.Engine.SetComputedHlr(item.IsChecked))),
            CheckMenuItem(CadLocalization.Text("Menu.Antialiasing"), true,
                item => ExecuteSafe(() => Session.Engine.SetAntialiasing(item.IsChecked))),
            CheckMenuItem(CadLocalization.Text("Menu.Triedron"), true,
                item => ExecuteSafe(() => Session.Engine.SetTriedronVisible(item.IsChecked))),
            CheckMenuItem(CadLocalization.Text("Menu.ViewCube"), true,
                item => ExecuteSafe(() => Session.Engine.SetViewCubeVisible(item.IsChecked))));

        var standard = Menu(MenuHeader("Menu.StandardViews"),
            MenuItem(CadLocalization.Text("Menu.Front"), () => Session.Engine.SetView(OcctViewOrientation.Front), Shortcut(Key.D1)),
            MenuItem(CadLocalization.Text("Menu.Back"), () => Session.Engine.SetView(OcctViewOrientation.Back)),
            MenuItem(CadLocalization.Text("Menu.Left"), () => Session.Engine.SetView(OcctViewOrientation.Left), Shortcut(Key.D2)),
            MenuItem(CadLocalization.Text("Menu.Right"), () => Session.Engine.SetView(OcctViewOrientation.Right)),
            MenuItem(CadLocalization.Text("Menu.Top"), () => Session.Engine.SetView(OcctViewOrientation.Top), Shortcut(Key.D3)),
            MenuItem(CadLocalization.Text("Menu.Bottom"), () => Session.Engine.SetView(OcctViewOrientation.Bottom)),
            new Separator(),
            MenuItem(CadLocalization.Text("Menu.Isometric"), () => Session.Engine.SetView(OcctViewOrientation.Isometric), Shortcut(Key.D0)),
            MenuItem(CadLocalization.Text("Menu.NorthEast"), () => Session.SetIsoView(CadIsoView.NorthEast)),
            MenuItem(CadLocalization.Text("Menu.NorthWest"), () => Session.SetIsoView(CadIsoView.NorthWest)),
            MenuItem(CadLocalization.Text("Menu.SouthEast"), () => Session.SetIsoView(CadIsoView.SouthEast)),
            MenuItem(CadLocalization.Text("Menu.SouthWest"), () => Session.SetIsoView(CadIsoView.SouthWest)));

        var projection = Menu(MenuHeader("Menu.Projection"),
            MenuItem(CadLocalization.Text("Menu.Orthographic"), () => Session.Engine.SetProjection(OcctProjectionType.Orthographic)),
            MenuItem(CadLocalization.Text("Menu.Perspective"), () => Session.Engine.SetProjection(OcctProjectionType.Perspective)),
            AsyncMenuItem(CadLocalization.Text("Menu.PerspectiveFov"), SetPerspectiveFovAsync));

        return Menu(MenuHeader("Menu.View"),
            MenuItem(CadLocalization.Text("Menu.FitAll"), () => Session.Engine.FitAll(), Shortcut(Key.F)),
            MenuItem(CadLocalization.Text("Menu.FitSelected"), () =>
            {
                var shape = ActiveShape();
                if (shape is not null) Session.Engine.Fit(shape.Value);
            }),
            new Separator(),
            display,
            BuildDepthMenu(),
            standard,
            projection,
            AsyncMenuItem(CadLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecisionAsync),
            BuildLightingMenu(),
            BuildMaterialMenu(),
            BuildSelectionMenu(),
            BuildSelectionAppearanceMenu(),
            CheckMenuItem(CadLocalization.Text("Menu.WindowSelection"), _viewport.EnableRectangleSelection, item =>
            {
                _viewport.EnableRectangleSelection = item.IsChecked;
                _commandStatus.Text = CadLocalization.Text(item.IsChecked ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
            }),
            AsyncMenuItem(CadLocalization.Text("Menu.SelectionTolerance"), SetSelectionToleranceAsync),
            MenuItem(CadLocalization.Text("Menu.Background"), SetBackgroundColor),
            MenuItem(CadLocalization.Text("Menu.GradientBackground"), () =>
                Session.Engine.SetGradientBackground(DrawingColor.White, DrawingColor.LightSteelBlue)));
    }

    private MenuItem BuildDepthMenu()
    {
        return Menu(MenuHeader("Menu.DepthHandling"),
            CheckMenuItem(CadLocalization.Text("Menu.AutoZFit"), _autoZFitEnabled, item => ExecuteSafe(() =>
            {
                _autoZFitEnabled = item.IsChecked;
                Session.Engine.SetAutoZFitMode(_autoZFitEnabled, 1.0);
                var message = CadLocalization.Text(_autoZFitEnabled ? "Status.AutoZFitOn" : "Status.AutoZFitOff");
                _commandStatus.Text = message;
                Log(message);
            })),
            MenuItem(CadLocalization.Text("Menu.AutoZFitNow"), () => ExecuteSafe(Session.Engine.AutoZFit)),
            new Separator(),
            MenuItem(CadLocalization.Text("Menu.DepthForward"), () => ApplyDepthBias(CadDepthBiasPreset.Forward)),
            MenuItem(CadLocalization.Text("Menu.DepthBackward"), () => ApplyDepthBias(CadDepthBiasPreset.Backward)),
            MenuItem(CadLocalization.Text("Menu.DepthReset"), () => ApplyDepthBias(CadDepthBiasPreset.Default)));
    }

    private MenuItem BuildLightingMenu()
    {
        var items = new List<object>();
        foreach (var preset in Enum.GetValues<OcctLightingPreset>())
        {
            var captured = preset;
            items.Add(MenuItem(LightingPresetName(captured), () => ApplyLightingPreset(captured)));
        }
        items.Add(new Separator());
        items.Add(AsyncMenuItem(Local("Custom Lighting...", "自定义灯光..."), SetAdvancedLightingAsync));
        items.Add(MenuItem(Local("OCCT Default Lights", "恢复 OCCT 默认灯光"), () => ExecuteSafe(Session.Engine.ResetSceneLighting)));
        return Menu(Local("Lighting", "灯光"), items.ToArray());
    }

    private MenuItem BuildMaterialMenu()
    {
        var items = new List<object>();
        foreach (var material in Enum.GetValues<OcctMaterial>())
        {
            var captured = material;
            items.Add(MenuItem(MaterialDisplayName(captured), () =>
            {
                var apply = Forms.MessageBox.Show(
                    CadLocalization.Text("Dialog.ApplyExistingMaterial"),
                    CadLocalization.Text("Menu.Material"),
                    Forms.MessageBoxButtons.YesNo,
                    Forms.MessageBoxIcon.Question) == Forms.DialogResult.Yes;
                ExecuteSafe(() => Session.Engine.SetDefaultMaterial(captured, apply));
                Log($"{CadLocalization.Text("Menu.Material")}: {MaterialDisplayName(captured)}");
            }));
        }
        return Menu(MenuHeader("Menu.Material"), items.ToArray());
    }

    private MenuItem BuildSelectionMenu()
    {
        var items = new List<object>();
        foreach (var mode in Enum.GetValues<OcctSelectionMode>())
        {
            var captured = mode;
            items.Add(MenuItem(SelectionModeName(captured), () => SetSelectionMode(captured)));
        }
        return Menu(MenuHeader("Menu.SelectionMode"), items.ToArray());
    }

    private MenuItem BuildSelectionAppearanceMenu()
    {
        return Menu(Local("Selection Appearance", "选择外观"),
            MenuItem(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColor),
            MenuItem(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColor));
    }

    private void BuildToolbar()
    {
        var selectedIndex = _selectionCombo?.SelectedIndex ?? 0;
        _toolbar.Children.Clear();
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.New"), NewDocument));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Open"), OpenDocument));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Save"), () => SaveDocument(false)));
        _toolbar.Children.Add(ToolSeparator());
        _undoButton = ToolButton(CadLocalization.Text("Toolbar.Undo"), Undo);
        _redoButton = ToolButton(CadLocalization.Text("Toolbar.Redo"), Redo);
        _toolbar.Children.Add(_undoButton);
        _toolbar.Children.Add(_redoButton);
        _toolbar.Children.Add(ToolSeparator());
        _toolbar.Children.Add(CommandButton(CadLocalization.Text("Toolbar.Line"), CadCommandId.Line));
        _toolbar.Children.Add(CommandButton(CadLocalization.Text("Toolbar.Circle"), CadCommandId.Circle));
        _toolbar.Children.Add(CommandButton(CadLocalization.Text("Toolbar.Box"), CadCommandId.Box));
        _toolbar.Children.Add(CommandButton(CadLocalization.Text("Toolbar.Cylinder"), CadCommandId.Cylinder));
        _toolbar.Children.Add(ToolSeparator());
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Shaded"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded)));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Wireframe"), () => Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe)));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Extents"), () => Session.Engine.FitAll()));
        _toolbar.Children.Add(ToolButton(CadLocalization.Text("Toolbar.Isometric"), () => Session.Engine.SetView(OcctViewOrientation.Isometric)));
        _toolbar.Children.Add(ToolSeparator());
        _toolbar.Children.Add(new TextBlock
        {
            Text = CadLocalization.Text("Toolbar.Selection"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 2, 0)
        });
        _selectionCombo = new ComboBox
        {
            Width = 130,
            Margin = new Thickness(2),
            ItemsSource = Enum.GetValues<OcctSelectionMode>().Select(SelectionModeName).ToArray()
        };
        _selectionCombo.SelectedIndex = Math.Clamp(selectedIndex, 0, Enum.GetValues<OcctSelectionMode>().Length - 1);
        _selectionCombo.SelectionChanged += (_, _) =>
        {
            if (_selectionCombo.SelectedIndex >= 0)
                SetSelectionMode((OcctSelectionMode)_selectionCombo.SelectedIndex);
        };
        _toolbar.Children.Add(_selectionCombo);
        UpdateHistoryUi();
    }

    private void AddCommands(ICollection<object> parent, params CadCommandId[] commands)
    {
        foreach (var id in commands)
        {
            var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));
            var item = AsyncMenuItem(definition.Text, () => RunCommandAsync(id), ShortcutFromText(definition.Shortcut));
            AvaloniaToolTip.SetTip(item, definition.Description);
            parent.Add(item);
        }
    }

    private async Task RunCommandAsync(CadCommandId id)
    {
        if (_session is null) return;

        var availability = _session.GetCommandAvailability(id);
        if (!availability.CanExecute)
        {
            ReportCommandPrecondition(availability.Message);
            return;
        }

        var definition = CadLocalization.Localize(CadCommandCatalog.Get(id));
        var input = await ParameterDialog.GetValuesAsync(this, definition.Text, definition.Parameters);
        if (!input.Accepted) return;

        ExecuteSafe(() =>
        {
            var result = Session.Execute(id, input.Values);
            if (!string.IsNullOrWhiteSpace(result.AnalysisText))
            {
                Log(result.AnalysisText);
                Forms.MessageBox.Show(result.AnalysisText, definition.Text, Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
            }
            RefreshObjectTree();
        });
    }

    private void ReportCommandPrecondition(string message)
    {
        _commandStatus.Text = message;
        Log(message);
        System.Media.SystemSounds.Asterisk.Play();
        _viewport.Focus();
    }

    private void NewDocument()
    {
        if (!ConfirmDiscardChanges()) return;
        ExecuteSafe(Session.NewDocument);
    }

    private void OpenDocument()
    {
        if (!ConfirmDiscardChanges()) return;
        using var dialog = new Forms.OpenFileDialog
        {
            Filter = CadFileFilter(),
            Title = CadLocalization.Text("Dialog.OpenTitle"),
            Multiselect = false
        };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() => Session.Open(dialog.FileName));
    }

    private void ImportDocument()
    {
        using var dialog = new Forms.OpenFileDialog
        {
            Filter = CadFileFilter(),
            Title = CadLocalization.Text("Dialog.ImportTitle"),
            Multiselect = true
        };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
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
            using var dialog = new Forms.SaveFileDialog
            {
                Filter = SaveFileFilter(),
                Title = CadLocalization.Text("Dialog.SaveTitle"),
                DefaultExt = "step",
                AddExtension = true
            };
            if (dialog.ShowDialog() != Forms.DialogResult.OK) return false;
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
        using var dialog = new Forms.SaveFileDialog
        {
            Filter = SaveFileFilter(),
            Title = CadLocalization.Text("Dialog.ExportTitle"),
            DefaultExt = "step",
            AddExtension = true
        };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() => Session.ExportSelected(dialog.FileName));
    }

    private void ExportViewImage()
    {
        using var dialog = new Forms.SaveFileDialog
        {
            Filter = Local("PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp", "PNG 图片|*.png|JPEG 图片|*.jpg;*.jpeg|BMP 图片|*.bmp"),
            Title = CadLocalization.Text("Dialog.ExportImageTitle"),
            DefaultExt = "png",
            AddExtension = true
        };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            Session.Engine.DumpView(dialog.FileName);
            Log(Local($"View image exported: {dialog.FileName}", $"已导出视图图片：{dialog.FileName}"));
        });
    }

    private async Task SetPerspectiveFovAsync()
    {
        var parameters = new[]
        {
            new CadParameterDefinition("fov", Local("Vertical Field of View", "垂直视场角"), CadParameterKind.Number, "45", "°")
        };
        var input = await ParameterDialog.GetValuesAsync(this, CadLocalization.Text("Menu.PerspectiveFov"), parameters);
        if (!input.Accepted) return;
        ExecuteSafe(() => Session.Engine.SetPerspectiveFieldOfView(new CadValues(input.Values).Number("fov", 45)));
    }

    private async Task SetDisplayPrecisionAsync()
    {
        var parameters = new[]
        {
            new CadParameterDefinition("coefficient", Local("Deviation Coefficient", "离散偏差系数"), CadParameterKind.Number, "0.001"),
            new CadParameterDefinition("angle", Local("Angular Deflection", "角度偏差"), CadParameterKind.Number, "12", "°"),
            new CadParameterDefinition("existing", Local("Apply to Existing Objects", "应用到现有对象"), CadParameterKind.Boolean, "true")
        };
        var input = await ParameterDialog.GetValuesAsync(this, CadLocalization.Text("Menu.DisplayPrecision"), parameters);
        if (!input.Accepted) return;
        var values = new CadValues(input.Values);
        ExecuteSafe(() => Session.Engine.SetDisplayPrecision(values.Number("coefficient"), values.Number("angle"), values.Boolean("existing", true)));
    }

    private async Task SetAdvancedLightingAsync()
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
        var input = await ParameterDialog.GetValuesAsync(this, Local("Custom Lighting", "自定义灯光"), parameters);
        if (!input.Accepted) return;
        var values = new CadValues(input.Values);
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

    private void SetSelectionHighlightColor()
    {
        using var dialog = new Forms.ColorDialog { Color = _selectionHighlightColor, FullOpen = true };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _selectionHighlightColor = dialog.Color;
            Session.Engine.SetSelectionHighlightColor(dialog.Color);
        });
    }

    private void SetHoverHighlightColor()
    {
        using var dialog = new Forms.ColorDialog { Color = _hoverHighlightColor, FullOpen = true };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        ExecuteSafe(() =>
        {
            _hoverHighlightColor = dialog.Color;
            Session.Engine.SetHoverHighlightColor(dialog.Color);
        });
    }

    private async Task SetSelectionToleranceAsync()
    {
        var parameters = new[]
        {
            new CadParameterDefinition("pixels", Local("Aperture Size", "像素容差"), CadParameterKind.Integer, "4", "px")
        };
        var input = await ParameterDialog.GetValuesAsync(this, CadLocalization.Text("Menu.SelectionTolerance"), parameters);
        if (!input.Accepted) return;
        ExecuteSafe(() => Session.Engine.SetSelectionTolerance(new CadValues(input.Values).Integer("pixels", 4)));
    }

    private void SetBackgroundColor()
    {
        using var dialog = new Forms.ColorDialog { Color = DrawingColor.White, FullOpen = true };
        if (dialog.ShowDialog() == Forms.DialogResult.OK)
            ExecuteSafe(() => Session.Engine.SetBackground(dialog.Color));
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

    private void RefreshObjectTree()
    {
        if (_session is null) return;
        _refreshingTree = true;
        try
        {
            _objectNodes.Clear();
            var shapeItems = new List<object>();
            var textItems = new List<object>();
            var dimensionItems = new List<object>();

            foreach (var value in Session.Engine.Objects)
            {
                var visible = new CheckBox
                {
                    Content = Session.SafeName(value),
                    IsChecked = true,
                    Tag = value,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                visible.IsCheckedChanged += (_, _) =>
                {
                    if (_refreshingTree || _session is null) return;
                    ExecuteSafe(() => Session.Engine.SetVisible(value, visible.IsChecked == true));
                };

                var item = new TreeViewItem
                {
                    Header = visible,
                    Tag = value,
                    ContextMenu = BuildObjectContextMenu(value)
                };
                _objectNodes[value.Id] = item;
                switch (value.Kind)
                {
                    case OcctObjectKind.Text:
                        textItems.Add(item);
                        break;
                    case OcctObjectKind.Dimension:
                        dimensionItems.Add(item);
                        break;
                    default:
                        shapeItems.Add(item);
                        break;
                }
            }

            var shapeRoot = TreeRoot(Local("Shapes", "形体"), shapeItems);
            var textRoot = TreeRoot(Local("Text", "文字"), textItems);
            var dimensionRoot = TreeRoot(Local("Dimensions", "尺寸"), dimensionItems);
            _objectTree.ItemsSource = new object[] { shapeRoot, textRoot, dimensionRoot };
        }
        finally
        {
            _refreshingTree = false;
        }

        ShowObjectProperties(Session.ActiveObject);
        _selectionStatus.Text = Local(
            $"Objects {Session.Engine.ObjectCount} / Shapes {Session.Engine.ShapeCount}",
            $"对象 {Session.Engine.ObjectCount} / 形体 {Session.Engine.ShapeCount}");
    }

    private ContextMenu BuildObjectContextMenu(IOcctObject value)
    {
        return new ContextMenu
        {
            ItemsSource = new object[]
            {
                MenuItem(CadLocalization.Text("Menu.FitSelected"), () =>
                {
                    Session.ActiveObject = value;
                    if (value.Kind == OcctObjectKind.Shape) Session.Engine.Fit(new OcctShape(value.Id));
                }),
                MenuItem(Local("Show", "显示"), () => Session.Engine.SetVisible(value, true)),
                MenuItem(Local("Hide", "隐藏"), () => Session.Engine.SetVisible(value, false)),
                MenuItem(Local("Color...", "颜色..."), () => SetObjectColor(value)),
                AsyncMenuItem(Local("Material...", "材质..."), () => SetObjectMaterialAsync(value)),
                new Separator(),
                AsyncMenuItem(CadLocalization.CommandText(CadCommandId.Delete), async () =>
                {
                    Session.ActiveObject = value;
                    await RunCommandAsync(CadCommandId.Delete);
                })
            }
        };
    }

    private void ObjectTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_refreshingTree || _session is null || _objectTree.SelectedItem is not TreeViewItem { Tag: IOcctObject value }) return;
        Session.ActiveObject = value;
        Session.Engine.SelectObject(value, false);
        _viewport.RaiseSelectionChanged();
        ShowObjectProperties(value);
        _selectionStatus.Text = Local($"Current: {Session.SafeName(value)}", $"当前：{Session.SafeName(value)}");
    }

    private void ShowObjectProperties(IOcctObject? value)
    {
        _propertyPanel.Children.Clear();
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2*,3*"),
            Background = new SolidColorBrush(AvaloniaColor.Parse("#E7EAED")),
            Margin = new Thickness(0, 0, 0, 2)
        };
        var nameHeader = new TextBlock { Text = CadLocalization.Text("Property.Name"), FontWeight = FontWeight.SemiBold, Margin = new Thickness(6, 4) };
        var valueHeader = new TextBlock { Text = CadLocalization.Text("Property.Value"), FontWeight = FontWeight.SemiBold, Margin = new Thickness(6, 4) };
        Grid.SetColumn(valueHeader, 1);
        header.Children.Add(nameHeader);
        header.Children.Add(valueHeader);
        _propertyPanel.Children.Add(header);

        if (value is null || _session is null) return;
        foreach (var property in Session.DescribeObject(value.Value))
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("2*,3*"),
                Background = AvaloniaBrushes.White,
                Margin = new Thickness(0, 0, 0, 1)
            };
            var name = new TextBlock { Text = property.Key, Margin = new Thickness(6, 4), TextWrapping = TextWrapping.Wrap };
            var propertyValue = new TextBlock { Text = property.Value, Margin = new Thickness(6, 4), TextWrapping = TextWrapping.Wrap };
            Grid.SetColumn(propertyValue, 1);
            row.Children.Add(name);
            row.Children.Add(propertyValue);
            _propertyPanel.Children.Add(row);
        }
    }

    private void SelectTreeNode(IOcctObject? value)
    {
        if (value is null || !_objectNodes.TryGetValue(value.Value.Id, out var item)) return;
        item.IsSelected = true;
        item.BringIntoView();
    }

    private void SetObjectColor(IOcctObject value)
    {
        using var dialog = new Forms.ColorDialog { Color = DrawingColor.SteelBlue, FullOpen = true };
        if (dialog.ShowDialog() == Forms.DialogResult.OK)
            ExecuteSafe(() => Session.Engine.SetColor(value, dialog.Color));
    }

    private async Task SetObjectMaterialAsync(IOcctObject value)
    {
        if (value.Kind != OcctObjectKind.Shape) return;
        var options = Enum.GetValues<OcctMaterial>().Select(MaterialDisplayName).ToArray();
        var parameters = new[]
        {
            new CadParameterDefinition("material", Local("Material", "材质"), CadParameterKind.Choice,
                MaterialDisplayName(OcctMaterial.Steel), null, options)
        };
        var input = await ParameterDialog.GetValuesAsync(this, Local("Object Material", "对象材质"), parameters);
        if (!input.Accepted) return;
        var name = new CadValues(input.Values).Text("material");
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
        var answer = Forms.MessageBox.Show(
            CadLocalization.Text("Dialog.ConfirmDiscard"),
            CadLocalization.Text("Dialog.ConfirmDiscardTitle"),
            Forms.MessageBoxButtons.YesNoCancel,
            Forms.MessageBoxIcon.Question);
        if (answer == Forms.DialogResult.Cancel) return false;
        if (answer == Forms.DialogResult.Yes) return SaveDocument(false);
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
        FontFamily = new AvaloniaFontFamily(CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified ? "Microsoft YaHei UI" : "Segoe UI");
        Title = "OCCT CAD - Avalonia";
        _modelExplorerGroup.Header = CadLocalization.Text("Panel.ModelExplorer");
        _propertiesGroup.Header = CadLocalization.Text("Panel.Properties");
        _commandLineGroup.Header = CadLocalization.Text("Panel.CommandLine");
        if (_session is null)
        {
            _commandStatus.Text = CadLocalization.Text("Status.Initializing");
            _selectionStatus.Text = CadLocalization.Text("Status.NoneSelected");
        }
        else
        {
            _commandStatus.Text = CadLocalization.Text("Status.Ready", OcctEngine.OcctVersion);
            ExecuteSafe(ApplyViewCubeLanguage);
        }
        BuildMenus();
        BuildToolbar();
        RefreshObjectTree();
        ShowObjectProperties(_session?.ActiveObject);
    }

    private void ApplyViewCubeLanguage()
    {
        if (_session is null) return;
        _session.Engine.SetViewCubeLanguage(
            CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified
                ? OcctViewCubeLanguage.ChineseSimplified
                : OcctViewCubeLanguage.English);
    }

    private void MainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!ConfirmDiscardChanges()) e.Cancel = true;
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
            NewDocument();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.O)
        {
            OpenDocument();
            e.Handled = true;
        }
        else if (modifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.S)
        {
            SaveDocument(modifiers.HasFlag(KeyModifiers.Shift));
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            await RunCommandAsync(CadCommandId.Delete);
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
            Forms.MessageBox.Show(exception.Message + logMessage,
                CadLocalization.Text("Dialog.ErrorTitle"), Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
        }
    }

    private void Log(string message)
    {
        _logBox.Text = (_logBox.Text ?? string.Empty) + $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
        _logBox.CaretIndex = _logBox.Text.Length;
    }

    private void ShowMouseHelp()
    {
        Forms.MessageBox.Show(CadLocalization.Text("Dialog.MouseText"), CadLocalization.Text("Menu.MouseHelp"),
            Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
    }

    private void ShowAbout()
    {
        var text = Local(
            "OCCT CAD demonstration application\nOpen CASCADE Technology 7.9.0\nWinForms / WPF / Avalonia native viewport bridge\n\nRepository: https://github.com/zly258/OcctCSharpBridge\nLicense: PolyForm Noncommercial License 1.0.0\nAuthor: Liaoyuan Zhang\nEmail: zhangly1403@gmail.com",
            "OCCT CAD 演示应用\nOpen CASCADE Technology 7.9.0\nWinForms / WPF / Avalonia 原生视口桥接\n\n仓库：https://github.com/zly258/OcctCSharpBridge\n许可证：PolyForm Noncommercial License 1.0.0\n作者：Liaoyuan Zhang\n邮箱：zhangly1403@gmail.com");
        Forms.MessageBox.Show(text, CadLocalization.Text("Menu.About"), Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
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

    private Button ToolButton(string text, Action action)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(9, 4),
            Margin = new Thickness(1)
        };
        AvaloniaToolTip.SetTip(button, text);
        button.Click += (_, _) => ExecuteSafe(action);
        return button;
    }

    private Button CommandButton(string text, CadCommandId command)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(9, 4),
            Margin = new Thickness(1),
            Tag = command
        };
        AvaloniaToolTip.SetTip(button, text);
        button.Click += async (_, _) => await RunCommandAsync((CadCommandId)button.Tag!);
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

    private static string MenuHeader(string key) => CadLocalization.Text(key).Replace("&", "_", StringComparison.Ordinal);

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

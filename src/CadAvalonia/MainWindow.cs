using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using OcctNet;
using DrawingColor = System.Drawing.Color;

namespace CadAvalonia;

public sealed class MainWindow : Window
{
    private readonly OcctAvaloniaViewport _viewport;
    private readonly TextBlock _status;
    private int _extraShapeIndex;

    public MainWindow()
    {
        Title = "OcctCSharpBridge - Avalonia Demo";
        Width = 1280;
        Height = 800;
        MinWidth = 900;
        MinHeight = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _viewport = new OcctAvaloniaViewport
        {
            EnableDefaultInteraction = true,
            EnableRectangleSelection = true,
            RectangleSelectionBehavior = OcctRectangleSelectionBehavior.Directional,
            SynchronizeRenderDpi = true
        };
        _status = new TextBlock
        {
            Text = "Initializing OCCT / 正在初始化 OCCT...",
            Margin = new Thickness(12, 7),
            VerticalAlignment = VerticalAlignment.Center
        };

        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(10, 8)
        };
        toolbar.Children.Add(CreateButton("Fit / 适配", () => _viewport.Engine.FitAll()));
        toolbar.Children.Add(CreateButton("Front / 前", () => _viewport.Engine.SetZUpView(OcctZUpViewOrientation.Front)));
        toolbar.Children.Add(CreateButton("Top / 顶", () => _viewport.Engine.SetZUpView(OcctZUpViewOrientation.Top)));
        toolbar.Children.Add(CreateButton("Right / 右", () => _viewport.Engine.SetZUpView(OcctZUpViewOrientation.Right)));
        toolbar.Children.Add(CreateButton("Iso / 轴测", () => _viewport.Engine.SetZUpView(OcctZUpViewOrientation.IsometricXPositiveYNegative)));
        toolbar.Children.Add(CreateButton("Add Box / 加方块", AddBox));
        Grid.SetRow(toolbar, 0);
        root.Children.Add(toolbar);

        _viewport.EngineInitialized += (_, _) =>
            Dispatcher.UIThread.Post(InitializeScene, DispatcherPriority.Background);
        _viewport.ObjectSelectionChanged += (_, args) =>
        {
            _status.Text = args.SelectedObject is { } selected
                ? $"Selected / 已选择: {selected.Kind} #{selected.Id}"
                : "Selected / 已选择: none / 无";
        };
        _viewport.WorldPointChanged += (_, args) =>
        {
            if (_viewport.Engine.SelectedObjects.Count == 0)
                _status.Text = $"World / 世界坐标: {args.WorldPoint.X:F2}, {args.WorldPoint.Y:F2}, {args.WorldPoint.Z:F2}";
        };
        _viewport.ErrorOccurred += (_, args) => _status.Text = $"Error / 错误: {args.Exception.Message}";
        Grid.SetRow(_viewport, 1);
        root.Children.Add(_viewport);

        var statusBorder = new Border
        {
            BorderThickness = new Thickness(0, 1, 0, 0),
            BorderBrush = Brushes.LightGray,
            Child = _status
        };
        Grid.SetRow(statusBorder, 2);
        root.Children.Add(statusBorder);

        Content = root;
        Opened += (_, _) => Dispatcher.UIThread.Post(_viewport.RefreshNativeView, DispatcherPriority.Background);
    }

    private Button CreateButton(string text, Action action)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(12, 6)
        };
        button.Click += (_, _) => Run(action);
        return button;
    }

    private void InitializeScene()
    {
        Run(() =>
        {
            var engine = _viewport.Engine;
            engine.SetGradientBackground(
                DrawingColor.FromArgb(247, 249, 252),
                DrawingColor.FromArgb(218, 230, 242),
                OcctGradientFillMethod.Vertical);
            engine.SetTriedronVisible(true);
            engine.SetViewCubeVisible(true);
            engine.SetViewCubeLanguage(OcctViewCubeLanguage.English);
            engine.SetAntialiasing(true);

            var box = engine.MakeBox(80, 60, 40, -40, -30, 0);
            engine.SetColor(box, DrawingColor.SteelBlue);

            var cylinder = engine.MakeCylinder(18, 70, 60, 0, 0);
            engine.SetColor(cylinder, DrawingColor.OrangeRed);

            var sphere = engine.MakeSphere(24, -70, 10, 30);
            engine.SetColor(sphere, DrawingColor.SeaGreen);

            engine.SetZUpView(OcctZUpViewOrientation.IsometricXPositiveYNegative);
            engine.FitAll();
            _status.Text = "Ready / 就绪 - LMB select, drag box, RMB rotate, MMB pan, wheel zoom";
        });
    }

    private void AddBox()
    {
        var engine = _viewport.Engine;
        var index = _extraShapeIndex++;
        var box = engine.MakeBox(24, 24, 24, -100 + index * 30, 70, 0);
        engine.SetColor(box, DrawingColor.MediumPurple);
        engine.FitAll();
    }

    private void Run(Action action)
    {
        try
        {
            if (!_viewport.IsEngineInitialized)
            {
                _status.Text = "OCCT viewport is not initialized / OCCT 视口尚未初始化";
                return;
            }
            action();
        }
        catch (Exception exception)
        {
            _status.Text = $"Error / 错误: {exception.Message}";
        }
    }
}

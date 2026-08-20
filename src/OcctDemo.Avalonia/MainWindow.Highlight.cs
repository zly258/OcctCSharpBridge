using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using OcctDemo.Common;
using OcctNet;
using DrawingColor = System.Drawing.Color;
using MenuItem = Avalonia.Controls.MenuItem;

namespace OcctDemo.Avalonia;

public sealed partial class MainWindow
{
    private OcctDisplayMode _displayMode = OcctDisplayMode.Shaded;
    private OcctProjectionType _projectionType = OcctProjectionType.Orthographic;
    private OcctHighlightMode _selectionHighlightMode = OcctHighlightMode.Wireframe;
    private OcctHighlightMode _hoverHighlightMode = OcctHighlightMode.Wireframe;
    private OcctCornerPosition _triedronPosition = OcctCornerPosition.LeftLower;
    private OcctCornerPosition _viewCubePosition = OcctCornerPosition.RightUpper;
    private Window? _viewSettingsWindow;

    private MenuItem BuildSelectionHighlightModeMenu() =>
        BuildHighlightModeMenu(
            Local("Selected Mode", "选中高亮模式"),
            "selection-highlight-mode",
            _selectionHighlightMode,
            SetSelectionHighlightMode);

    private MenuItem BuildHoverHighlightModeMenu() =>
        BuildHighlightModeMenu(
            Local("Hover Mode", "悬浮高亮模式"),
            "hover-highlight-mode",
            _hoverHighlightMode,
            SetHoverHighlightMode);

    private MenuItem BuildViewHelpersMenu()
    {
        var items = new List<object>
        {
            BuildCornerPositionMenu(
                Local("Triedron Position", "坐标轴位置"),
                "triedron-position",
                _triedronPosition,
                SetTriedronPosition),
            BuildCornerPositionMenu(
                Local("ViewCube Position", "ViewCube 位置"),
                "viewcube-position",
                _viewCubePosition,
                SetViewCubePosition),
            new Separator(),
            MenuItem(Local("ViewCube Small", "ViewCube 小"), () => SetViewCubeSize(72)),
            MenuItem(Local("ViewCube Normal", "ViewCube 正常"), () => SetViewCubeSize(90)),
            MenuItem(Local("ViewCube Large", "ViewCube 大"), () => SetViewCubeSize(120)),
            MenuItem(Local("ViewCube Offset 10 px", "ViewCube 偏移 10 px"), () => SetViewCubeOffset(10, 10)),
            MenuItem(Local("ViewCube Offset 20 px", "ViewCube 偏移 20 px"), () => SetViewCubeOffset(20, 20))
        };
        return Menu(Local("View Helpers", "视图辅助"), items.ToArray());
    }

    private void ShowViewSettingsWindow()
    {
        if (_viewSettingsWindow is { IsVisible: true })
        {
            _viewSettingsWindow.Activate();
            return;
        }

        var tabs = new TabControl
        {
            ItemsSource = new object[]
            {
                SettingsTab(Local("View", "视图"),
                    Button(DemoLocalization.Text("Menu.FitAll"), () => Session.Engine.FitAll()),
                    Button(DemoLocalization.Text("Menu.FitSelected"), () => { var shape = ActiveShape(); if (shape is not null) Session.Engine.Fit(shape.Value); }),
                    Button(DemoLocalization.Text("Menu.Orthographic"), () => SetProjectionMode(OcctProjectionType.Orthographic)),
                    Button(DemoLocalization.Text("Menu.Perspective"), () => SetProjectionMode(OcctProjectionType.Perspective)),
                    AsyncButton(DemoLocalization.Text("Menu.PerspectiveFov"), SetPerspectiveFovAsync),
                    AsyncButton(DemoLocalization.Text("Menu.DisplayPrecision"), SetDisplayPrecisionAsync),
                    AsyncButton(Local("Zoom Sensitivity...", "缩放灵敏度..."), SetZoomSensitivityAsync)),
                SettingsTab(Local("Display", "显示"),
                    Button(DemoLocalization.Text("Menu.Shaded"), () => SetDisplayStyle(OcctDisplayMode.Shaded)),
                    Button(DemoLocalization.Text("Menu.Wireframe"), () => SetDisplayStyle(OcctDisplayMode.Wireframe)),
                    CheckBoxButton(DemoLocalization.Text("Menu.ShadedEdges"), true, value => Session.Engine.SetFaceBoundariesVisible(value)),
                    CheckBoxButton(DemoLocalization.Text("Menu.Hlr"), false, value => Session.Engine.SetComputedHlr(value)),
                    CheckBoxButton(DemoLocalization.Text("Menu.Antialiasing"), true, value => Session.Engine.SetAntialiasing(value)),
                    CheckBoxButton(DemoLocalization.Text("Menu.Triedron"), true, value => Session.Engine.SetTriedronVisible(value)),
                    CheckBoxButton(DemoLocalization.Text("Menu.ViewCube"), true, value => Session.Engine.SetViewCubeVisible(value)),
                    AsyncButton(DemoLocalization.Text("Menu.Background"), SetBackgroundColorAsync),
                    Button(DemoLocalization.Text("Menu.GradientBackground"), () => Session.Engine.SetGradientBackground(DrawingColor.White, DrawingColor.LightSteelBlue))),
                SettingsTab(Local("Selection", "选择"),
                    AsyncButton(Local("Selected Color...", "选中高亮颜色..."), SetSelectionHighlightColorAsync),
                    AsyncButton(Local("Hover Color...", "悬浮高亮颜色..."), SetHoverHighlightColorAsync),
                    Button(Local("Selected Bounding Box", "选中包围盒"), () => SetSelectionHighlightMode(OcctHighlightMode.BoundingBox)),
                    Button(Local("Selected Wireframe", "选中线框"), () => SetSelectionHighlightMode(OcctHighlightMode.Wireframe)),
                    Button(Local("Selected Shaded", "选中着色"), () => SetSelectionHighlightMode(OcctHighlightMode.Shaded)),
                    Button(Local("Hover Bounding Box", "悬浮包围盒"), () => SetHoverHighlightMode(OcctHighlightMode.BoundingBox)),
                    Button(Local("Hover Wireframe", "悬浮线框"), () => SetHoverHighlightMode(OcctHighlightMode.Wireframe)),
                    Button(Local("Hover Shaded", "悬浮着色"), () => SetHoverHighlightMode(OcctHighlightMode.Shaded)),
                    AsyncButton(DemoLocalization.Text("Menu.SelectionTolerance"), SetSelectionToleranceAsync),
                    CheckBoxButton(DemoLocalization.Text("Menu.WindowSelection"), (_viewport.InteractionFeatures & OcctViewportInteractionFeatures.RectangleSelection) != 0, SetWindowSelectionEnabled)),
                SettingsTab(Local("Helpers", "辅助"),
                    Button(Local("Triedron Left Lower", "坐标轴左下"), () => SetTriedronPosition(OcctCornerPosition.LeftLower)),
                    Button(Local("Triedron Left Upper", "坐标轴左上"), () => SetTriedronPosition(OcctCornerPosition.LeftUpper)),
                    Button(Local("Triedron Right Lower", "坐标轴右下"), () => SetTriedronPosition(OcctCornerPosition.RightLower)),
                    Button(Local("Triedron Right Upper", "坐标轴右上"), () => SetTriedronPosition(OcctCornerPosition.RightUpper)),
                    Button(Local("ViewCube Left Lower", "ViewCube 左下"), () => SetViewCubePosition(OcctCornerPosition.LeftLower)),
                    Button(Local("ViewCube Left Upper", "ViewCube 左上"), () => SetViewCubePosition(OcctCornerPosition.LeftUpper)),
                    Button(Local("ViewCube Right Lower", "ViewCube 右下"), () => SetViewCubePosition(OcctCornerPosition.RightLower)),
                    Button(Local("ViewCube Right Upper", "ViewCube 右上"), () => SetViewCubePosition(OcctCornerPosition.RightUpper)),
                    Button(Local("ViewCube 72 px", "ViewCube 72 px"), () => SetViewCubeSize(72)),
                    Button(Local("ViewCube 90 px", "ViewCube 90 px"), () => SetViewCubeSize(90)),
                    Button(Local("ViewCube 120 px", "ViewCube 120 px"), () => SetViewCubeSize(120)),
                    Button(Local("ViewCube Offset 10", "ViewCube 偏移 10"), () => SetViewCubeOffset(10, 10)),
                    Button(Local("ViewCube Offset 20", "ViewCube 偏移 20"), () => SetViewCubeOffset(20, 20)))
            }
        };

        var window = new Window
        {
            Title = Local("View Settings", "视图设置"),
            Width = 520,
            Height = 620,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = tabs
        };
        _viewSettingsWindow = window;
        window.Closed += (_, _) => _viewSettingsWindow = null;
        window.Show(this);
    }

    private static TabItem SettingsTab(string text, params Control[] controls)
    {
        var panel = new StackPanel { Margin = new Thickness(12), Spacing = 6 };
        foreach (var control in controls) panel.Children.Add(control);
        return new TabItem
        {
            Header = text,
            Content = new ScrollViewer { Content = panel }
        };
    }

    private static Button Button(string text, Action action)
    {
        var button = new Button { Content = text, Width = 240, Height = 30, HorizontalAlignment = HorizontalAlignment.Left };
        button.Click += (_, _) => action();
        return button;
    }

    private static Button AsyncButton(string text, Func<Task> action)
    {
        var button = new Button { Content = text, Width = 240, Height = 30, HorizontalAlignment = HorizontalAlignment.Left };
        button.Click += async (_, _) => await action();
        return button;
    }

    private static CheckBox CheckBoxButton(string text, bool initialValue, Action<bool> action)
    {
        var box = new CheckBox { Content = text, IsChecked = initialValue, Width = 280, Height = 28 };
        box.IsCheckedChanged += (_, _) => action(box.IsChecked == true);
        return box;
    }

    private void SetDisplayStyle(OcctDisplayMode mode)
    {
        ExecuteSafe(() =>
        {
            _displayMode = mode;
            Session.Engine.SetDisplayMode(mode);
            Log($"{Local("Display Style", "显示样式")}: {mode}");
        });
    }

    private void SetProjectionMode(OcctProjectionType projection)
    {
        ExecuteSafe(() =>
        {
            _projectionType = projection;
            Session.Engine.SetProjection(projection);
            Log($"{DemoLocalization.Text("Menu.Projection")}: {projection}");
        });
    }

    private void SetWindowSelectionEnabled(bool enabled)
    {
        if (enabled)
            _viewport.InteractionFeatures |= OcctViewportInteractionFeatures.RectangleSelection;
        else
            _viewport.InteractionFeatures &= ~OcctViewportInteractionFeatures.RectangleSelection;
        _commandStatus.Text = DemoLocalization.Text(enabled ? "Status.WindowSelectionOn" : "Status.WindowSelectionOff");
    }

    private MenuItem BuildHighlightModeMenu(
        string text,
        string groupName,
        OcctHighlightMode currentMode,
        Action<OcctHighlightMode> apply)
    {
        var items = new List<object>();
        foreach (var mode in Enum.GetValues<OcctHighlightMode>())
        {
            var captured = mode;
            items.Add(CheckMenuItem(
                HighlightModeName(captured),
                captured == currentMode,
                _ => apply(captured),
                radio: true,
                groupName: groupName));
        }
        return Menu(text, items.ToArray());
    }

    private MenuItem BuildCornerPositionMenu(
        string text,
        string groupName,
        OcctCornerPosition currentPosition,
        Action<OcctCornerPosition> apply)
    {
        var items = new List<object>();
        foreach (var position in Enum.GetValues<OcctCornerPosition>())
        {
            var captured = position;
            items.Add(CheckMenuItem(
                CornerPositionName(captured),
                captured == currentPosition,
                _ => apply(captured),
                radio: true,
                groupName: groupName));
        }
        return Menu(text, items.ToArray());
    }

    private void SetSelectionHighlightMode(OcctHighlightMode mode)
    {
        ExecuteSafe(() =>
        {
            _selectionHighlightMode = mode;
            Session.Engine.SetSelectionHighlightStyle(
                new OcctViewerHighlightStyle(_selectionHighlightMode, _selectionHighlightColor));
            var message = Local(
                $"Selected highlight mode: {HighlightModeName(mode)}",
                $"选中高亮模式：{HighlightModeName(mode)}");
            _commandStatus.Text = message;
            Log(message);
        });
    }

    private void SetHoverHighlightMode(OcctHighlightMode mode)
    {
        ExecuteSafe(() =>
        {
            _hoverHighlightMode = mode;
            Session.Engine.SetHoverHighlightStyle(
                new OcctViewerHighlightStyle(_hoverHighlightMode, _hoverHighlightColor));
            var message = Local(
                $"Hover highlight mode: {HighlightModeName(mode)}",
                $"悬浮高亮模式：{HighlightModeName(mode)}");
            _commandStatus.Text = message;
            Log(message);
        });
    }

    private void SetTriedronPosition(OcctCornerPosition position)
    {
        ExecuteSafe(() =>
        {
            _triedronPosition = position;
            Session.Engine.SetTriedronPosition(position);
            var message = Local($"Triedron position: {CornerPositionName(position)}", $"坐标轴位置：{CornerPositionName(position)}");
            _commandStatus.Text = message;
            Log(message);
        });
    }

    private void SetViewCubePosition(OcctCornerPosition position)
    {
        ExecuteSafe(() =>
        {
            _viewCubePosition = position;
            Session.Engine.SetViewCubePosition(position);
            var message = Local($"ViewCube position: {CornerPositionName(position)}", $"ViewCube 位置：{CornerPositionName(position)}");
            _commandStatus.Text = message;
            Log(message);
        });
    }

    private void SetViewCubeSize(int sizePixels)
    {
        ExecuteSafe(() =>
        {
            Session.Engine.SetViewCubeSize(sizePixels);
            var message = Local($"ViewCube size: {sizePixels}px", $"ViewCube 大小：{sizePixels}px");
            _commandStatus.Text = message;
            Log(message);
        });
    }

    private void SetViewCubeOffset(int offsetX, int offsetY)
    {
        ExecuteSafe(() =>
        {
            Session.Engine.SetViewCubeOffset(offsetX, offsetY);
            var message = Local($"ViewCube offset: {offsetX}px, {offsetY}px", $"ViewCube 偏移：{offsetX}px，{offsetY}px");
            _commandStatus.Text = message;
            Log(message);
        });
    }

    private static string HighlightModeName(OcctHighlightMode mode) => mode switch
    {
        OcctHighlightMode.BoundingBox => Local("Bounding Box", "包围盒"),
        OcctHighlightMode.Shaded => Local("Shaded", "着色"),
        _ => Local("Wireframe", "线框")
    };

    private static string CornerPositionName(OcctCornerPosition position) => position switch
    {
        OcctCornerPosition.LeftUpper => Local("Left Upper", "左上"),
        OcctCornerPosition.RightLower => Local("Right Lower", "右下"),
        OcctCornerPosition.RightUpper => Local("Right Upper", "右上"),
        _ => Local("Left Lower", "左下")
    };
}

using OcctDemo.Common;
using OcctNet;

namespace OcctDemo.WinForms;

public sealed partial class MainForm
{
    private OcctDisplayMode _displayMode = OcctDisplayMode.Shaded;
    private OcctProjectionType _projectionType = OcctProjectionType.Orthographic;
    private OcctHighlightMode _selectionHighlightMode = OcctHighlightMode.Wireframe;
    private OcctHighlightMode _hoverHighlightMode = OcctHighlightMode.Wireframe;
    private OcctCornerPosition _triedronPosition = OcctCornerPosition.LeftLower;
    private OcctCornerPosition _viewCubePosition = OcctCornerPosition.RightUpper;

    private ToolStripMenuItem BuildSelectionHighlightModeMenu() =>
        BuildHighlightModeMenu(
            Local("Selected Mode", "选中高亮模式"),
            _selectionHighlightMode,
            SetSelectionHighlightMode);

    private ToolStripMenuItem BuildHoverHighlightModeMenu() =>
        BuildHighlightModeMenu(
            Local("Hover Mode", "悬浮高亮模式"),
            _hoverHighlightMode,
            SetHoverHighlightMode);

    private ToolStripMenuItem BuildViewHelpersMenu()
    {
        var menu = new ToolStripMenuItem(Local("View Helpers", "视图辅助"));
        menu.DropDownItems.Add(BuildCornerPositionMenu(
            Local("Triedron Position", "坐标轴位置"),
            _triedronPosition,
            SetTriedronPosition));
        menu.DropDownItems.Add(BuildCornerPositionMenu(
            Local("ViewCube Position", "ViewCube 位置"),
            _viewCubePosition,
            SetViewCubePosition));
        menu.DropDownItems.Add(new ToolStripSeparator());
        menu.DropDownItems.Add(MenuItem(Local("ViewCube Small", "ViewCube 小"), (_, _) => SetViewCubeSize(72)));
        menu.DropDownItems.Add(MenuItem(Local("ViewCube Normal", "ViewCube 正常"), (_, _) => SetViewCubeSize(90)));
        menu.DropDownItems.Add(MenuItem(Local("ViewCube Large", "ViewCube 大"), (_, _) => SetViewCubeSize(120)));
        menu.DropDownItems.Add(MenuItem(Local("ViewCube Offset 10 px", "ViewCube 偏移 10 px"), (_, _) => SetViewCubeOffset(10, 10)));
        menu.DropDownItems.Add(MenuItem(Local("ViewCube Offset 20 px", "ViewCube 偏移 20 px"), (_, _) => SetViewCubeOffset(20, 20)));
        return menu;
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

    private ToolStripMenuItem BuildHighlightModeMenu(
        string text,
        OcctHighlightMode currentMode,
        Action<OcctHighlightMode> apply)
    {
        var menu = new ToolStripMenuItem(text);
        foreach (var mode in Enum.GetValues<OcctHighlightMode>())
        {
            var captured = mode;
            var item = new ToolStripMenuItem(HighlightModeName(captured))
            {
                Checked = captured == currentMode
            };
            item.Click += (_, _) => apply(captured);
            menu.DropDownItems.Add(item);
        }
        return menu;
    }

    private ToolStripMenuItem BuildCornerPositionMenu(
        string text,
        OcctCornerPosition currentPosition,
        Action<OcctCornerPosition> apply)
    {
        var menu = new ToolStripMenuItem(text);
        foreach (var position in Enum.GetValues<OcctCornerPosition>())
        {
            var captured = position;
            var item = new ToolStripMenuItem(CornerPositionName(captured))
            {
                Checked = captured == currentPosition
            };
            item.Click += (_, _) => apply(captured);
            menu.DropDownItems.Add(item);
        }
        return menu;
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

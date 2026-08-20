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
    private bool _viewCubeVisible = true;
    private int _viewCubeSize = 90;
    private int _viewCubeOffset = 10;

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

    private void ApplyViewCubeOptions(bool refresh = true)
    {
        ExecuteSafe(() =>
        {
            // The bridge's individual SetViewCube* APIs build a PARTIAL
            // OcctViewCubeOptions and reset every other field back to its default
            // (e.g. SetViewCubeOffset resets SizePixels to 90). Always send the
            // full options object built from the tracked state instead.
            Session.Engine.SetViewCubeOptions(new OcctViewCubeOptions
            {
                Visible = _viewCubeVisible,
                Position = _viewCubePosition,
                SizePixels = _viewCubeSize,
                OffsetX = _viewCubeOffset,
                OffsetY = _viewCubeOffset
            });
            if (refresh) _viewport.RefreshNativeView();
        });
    }

    private void SetViewCubeVisible(bool visible)
    {
        _viewCubeVisible = visible;
        ApplyViewCubeOptions();
        var message = Local(visible ? "ViewCube visible" : "ViewCube hidden",
            visible ? "ViewCube 显示" : "ViewCube 隐藏");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubePosition(OcctCornerPosition position)
    {
        _viewCubePosition = position;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube position: {CornerPositionName(position)}", $"ViewCube 位置：{CornerPositionName(position)}");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeSize(int sizePixels)
    {
        _viewCubeSize = sizePixels;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube size: {sizePixels}px", $"ViewCube 大小：{sizePixels}px");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeOffset(int offsetX, int offsetY)
    {
        _viewCubeOffset = offsetX;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube offset: {offsetX}px, {offsetY}px", $"ViewCube 偏移：{offsetX}px，{offsetY}px");
        _commandStatus.Text = message;
        Log(message);
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

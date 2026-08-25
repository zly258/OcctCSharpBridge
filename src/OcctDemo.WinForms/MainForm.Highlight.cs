using System.Drawing;
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
    private bool _viewCubeVisible = true;
    private int _viewCubeSize = 72;
    private int _viewCubeOffsetX = 82;
    private int _viewCubeOffsetY = 82;
    private int _viewCubeFontHeight = 15;
    private System.Drawing.Color _viewCubeBoxColor = System.Drawing.Color.FromArgb(200, 210, 225);
    private System.Drawing.Color _viewCubeFacetColor = System.Drawing.Color.FromArgb(255, 220, 0);
    private System.Drawing.Color _viewCubeTextColor = System.Drawing.Color.Black;

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
            // Full init-only assignment; defaults match product preference.
            var options = new OcctViewCubeOptions
            {
                Visible = _viewCubeVisible,
                Position = _viewCubePosition,
                SizePixels = _viewCubeSize,
                OffsetX = _viewCubeOffsetX,
                OffsetY = _viewCubeOffsetY,
                FontHeight = _viewCubeFontHeight,
                BoxColor = _viewCubeBoxColor,
                FacetColor = _viewCubeFacetColor,
                TextColor = _viewCubeTextColor
            };
            Session.Engine.SetViewCubeOptions(options);
            if (refresh) _viewport.Invalidate();
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
        _viewCubeOffsetX = offsetX;
        _viewCubeOffsetY = offsetY;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube offset: {offsetX}px, {offsetY}px", $"ViewCube 偏移：{offsetX}px，{offsetY}px");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeOffsetX(int offsetX)
    {
        _viewCubeOffsetX = offsetX;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube offset X: {offsetX}px", $"ViewCube 偏移 X：{offsetX}px");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeOffsetY(int offsetY)
    {
        _viewCubeOffsetY = offsetY;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube offset Y: {offsetY}px", $"ViewCube 偏移 Y：{offsetY}px");
        _commandStatus.Text = message;
        Log(message);
    }

    
    private void SetViewCubeBoxColor(System.Drawing.Color color)
    {
        _viewCubeBoxColor = color;
        ApplyViewCubeOptions();
    }

    private void PickViewCubeBoxColor()
    {
        using var dialog = new System.Windows.Forms.ColorDialog { Color = _viewCubeBoxColor, FullOpen = true };
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        SetViewCubeBoxColor(dialog.Color);
    }

    private void PickViewCubeFacetColor()
    {
        using var dialog = new ColorDialog { Color = _viewCubeFacetColor, FullOpen = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        SetViewCubeFacetColor(dialog.Color);
    }

    private void PickViewCubeTextColor()
    {
        using var dialog = new ColorDialog { Color = _viewCubeTextColor, FullOpen = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        SetViewCubeTextColor(dialog.Color);
    }

    private void SetViewCubeFontHeight(int height)
    {
        _viewCubeFontHeight = height;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube font height: {height}", $"ViewCube 文字大小：{height}");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeFacetColor(System.Drawing.Color color)
    {
        _viewCubeFacetColor = color;
        ApplyViewCubeOptions();
        var message = Local("ViewCube highlight color updated.", "ViewCube 高亮颜色已更新。");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeTextColor(System.Drawing.Color color)
    {
        _viewCubeTextColor = color;
        ApplyViewCubeOptions();
        var message = Local("ViewCube text color updated.", "ViewCube 文字颜色已更新。");
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

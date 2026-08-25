using System.Threading.Tasks;
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
    
    private enum DemoVisualStyle { Wireframe, Shaded, ShadedEdges, HiddenLine }
    private DemoVisualStyle _visualStyle = DemoVisualStyle.ShadedEdges;

    private void ApplyVisualStyle(DemoVisualStyle style)
    {
        _visualStyle = style;
        ExecuteSafe(() =>
        {
            switch (style)
            {
                case DemoVisualStyle.Wireframe:
                    _displayMode = OcctDisplayMode.Wireframe;
                    Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe);
                    Session.Engine.SetComputedHlr(false);
                    break;
                case DemoVisualStyle.Shaded:
                    _displayMode = OcctDisplayMode.Shaded;
                    Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded);
                    Session.Engine.SetFaceBoundariesVisible(false);
                    Session.Engine.SetComputedHlr(false);
                    break;
                case DemoVisualStyle.ShadedEdges:
                    _displayMode = OcctDisplayMode.Shaded;
                    Session.Engine.SetDisplayMode(OcctDisplayMode.Shaded);
                    Session.Engine.SetFaceBoundariesVisible(true);
                    Session.Engine.SetComputedHlr(false);
                    break;
                case DemoVisualStyle.HiddenLine:
                    _displayMode = OcctDisplayMode.Wireframe;
                    Session.Engine.SetDisplayMode(OcctDisplayMode.Wireframe);
                    Session.Engine.SetComputedHlr(true);
                    break;
            }
        });
    }

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
    private int _viewCubeFontHeight = 30;
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
            if (refresh) _viewport.InvalidateVisual();
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
    }

    private void SetViewCubeOffsetY(int offsetY)
    {
        _viewCubeOffsetY = offsetY;
        ApplyViewCubeOptions();
    }

    private static string HighlightModeName(OcctHighlightMode mode) => mode switch
    {
        OcctHighlightMode.BoundingBox => Local("Bounding Box", "包围盒"),
        OcctHighlightMode.Shaded => Local("Shaded", "着色"),
        _ => Local("Wireframe", "线框")
    };

    
    private void SetViewCubeFontHeight(int height) { _viewCubeFontHeight = height; ApplyViewCubeOptions(); }
    private void SetViewCubeFacetColor(System.Drawing.Color color)
    {
        _viewCubeFacetColor = color; ApplyViewCubeOptions(); }
    private void SetViewCubeTextColor(System.Drawing.Color color)
    {
        _viewCubeTextColor = color; ApplyViewCubeOptions(); }

    
    private void SetViewCubeBoxColor(System.Drawing.Color color)
    {
        _viewCubeBoxColor = color;
        ApplyViewCubeOptions();
    }

    private async System.Threading.Tasks.Task PickViewCubeBoxColorAsync()
    {
        var color = await ClassicColorDialog.ShowAsync(this, Local("ViewCube Face Color", "ViewCube 面颜色"), _viewCubeBoxColor);
        if (color is null) return;
        SetViewCubeBoxColor(color.Value);
    }

    private async Task PickViewCubeFacetColorAsync()
    {
        var initial = _viewCubeFacetColor;
        var color = await ClassicColorDialog.ShowAsync(this, Local("ViewCube Highlight Color", "ViewCube 高亮颜色"), initial);
        if (color is null) return;
        SetViewCubeFacetColor(color.Value);
    }

    private async Task PickViewCubeTextColorAsync()
    {
        var initial = _viewCubeTextColor;
        var color = await ClassicColorDialog.ShowAsync(this, Local("ViewCube Text Color", "ViewCube 文字颜色"), initial);
        if (color is null) return;
        SetViewCubeTextColor(color.Value);
    }

private static string CornerPositionName(OcctCornerPosition position) => position switch
    {
        OcctCornerPosition.LeftUpper => Local("Left Upper", "左上"),
        OcctCornerPosition.RightLower => Local("Right Lower", "右下"),
        OcctCornerPosition.RightUpper => Local("Right Upper", "右上"),
        _ => Local("Left Lower", "左下")
    };
}

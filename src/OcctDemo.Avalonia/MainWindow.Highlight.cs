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
    private int _viewCubeSize = 72;
    private int _viewCubeOffset = 82;
    private double _viewCubeFontHeight = 12.0;
    private string _viewCubeFontName = "Segoe UI";
    private DrawingColor _viewCubeTextColor = DrawingColor.Black;
    private DrawingColor _viewCubeBoxColor = DrawingColor.LightGray;
    private DrawingColor _viewCubeFacetColor = DrawingColor.SteelBlue;
    private double _viewCubeCornerRadius = 0.12;
    private double _viewCubeEdgeWidth = 1.0;

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
            Session.Engine.SetViewCubeOptions(new OcctViewCubeOptions
            {
                Visible = _viewCubeVisible,
                Position = _viewCubePosition,
                SizePixels = _viewCubeSize,
                OffsetX = _viewCubeOffset,
                OffsetY = _viewCubeOffset,
                FontHeight = _viewCubeFontHeight,
                FontName = _viewCubeFontName,
                TextColor = _viewCubeTextColor,
                BoxColor = _viewCubeBoxColor,
                FacetColor = _viewCubeFacetColor,
                CornerRadius = _viewCubeCornerRadius,
                EdgeWidth = _viewCubeEdgeWidth
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

    private void SetViewCubeFontHeight(double fontHeight)
    {
        _viewCubeFontHeight = fontHeight;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube font height: {fontHeight:F1}pt", $"ViewCube 字体大小：{fontHeight:F1}pt");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeFontName(string fontName)
    {
        _viewCubeFontName = fontName;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube font: {fontName}", $"ViewCube 字体：{fontName}");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeTextColor(DrawingColor color)
    {
        _viewCubeTextColor = color;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube text color: {color.Name}", $"ViewCube 文字颜色：{color.Name}");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeBoxColor(DrawingColor color)
    {
        _viewCubeBoxColor = color;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube box color: {color.Name}", $"ViewCube 背景颜色：{color.Name}");
        _commandStatus.Text = message;
        Log(message);
    }

    private void SetViewCubeFacetColor(DrawingColor color)
    {
        _viewCubeFacetColor = color;
        ApplyViewCubeOptions();
        var message = Local($"ViewCube facet color: {color.Name}", $"ViewCube 面高亮颜色：{color.Name}");
        _commandStatus.Text = message;
        Log(message);
    }

    private void ResetViewCubeAppearance()
    {
        _viewCubeFontHeight = 12.0;
        _viewCubeFontName = "Segoe UI";
        _viewCubeTextColor = DrawingColor.Black;
        _viewCubeBoxColor = DrawingColor.LightGray;
        _viewCubeFacetColor = DrawingColor.SteelBlue;
        _viewCubeCornerRadius = 0.12;
        _viewCubeEdgeWidth = 1.0;
        ApplyViewCubeOptions();
        var message = Local("ViewCube appearance reset to defaults", "ViewCube 外观已重置为默认值");
        _commandStatus.Text = message;
        Log(message);
    }

    private void ApplyDepthBias(OcctDepthBiasPreset preset)
    {
        Session.Engine.SetDepthBiasPreset(preset);
        var message = Local($"Global Depth Bias: {preset}", $"全局深度偏移/防闪烁：{DepthBiasName(preset)}");
        _commandStatus.Text = message;
        Log(message);
    }

    private static string DepthBiasName(OcctDepthBiasPreset preset) => preset switch
    {
        OcctDepthBiasPreset.None => Local("None (Off)", "关闭偏移"),
        OcctDepthBiasPreset.CoincidentFaces => Local("Coincident Faces (Anti-flicker)", "重合面防闪烁 (推荐)"),
        OcctDepthBiasPreset.Aggressive => Local("Aggressive", "强力偏移"),
        _ => Local("Default", "默认标准")
    };

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

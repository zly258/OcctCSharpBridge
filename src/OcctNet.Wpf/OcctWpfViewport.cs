using System.Windows;
using System.Windows.Forms.Integration;
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;
using MediaColors = System.Windows.Media.Colors;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace OcctNet;

/// <summary>
/// Reusable WPF host for the OCCT HWND viewport. The native viewer remains isolated in
/// <see cref="OcctViewportControl"/>, while WPF applications receive dependency properties
/// and WPF-native composition through <see cref="WindowsFormsHost"/>.
/// </summary>
public sealed class OcctWpfViewport : WpfUserControl
{
    private readonly WindowsFormsHost _host;
    private readonly OcctViewportControl _viewport;

    public static readonly DependencyProperty EnableRectangleSelectionProperty =
        DependencyProperty.Register(
            nameof(EnableRectangleSelection),
            typeof(bool),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(true, OnSelectionPropertyChanged));

    public static readonly DependencyProperty RectangleSelectionThresholdProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionThreshold),
            typeof(int),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(3, OnSelectionPropertyChanged, CoercePositiveInteger));

    public static readonly DependencyProperty RectangleSelectionBehaviorProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionBehavior),
            typeof(OcctRectangleSelectionBehavior),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(OcctRectangleSelectionBehavior.Inclusive, OnSelectionPropertyChanged));

    public static readonly DependencyProperty RectangleSelectionLineColorProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionLineColor),
            typeof(MediaColor),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(MediaColors.DodgerBlue, OnSelectionPropertyChanged));

    public static readonly DependencyProperty RectangleSelectionFillColorProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionFillColor),
            typeof(MediaColor),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(MediaColors.LightSkyBlue, OnSelectionPropertyChanged));

    public static readonly DependencyProperty RectangleSelectionFillTransparencyProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionFillTransparency),
            typeof(double),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(0.82, OnSelectionPropertyChanged, CoerceUnitInterval));

    public static readonly DependencyProperty RectangleSelectionLineWidthProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionLineWidth),
            typeof(double),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(1.0, OnSelectionPropertyChanged, CoercePositiveDouble));

    public OcctWpfViewport()
    {
        _viewport = new OcctViewportControl();
        _host = new WindowsFormsHost { Child = _viewport };
        Content = _host;
        Focusable = true;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;
        ApplySelectionProperties();

        Loaded += (_, _) => FocusViewport();
        PreviewMouseDown += (_, _) => FocusViewport();
    }

    public OcctEngine Engine => _viewport.Engine;

    /// <summary>Access to the low-level WinForms HWND host for advanced interoperability.</summary>
    public OcctViewportControl WinFormsViewport => _viewport;

    public bool EnableRectangleSelection
    {
        get => (bool)GetValue(EnableRectangleSelectionProperty);
        set => SetValue(EnableRectangleSelectionProperty, value);
    }

    public int RectangleSelectionThreshold
    {
        get => (int)GetValue(RectangleSelectionThresholdProperty);
        set => SetValue(RectangleSelectionThresholdProperty, value);
    }

    public OcctRectangleSelectionBehavior RectangleSelectionBehavior
    {
        get => (OcctRectangleSelectionBehavior)GetValue(RectangleSelectionBehaviorProperty);
        set => SetValue(RectangleSelectionBehaviorProperty, value);
    }

    public MediaColor RectangleSelectionLineColor
    {
        get => (MediaColor)GetValue(RectangleSelectionLineColorProperty);
        set => SetValue(RectangleSelectionLineColorProperty, value);
    }

    public MediaColor RectangleSelectionFillColor
    {
        get => (MediaColor)GetValue(RectangleSelectionFillColorProperty);
        set => SetValue(RectangleSelectionFillColorProperty, value);
    }

    public double RectangleSelectionFillTransparency
    {
        get => (double)GetValue(RectangleSelectionFillTransparencyProperty);
        set => SetValue(RectangleSelectionFillTransparencyProperty, value);
    }

    public double RectangleSelectionLineWidth
    {
        get => (double)GetValue(RectangleSelectionLineWidthProperty);
        set => SetValue(RectangleSelectionLineWidthProperty, value);
    }

    public event EventHandler<OcctShape?>? SelectionChanged
    {
        add => _viewport.SelectionChanged += value;
        remove => _viewport.SelectionChanged -= value;
    }

    public event EventHandler<OcctViewportSelectionEventArgs>? ObjectSelectionChanged
    {
        add => _viewport.ObjectSelectionChanged += value;
        remove => _viewport.ObjectSelectionChanged -= value;
    }

    public event EventHandler<OcctViewportWorldPointEventArgs>? WorldPointChanged
    {
        add => _viewport.WorldPointChanged += value;
        remove => _viewport.WorldPointChanged -= value;
    }

    public event EventHandler<OcctViewportErrorEventArgs>? ErrorOccurred
    {
        add => _viewport.ErrorOccurred += value;
        remove => _viewport.ErrorOccurred -= value;
    }

    public event EventHandler? EngineInitialized
    {
        add => _viewport.EngineInitialized += value;
        remove => _viewport.EngineInitialized -= value;
    }

    public void FocusViewport() => _viewport.Focus();

    public void RaiseSelectionChanged() => _viewport.RaiseSelectionChanged();

    private static void OnSelectionPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs _)
    {
        ((OcctWpfViewport)dependencyObject).ApplySelectionProperties();
    }

    private void ApplySelectionProperties()
    {
        _viewport.EnableRectangleSelection = EnableRectangleSelection;
        _viewport.RectangleSelectionThreshold = RectangleSelectionThreshold;
        _viewport.RectangleSelectionBehavior = RectangleSelectionBehavior;
        _viewport.RectangleSelectionLineColor = ToDrawingColor(RectangleSelectionLineColor);
        _viewport.RectangleSelectionFillColor = ToDrawingColor(RectangleSelectionFillColor);
        _viewport.RectangleSelectionFillTransparency = RectangleSelectionFillTransparency;
        _viewport.RectangleSelectionLineWidth = RectangleSelectionLineWidth;
    }

    private static DrawingColor ToDrawingColor(MediaColor value) =>
        DrawingColor.FromArgb(value.A, value.R, value.G, value.B);

    private static object CoercePositiveInteger(DependencyObject _, object value) =>
        Math.Max(1, (int)value);

    private static object CoercePositiveDouble(DependencyObject _, object value)
    {
        var number = (double)value;
        return double.IsFinite(number) && number > 0.0 ? number : 1.0;
    }

    private static object CoerceUnitInterval(DependencyObject _, object value)
    {
        var number = (double)value;
        return double.IsFinite(number) ? Math.Clamp(number, 0.0, 1.0) : 0.82;
    }
}

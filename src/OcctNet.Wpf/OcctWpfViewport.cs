using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;
using MediaColors = System.Windows.Media.Colors;

namespace OcctNet;

/// <summary>Selection state reported by the native WPF viewport host.</summary>
public sealed class OcctWpfSelectionEventArgs : EventArgs
{
    public OcctWpfSelectionEventArgs(IOcctObject? selectedObject, IReadOnlyList<IOcctObject> selectedObjects)
    {
        SelectedObject = selectedObject;
        SelectedObjects = selectedObjects ?? throw new ArgumentNullException(nameof(selectedObjects));
    }

    public IOcctObject? SelectedObject { get; }
    public IReadOnlyList<IOcctObject> SelectedObjects { get; }
}

/// <summary>World-space point corresponding to a WPF viewport screen position.</summary>
public sealed class OcctWpfWorldPointEventArgs : EventArgs
{
    public OcctWpfWorldPointEventArgs(int screenX, int screenY, OcctPoint3d worldPoint)
    {
        ScreenX = screenX;
        ScreenY = screenY;
        WorldPoint = worldPoint;
    }

    public int ScreenX { get; }
    public int ScreenY { get; }
    public OcctPoint3d WorldPoint { get; }
}

/// <summary>Error reported by the WPF viewport adapter without terminating the UI event loop.</summary>
public sealed class OcctWpfErrorEventArgs : EventArgs
{
    public OcctWpfErrorEventArgs(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public Exception Exception { get; }
}

/// <summary>
/// Native WPF host for the OCCT HWND viewport. The WPF adapter owns its child HWND directly
/// through <see cref="HwndHost"/> and has no dependency on Windows Forms.
/// </summary>
public sealed partial class OcctWpfViewport : HwndHost, IOcctViewportHost, IOcctViewportInputSource
{
    private OcctEngine? _engine;
    private IntPtr _nativeHandle;
    private bool _rotating;
    private bool _panning;
    private bool _leftSelectionGesture;
    private bool _selectingRectangle;
    private bool _rectangleDragStarted;
    private int _lastMouseX;
    private int _lastMouseY;
    private int _selectionStartX;
    private int _selectionStartY;
    private int _selectionCurrentX;
    private int _selectionCurrentY;
    private SelectionFrame? _selectionFrame;
    private long _lastHoverTimestamp;
    private long _lastWorldPointTimestamp;
    private bool _nativeRefreshScheduled;
    private bool _nativeRenderScheduled;
    private uint _lastRenderDpi;
    private OcctViewportHostState _hostState = OcctViewportHostState.Detached;
    private long _engineGeneration;

    public static readonly DependencyProperty InteractionFeaturesProperty =
        DependencyProperty.Register(
            nameof(InteractionFeatures),
            typeof(OcctViewportInteractionFeatures),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(OcctViewportInteractionFeatures.Default, OnInteractionFeaturesChanged));

    public static readonly DependencyProperty ZoomSensitivityProperty =
        DependencyProperty.Register(
            nameof(ZoomSensitivity),
            typeof(double),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(1.0, null, CoerceZoomSensitivity));

    public static readonly DependencyProperty RectangleSelectionThresholdProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionThreshold),
            typeof(int),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(3, null, CoercePositiveInteger));

    public static readonly DependencyProperty RectangleSelectionBehaviorProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionBehavior),
            typeof(OcctRectangleSelectionBehavior),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(OcctRectangleSelectionBehavior.Inclusive));

    public static readonly DependencyProperty RectangleSelectionLineColorProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionLineColor),
            typeof(MediaColor),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(MediaColors.DodgerBlue));

    public static readonly DependencyProperty RectangleSelectionFillColorProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionFillColor),
            typeof(MediaColor),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(MediaColors.LightSkyBlue));

    public static readonly DependencyProperty RectangleSelectionFillTransparencyProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionFillTransparency),
            typeof(double),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(0.82, null, CoerceUnitInterval));

    public static readonly DependencyProperty RectangleSelectionLineWidthProperty =
        DependencyProperty.Register(
            nameof(RectangleSelectionLineWidth),
            typeof(double),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(1.0, null, CoercePositiveDouble));

    public static readonly DependencyProperty SynchronizeRenderDpiProperty =
        DependencyProperty.Register(
            nameof(SynchronizeRenderDpi),
            typeof(bool),
            typeof(OcctWpfViewport),
            new FrameworkPropertyMetadata(true, OnDpiSynchronizationChanged));

    public OcctWpfViewport()
    {
        Focusable = true;
        IsVisibleChanged += (_, _) => ScheduleNativeViewRefresh();
    }

    public OcctEngine Engine => _engine ?? throw new InvalidOperationException("The WPF OCCT viewport has not been created yet.");
    public IntPtr NativeHandle => _nativeHandle;
    public bool IsEngineInitialized => _engine?.IsInitialized == true;
    public OcctViewportHostState HostState => _hostState;
    public long EngineGeneration => _engineGeneration;

    public OcctViewportInteractionFeatures InteractionFeatures
    {
        get => (OcctViewportInteractionFeatures)GetValue(InteractionFeaturesProperty);
        set => SetValue(InteractionFeaturesProperty, value);
    }

    public double ZoomSensitivity
    {
        get => (double)GetValue(ZoomSensitivityProperty);
        set => SetValue(ZoomSensitivityProperty, value);
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

    public bool SynchronizeRenderDpi
    {
        get => (bool)GetValue(SynchronizeRenderDpiProperty);
        set => SetValue(SynchronizeRenderDpiProperty, value);
    }

    public event EventHandler<OcctShape?>? SelectionChanged;
    public event EventHandler<OcctWpfSelectionEventArgs>? ObjectSelectionChanged;
    public event EventHandler<OcctWpfWorldPointEventArgs>? WorldPointChanged;
    public event EventHandler<OcctWpfErrorEventArgs>? ErrorOccurred;
    public event EventHandler<OcctPointerInputEventArgs>? PreviewPointerInput;
    public event EventHandler<OcctPointerInputEventArgs>? PointerInput;
    public event EventHandler<OcctKeyInputEventArgs>? PreviewKeyInput;
    public event EventHandler<OcctKeyInputEventArgs>? KeyInput;
    public event EventHandler<OcctViewportHostStateChangedEventArgs>? HostStateChanged;
    public event EventHandler<OcctViewportFaultedEventArgs>? Faulted;
    public event EventHandler<OcctEngineLifecycleEventArgs>? EngineDisposing;
    public event EventHandler<OcctEngineLifecycleEventArgs>? EngineRecreated;

    public void FocusViewport()
    {
        Focus();
        if (_nativeHandle != IntPtr.Zero) SetFocus(_nativeHandle);
    }

    public void RaiseSelectionChanged()
    {
        if (_engine?.IsInitialized != true) return;
        var selected = _engine.FirstSelectedObject;
        SelectionChanged?.Invoke(this, _engine.FirstSelected);
        ObjectSelectionChanged?.Invoke(this, new OcctWpfSelectionEventArgs(selected, _engine.SelectedObjects));
    }

    private bool HasInteractionFeature(OcctViewportInteractionFeatures feature) =>
        (InteractionFeatures & feature) != 0;

    private void TryInvoke(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
    }

    private void ReportError(Exception exception)
    {
        System.Diagnostics.Debug.WriteLine(exception);
        try
        {
            ErrorOccurred?.Invoke(this, new OcctWpfErrorEventArgs(exception));
        }
        catch (Exception handlerException)
        {
            System.Diagnostics.Debug.WriteLine(handlerException);
        }
    }

    private static void OnInteractionFeaturesChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        var viewport = (OcctWpfViewport)dependencyObject;
        var value = (OcctViewportInteractionFeatures)args.NewValue;
        const OcctViewportInteractionFeatures allowed = OcctViewportInteractionFeatures.Default;
        if ((value & ~allowed) != 0)
            throw new ArgumentOutOfRangeException(nameof(InteractionFeatures), value, "Unknown viewport interaction feature flags.");

        if (!viewport.HasInteractionFeature(OcctViewportInteractionFeatures.Rotate)) viewport._rotating = false;
        if (!viewport.HasInteractionFeature(OcctViewportInteractionFeatures.Pan)) viewport._panning = false;
        if (!viewport.HasInteractionFeature(OcctViewportInteractionFeatures.RectangleSelection) && viewport._selectingRectangle)
            viewport.CancelRectangleSelection();
        else if (!viewport.HasInteractionFeature(OcctViewportInteractionFeatures.Selection))
            viewport.CancelRectangleSelection();
    }

    private static void OnDpiSynchronizationChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs _)
    {
        ((OcctWpfViewport)dependencyObject).ScheduleNativeViewRefresh();
    }

    private static object CoercePositiveInteger(DependencyObject _, object value) => Math.Max(1, (int)value);

    private static object CoercePositiveDouble(DependencyObject _, object value)
    {
        var number = (double)value;
        return double.IsFinite(number) && number > 0.0 ? number : 1.0;
    }

    private static object CoerceZoomSensitivity(DependencyObject _, object value)
    {
        var number = (double)value;
        return double.IsFinite(number)
            ? OcctViewportInteractionPolicy.NormalizeZoomSensitivity(number)
            : 1.0;
    }

    private static object CoerceUnitInterval(DependencyObject _, object value)
    {
        var number = (double)value;
        return double.IsFinite(number) ? Math.Clamp(number, 0.0, 1.0) : 0.82;
    }
}

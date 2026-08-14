using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetObjectClipPlanes(IOcctObject value, IReadOnlyList<OcctViewClipPlane> planes)
    {
        EnsureObject(value);
        ArgumentNullException.ThrowIfNull(planes);
        if (planes.Count > ViewClipPlaneLimit)
            throw new ArgumentException("Clip plane count exceeds the current view limit.", nameof(planes));

        var native = new NativeOcctViewClipPlane[planes.Count];
        for (var index = 0; index < planes.Count; index++)
        {
            var plane = planes[index] ?? throw new ArgumentException("Clip plane entries must not be null.", nameof(planes));
            OcctGuard.Finite(plane.Point, nameof(planes));
            OcctGuard.NonZero(plane.Normal, nameof(planes));
            native[index] = new NativeOcctViewClipPlane
            {
                Point = plane.Point,
                Normal = plane.Normal,
                Enabled = plane.Enabled ? 1 : 0,
                Capping = plane.Capping ? 1 : 0,
                CappingR = plane.CappingColor.R / 255.0,
                CappingG = plane.CappingColor.G / 255.0,
                CappingB = plane.CappingColor.B / 255.0
            };
        }

        CheckInitialized(() => PresentationNativeMethods.occt_set_object_clip_planes(
            _handle,
            value.Id,
            native,
            native.Length));
    }

    public void ClearObjectClipPlanes(IOcctObject value) =>
        SetObjectClipPlanes(value, Array.Empty<OcctViewClipPlane>());

    public void SetHighlightStyle(OcctHighlightStyleKind kind, OcctHighlightStyle style)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        var native = ToNativeHighlightStyle(style);
        CheckInitialized(() => PresentationNativeMethods.occt_set_global_highlight_style(_handle, (int)kind, in native));
    }

    public void SetObjectHighlightStyle(IOcctObject value, bool dynamic, OcctHighlightStyle style)
    {
        EnsureObject(value);
        var native = ToNativeHighlightStyle(style);
        CheckInitialized(() => PresentationNativeMethods.occt_set_object_highlight_style(
            _handle,
            value.Id,
            dynamic ? 1 : 0,
            in native));
    }

    public void ClearObjectHighlightStyle(IOcctObject value, bool dynamic)
    {
        EnsureObject(value);
        CheckInitialized(() => PresentationNativeMethods.occt_clear_object_highlight_style(
            _handle,
            value.Id,
            dynamic ? 1 : 0));
    }

    public void ResetDisplayMode(IOcctObject value)
    {
        EnsureObject(value);
        var options = PresentationStateOptions(NativeViewerPresentationStateUpdateMask.ResetDisplayMode);
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_presentation_state_update(
            _handle,
            value.Id,
            in options));
    }

    public OcctDisplayMode? GetDisplayModeOverride(IOcctObject value)
    {
        var state = GetPresentationState(value);
        if (state.HasDisplayModeOverride == 0) return null;
        if (!Enum.IsDefined(typeof(OcctDisplayMode), state.DisplayMode))
            throw new InvalidOperationException($"Native object display mode {state.DisplayMode} is not supported by the managed bridge.");
        return (OcctDisplayMode)state.DisplayMode;
    }

    public void SetAutoHighlight(IOcctObject value, bool enabled)
    {
        EnsureObject(value);
        var options = PresentationStateOptions(
            NativeViewerPresentationStateUpdateMask.AutoHighlight,
            autoHighlight: enabled);
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_presentation_state_update(
            _handle,
            value.Id,
            in options));
    }

    public bool GetAutoHighlight(IOcctObject value) => GetPresentationState(value).AutoHighlight != 0;

    public void SetInfiniteState(IOcctObject value, bool infinite)
    {
        EnsureObject(value);
        var options = PresentationStateOptions(
            NativeViewerPresentationStateUpdateMask.Infinite,
            infinite: infinite);
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_presentation_state_update(
            _handle,
            value.Id,
            in options));
    }

    public bool GetInfiniteState(IOcctObject value) => GetPresentationState(value).Infinite != 0;

    private void SetDisplayModeOverride(IOcctObject value, OcctDisplayMode displayMode)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(displayMode)) throw new ArgumentOutOfRangeException(nameof(displayMode));
        var options = PresentationStateOptions(
            NativeViewerPresentationStateUpdateMask.DisplayMode,
            displayMode: displayMode);
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_presentation_state_update(
            _handle,
            value.Id,
            in options));
    }

    private NativeViewerPresentationState GetPresentationState(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        var status = PresentationNativeMethods.occt_engine_presentation_state_get(
            _handle,
            value.Id,
            out var state);
        CheckPresentationStatus(status);
        if (state.ApiVersion != 1 || state.StructSize < (uint)Marshal.SizeOf<NativeViewerPresentationState>())
            throw new InvalidOperationException("Native presentation state version is not supported by this bridge.");
        return state;
    }

    private static NativeViewerPresentationStateOptions PresentationStateOptions(
        NativeViewerPresentationStateUpdateMask updateMask,
        OcctDisplayMode displayMode = OcctDisplayMode.Shaded,
        bool autoHighlight = false,
        bool infinite = false) => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeViewerPresentationStateOptions>(),
        ApiVersion = 1,
        UpdateMask = updateMask,
        DisplayMode = (int)displayMode,
        AutoHighlight = autoHighlight ? 1 : 0,
        Infinite = infinite ? 1 : 0
    };

    private void CheckPresentationStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }

    private static NativeOcctHighlightStyleSettings ToNativeHighlightStyle(OcctHighlightStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        if (!double.IsFinite(style.Transparency) || style.Transparency < 0.0 || style.Transparency > 1.0)
            throw new ArgumentOutOfRangeException(nameof(style.Transparency));
        OcctGuard.Positive(style.LineWidth, nameof(style.LineWidth));
        if (style.DisplayMode is { } displayMode && !Enum.IsDefined(displayMode))
            throw new ArgumentOutOfRangeException(nameof(style.DisplayMode));
        if (style.ZLayer is { } layer && !Enum.IsDefined(layer))
            throw new ArgumentOutOfRangeException(nameof(style.ZLayer));

        return new NativeOcctHighlightStyleSettings
        {
            R = style.Color.R / 255.0,
            G = style.Color.G / 255.0,
            B = style.Color.B / 255.0,
            Transparency = style.Transparency,
            LineWidth = style.LineWidth,
            DisplayMode = style.DisplayMode is { } mode ? (int)mode : -1,
            ZLayer = style.ZLayer is { } zLayer ? (int)zLayer : -1
        };
    }
}

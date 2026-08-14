using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetObjectClipPlanes(IOcctObject value, IReadOnlyList<OcctViewClipPlane> planes)
    {
        EnsureObject(value);
        ArgumentNullException.ThrowIfNull(planes);

        var native = new NativePresentationClipPlane[planes.Count];
        for (var index = 0; index < planes.Count; index++)
        {
            var plane = planes[index] ??
                throw new ArgumentException("Clip plane entries must not be null.", nameof(planes));
            OcctGuard.Finite(plane.Point, nameof(planes));
            OcctGuard.NonZero(plane.Normal, nameof(planes));
            native[index] = new NativePresentationClipPlane
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

        GCHandle pinned = default;
        try
        {
            var pointer = IntPtr.Zero;
            if (native.Length > 0)
            {
                pinned = GCHandle.Alloc(native, GCHandleType.Pinned);
                pointer = pinned.AddrOfPinnedObject();
            }

            var options = new NativeViewerClipPlanesOptions
            {
                StructSize = (uint)Marshal.SizeOf<NativeViewerClipPlanesOptions>(),
                ApiVersion = 1,
                Planes = pointer,
                Count = native.Length
            };
            EnsureInitialized();
            CheckPresentationStatus(PresentationNativeMethods.occt_engine_presentation_clip_planes_set(
                _handle,
                value.Id,
                in options));
        }
        finally
        {
            if (pinned.IsAllocated) pinned.Free();
        }
    }

    public void ClearObjectClipPlanes(IOcctObject value) =>
        SetObjectClipPlanes(value, Array.Empty<OcctViewClipPlane>());

    public void SetHighlightStyle(OcctHighlightStyleKind kind, OcctHighlightStyle style)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        var options = HighlightStyleOptions(kind, dynamic: false, style);
        EnsureInitialized();
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_highlight_style_global_set(
            _handle,
            in options));
    }

    public void SetObjectHighlightStyle(IOcctObject value, bool dynamic, OcctHighlightStyle style)
    {
        EnsureObject(value);
        var options = HighlightStyleOptions(OcctHighlightStyleKind.Dynamic, dynamic, style);
        EnsureInitialized();
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_highlight_style_object_set(
            _handle,
            value.Id,
            in options));
    }

    public void ClearObjectHighlightStyle(IOcctObject value, bool dynamic)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_highlight_style_object_clear(
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
        CheckPresentationStatus(PresentationNativeMethods.occt_engine_presentation_state_get(
            _handle,
            value.Id,
            out var state));
        if (state.ApiVersion != 1 ||
            state.StructSize < (uint)Marshal.SizeOf<NativeViewerPresentationState>())
        {
            throw new OcctException("Native presentation state ABI is incompatible with this SDK.");
        }
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

    private static NativeViewerHighlightStyleOptions HighlightStyleOptions(
        OcctHighlightStyleKind kind,
        bool dynamic,
        OcctHighlightStyle style) => new()
    {
        StructSize = (uint)Marshal.SizeOf<NativeViewerHighlightStyleOptions>(),
        ApiVersion = 1,
        Kind = (int)kind,
        Dynamic = dynamic ? 1 : 0,
        Settings = ToNativeHighlightStyle(style)
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

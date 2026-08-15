using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetAutoZFitMode(bool enabled, double scaleFactor = 1.0)
    {
        OcctGuard.Positive(scaleFactor, nameof(scaleFactor));
        UpdateDepth(new NativeViewerDepthUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerDepthUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerDepthUpdateMask.AutoZFitSettings,
            AutoZFitEnabled = enabled ? 1 : 0,
            AutoZFitScaleFactor = scaleFactor
        });
    }

    public OcctAutoZFitSettings GetAutoZFitSettings()
    {
        var state = GetDepthState();
        return new OcctAutoZFitSettings(state.AutoZFitEnabled != 0, state.AutoZFitScaleFactor);
    }

    public void AutoZFit() => UpdateDepth(new NativeViewerDepthUpdateOptions
    {
        StructSize = (uint)Marshal.SizeOf<NativeViewerDepthUpdateOptions>(),
        ApiVersion = 1,
        UpdateMask = NativeViewerDepthUpdateMask.AutoZFitNow
    });

    public void SetDefaultPolygonOffsets(
        OcctPolygonOffsetMode mode,
        double factor = 1.0,
        double units = 1.0,
        bool applyExisting = false)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        OcctGuard.Finite(factor, nameof(factor));
        OcctGuard.Finite(units, nameof(units));
        UpdateDepth(new NativeViewerDepthUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerDepthUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeViewerDepthUpdateMask.DefaultPolygonOffsets,
            PolygonOffsetMode = (int)mode,
            PolygonOffsetFactor = factor,
            PolygonOffsetUnits = units,
            ApplyPolygonOffsetsToExisting = applyExisting ? 1 : 0
        });
    }

    public OcctPolygonOffsetSettings GetDefaultPolygonOffsets()
    {
        var state = GetDepthState();
        return new OcctPolygonOffsetSettings(
            (OcctPolygonOffsetMode)state.PolygonOffsetMode,
            state.PolygonOffsetFactor,
            state.PolygonOffsetUnits);
    }

    public void SetPolygonOffsets(
        IOcctObject value,
        OcctPolygonOffsetMode mode,
        double factor = 1.0,
        double units = 1.0)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        OcctGuard.Finite(factor, nameof(factor));
        OcctGuard.Finite(units, nameof(units));
        UpdateObjectPolygonOffset(value, new NativeViewerObjectPolygonOffsetOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerObjectPolygonOffsetOptions>(),
            ApiVersion = 1,
            Mode = (int)mode,
            Factor = factor,
            Units = units
        });
    }

    public OcctPolygonOffsetSettings GetPolygonOffsets(IOcctObject value)
    {
        EnsureObject(value);
        EnsureInitialized();
        CheckDepthStatus(DepthNativeMethods.occt_engine_object_polygon_offset_get(
            _handle,
            value.Id,
            out var state));
        if (state.ApiVersion != 1 ||
            state.StructSize < (uint)Marshal.SizeOf<NativeViewerObjectPolygonOffsetState>())
        {
            throw new OcctException("Native object polygon offset state ABI is incompatible with this SDK.");
        }
        return new OcctPolygonOffsetSettings((OcctPolygonOffsetMode)state.Mode, state.Factor, state.Units);
    }

    public void ResetPolygonOffsets(IOcctObject value)
    {
        EnsureObject(value);
        UpdateObjectPolygonOffset(value, new NativeViewerObjectPolygonOffsetOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeViewerObjectPolygonOffsetOptions>(),
            ApiVersion = 1,
            ResetToDefault = 1
        });
    }

    private void UpdateDepth(NativeViewerDepthUpdateOptions options)
    {
        EnsureInitialized();
        CheckDepthStatus(DepthNativeMethods.occt_engine_depth_update(_handle, in options));
    }

    private NativeViewerDepthState GetDepthState()
    {
        EnsureInitialized();
        CheckDepthStatus(DepthNativeMethods.occt_engine_depth_state_get(_handle, out var state));
        if (state.ApiVersion != 1 || state.StructSize < (uint)Marshal.SizeOf<NativeViewerDepthState>())
            throw new OcctException("Native depth state ABI is incompatible with this SDK.");
        return state;
    }

    private void UpdateObjectPolygonOffset(
        IOcctObject value,
        NativeViewerObjectPolygonOffsetOptions options)
    {
        EnsureInitialized();
        CheckDepthStatus(DepthNativeMethods.occt_engine_object_polygon_offset_update(
            _handle,
            value.Id,
            in options));
    }

    private void CheckDepthStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }
}

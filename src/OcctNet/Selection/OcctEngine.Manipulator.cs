using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctManipulator AddManipulator()
    {
        EnsureInitialized();
        CheckManipulatorStatus(ManipulatorNativeMethods.occt_engine_manipulator_create(_handle, out var id));
        return CheckManipulator(id);
    }

    public void AttachManipulator(
        OcctManipulator manipulator,
        IEnumerable<IOcctObject> objects,
        OcctManipulatorAttachOptions? options = null)
    {
        EnsureManipulator(manipulator);
        var ids = GetObjectIds(objects, nameof(objects));
        if (ids.Length == 0) throw new ArgumentException("At least one target object is required.", nameof(objects));
        if (ids.Contains(manipulator.Id)) throw new ArgumentException("Manipulator cannot be attached to itself.", nameof(objects));

        options ??= new OcctManipulatorAttachOptions();
        var native = new NativeManipulatorAttachOptionsV1
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorAttachOptionsV1>(),
            ApiVersion = 1,
            AdjustPosition = options.AdjustPosition ? 1 : 0,
            AdjustSize = options.AdjustSize ? 1 : 0,
            EnableModes = options.EnableModes ? 1 : 0
        };

        var pinned = GCHandle.Alloc(ids, GCHandleType.Pinned);
        try
        {
            CheckManipulatorStatus(ManipulatorNativeMethods.occt_engine_manipulator_attach(
                _handle,
                manipulator.Id,
                pinned.AddrOfPinnedObject(),
                ids.Length,
                in native));
        }
        finally
        {
            pinned.Free();
        }
    }

    public void DetachManipulator(OcctManipulator manipulator)
    {
        EnsureManipulator(manipulator);
        CheckManipulatorStatus(ManipulatorNativeMethods.occt_engine_manipulator_detach(_handle, manipulator.Id));
    }

    public void SetManipulatorPart(
        OcctManipulator manipulator,
        OcctManipulatorMode mode,
        bool enabled,
        int? axisIndex = null)
    {
        EnsureManipulator(manipulator);
        ValidateManipulatorMode(mode);
        if (axisIndex is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(axisIndex));
        UpdateManipulator(manipulator, new NativeManipulatorUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeManipulatorUpdateMask.Part,
            AxisIndex = axisIndex ?? -1,
            Mode = (int)mode,
            Enabled = enabled ? 1 : 0
        });
    }

    public void SetManipulatorModeEnabled(
        OcctManipulator manipulator,
        OcctManipulatorMode mode,
        bool enabled)
    {
        EnsureManipulator(manipulator);
        ValidateManipulatorMode(mode);
        UpdateManipulator(manipulator, new NativeManipulatorUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeManipulatorUpdateMask.ModeEnabled,
            Mode = (int)mode,
            Enabled = enabled ? 1 : 0
        });
    }

    public void SetManipulatorModeActivationOnDetection(OcctManipulator manipulator, bool enabled)
    {
        EnsureManipulator(manipulator);
        UpdateManipulator(manipulator, new NativeManipulatorUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeManipulatorUpdateMask.ModeActivationOnDetection,
            Enabled = enabled ? 1 : 0
        });
    }

    public void SetManipulatorPosition(
        OcctManipulator manipulator,
        OcctPoint3d origin,
        OcctVector3d normal,
        OcctVector3d xDirection)
    {
        EnsureManipulator(manipulator);
        OcctGuard.Finite(origin, nameof(origin));
        OcctGuard.NonZero(normal, nameof(normal));
        OcctGuard.NonZero(xDirection, nameof(xDirection));
        var normalizedNormal = normal.Normalized();
        var normalizedX = xDirection.Normalized();
        if (Math.Abs(normalizedNormal.Dot(normalizedX)) > 1.0 - 1e-10)
            throw new ArgumentException("Manipulator normal and X direction must not be parallel.", nameof(xDirection));

        UpdateManipulator(manipulator, new NativeManipulatorUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeManipulatorUpdateMask.Position,
            Origin = origin,
            Normal = normal,
            XDirection = xDirection
        });
    }

    public void SetManipulatorSize(OcctManipulator manipulator, double size)
    {
        EnsureManipulator(manipulator);
        OcctGuard.Positive(size, nameof(size));
        UpdateManipulator(manipulator, new NativeManipulatorUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeManipulatorUpdateMask.Size,
            Size = size
        });
    }

    public void SetManipulatorGap(OcctManipulator manipulator, double gap)
    {
        EnsureManipulator(manipulator);
        if (!double.IsFinite(gap) || gap < 0.0) throw new ArgumentOutOfRangeException(nameof(gap));
        UpdateManipulator(manipulator, new NativeManipulatorUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeManipulatorUpdateMask.Gap,
            Gap = gap
        });
    }

    public void SetManipulatorZoomPersistence(OcctManipulator manipulator, bool enabled)
    {
        EnsureManipulator(manipulator);
        UpdateManipulator(manipulator, new NativeManipulatorUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeManipulatorUpdateMask.ZoomPersistence,
            Enabled = enabled ? 1 : 0
        });
    }

    public void SetManipulatorSkin(OcctManipulator manipulator, OcctManipulatorSkin skin)
    {
        EnsureManipulator(manipulator);
        if (!Enum.IsDefined(skin)) throw new ArgumentOutOfRangeException(nameof(skin));
        UpdateManipulator(manipulator, new NativeManipulatorUpdateOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorUpdateOptions>(),
            ApiVersion = 1,
            UpdateMask = NativeManipulatorUpdateMask.Skin,
            SkinMode = (int)skin
        });
    }

    public OcctManipulatorState GetManipulatorState(OcctManipulator manipulator)
    {
        EnsureManipulator(manipulator);
        CheckManipulatorStatus(ManipulatorNativeMethods.occt_engine_manipulator_state_get(
            _handle,
            manipulator.Id,
            out var native));
        if (native.ApiVersion != 1 || native.StructSize < (uint)Marshal.SizeOf<NativeManipulatorStateV1>())
            throw new OcctException("Native manipulator state ABI is incompatible with this SDK.");
        if (!Enum.IsDefined(typeof(OcctManipulatorMode), native.ActiveMode))
            throw new InvalidOperationException($"Native manipulator mode {native.ActiveMode} is not supported.");
        if (!Enum.IsDefined(typeof(OcctManipulatorSkin), native.SkinMode))
            throw new InvalidOperationException($"Native manipulator skin {native.SkinMode} is not supported.");
        return new OcctManipulatorState(
            native.Attached != 0,
            (OcctManipulatorMode)native.ActiveMode,
            native.ActiveAxisIndex,
            native.HasActiveTransformation != 0,
            native.ModeActivationOnDetection != 0,
            native.ZoomPersistence != 0,
            (OcctManipulatorSkin)native.SkinMode,
            native.Origin,
            native.Normal,
            native.XDirection,
            native.Size);
    }

    public IReadOnlyList<IOcctObject> GetManipulatorTargets(OcctManipulator manipulator)
    {
        EnsureManipulator(manipulator);
        CheckManipulatorStatus(ManipulatorNativeMethods.occt_engine_manipulator_targets_get(
            _handle,
            manipulator.Id,
            IntPtr.Zero,
            0,
            out var count));
        if (count == 0) return Array.Empty<IOcctObject>();
        if (count < 0) throw new InvalidOperationException("Native manipulator target count is invalid.");

        var ids = new long[count];
        var pinned = GCHandle.Alloc(ids, GCHandleType.Pinned);
        try
        {
            CheckManipulatorStatus(ManipulatorNativeMethods.occt_engine_manipulator_targets_get(
                _handle,
                manipulator.Id,
                pinned.AddrOfPinnedObject(),
                ids.Length,
                out var copied));
            if (copied != ids.Length)
                throw new InvalidOperationException("Manipulator target set changed while reading it.");
        }
        finally
        {
            pinned.Free();
        }
        return ids.Select(GetObject).ToArray();
    }

    public void StartManipulatorTransform(OcctManipulator manipulator, int x, int y) =>
        TransformManipulator(manipulator, NativeManipulatorTransformAction.Start, x, y, apply: false);

    public void UpdateManipulatorTransform(OcctManipulator manipulator, int x, int y) =>
        TransformManipulator(manipulator, NativeManipulatorTransformAction.Update, x, y, apply: false);

    public void StopManipulatorTransform(OcctManipulator manipulator, bool apply = true) =>
        TransformManipulator(manipulator, NativeManipulatorTransformAction.Stop, 0, 0, apply);

    public void DeactivateManipulatorMode(OcctManipulator manipulator) =>
        TransformManipulator(manipulator, NativeManipulatorTransformAction.DeactivateMode, 0, 0, apply: false);

    private void UpdateManipulator(OcctManipulator manipulator, NativeManipulatorUpdateOptions options)
    {
        EnsureInitialized();
        CheckManipulatorStatus(ManipulatorNativeMethods.occt_engine_manipulator_update(
            _handle,
            manipulator.Id,
            in options));
    }

    private void TransformManipulator(
        OcctManipulator manipulator,
        NativeManipulatorTransformAction action,
        int x,
        int y,
        bool apply)
    {
        EnsureManipulator(manipulator);
        var options = new NativeManipulatorTransformOptions
        {
            StructSize = (uint)Marshal.SizeOf<NativeManipulatorTransformOptions>(),
            ApiVersion = 1,
            Action = action,
            X = x,
            Y = y,
            Apply = apply ? 1 : 0
        };
        EnsureInitialized();
        CheckManipulatorStatus(ManipulatorNativeMethods.occt_engine_manipulator_transform(
            _handle,
            manipulator.Id,
            in options));
    }

    private void CheckManipulatorStatus(OcctStatus status)
    {
        if (status != OcctStatus.Ok) throw CreateException();
    }

    private static void ValidateManipulatorMode(OcctManipulatorMode mode)
    {
        if (mode == OcctManipulatorMode.None || !Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));
    }
}

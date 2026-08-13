namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctManipulator AddManipulator() =>
        CheckManipulator(ManipulatorNativeMethods.occt_add_manipulator(_handle));

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
        var native = new NativeOcctManipulatorAttachOptions
        {
            AdjustPosition = options.AdjustPosition ? 1 : 0,
            AdjustSize = options.AdjustSize ? 1 : 0,
            EnableModes = options.EnableModes ? 1 : 0
        };
        Check(ManipulatorNativeMethods.occt_attach_manipulator(
            _handle,
            manipulator.Id,
            ids,
            ids.Length,
            in native));
    }

    public void DetachManipulator(OcctManipulator manipulator)
    {
        EnsureManipulator(manipulator);
        Check(ManipulatorNativeMethods.occt_detach_manipulator(_handle, manipulator.Id));
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
        Check(ManipulatorNativeMethods.occt_set_manipulator_part(
            _handle,
            manipulator.Id,
            axisIndex ?? -1,
            (int)mode,
            enabled ? 1 : 0));
    }

    public void SetManipulatorModeEnabled(
        OcctManipulator manipulator,
        OcctManipulatorMode mode,
        bool enabled)
    {
        EnsureManipulator(manipulator);
        ValidateManipulatorMode(mode);
        Check(ManipulatorNativeMethods.occt_set_manipulator_mode_enabled(
            _handle,
            manipulator.Id,
            (int)mode,
            enabled ? 1 : 0));
    }

    public void SetManipulatorModeActivationOnDetection(OcctManipulator manipulator, bool enabled)
    {
        EnsureManipulator(manipulator);
        Check(ManipulatorNativeMethods.occt_set_manipulator_mode_activation_on_detection(
            _handle,
            manipulator.Id,
            enabled ? 1 : 0));
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
        Check(ManipulatorNativeMethods.occt_set_manipulator_position(
            _handle,
            manipulator.Id,
            origin,
            normal,
            xDirection));
    }

    public void SetManipulatorSize(OcctManipulator manipulator, double size)
    {
        EnsureManipulator(manipulator);
        OcctGuard.Positive(size, nameof(size));
        Check(ManipulatorNativeMethods.occt_set_manipulator_size(_handle, manipulator.Id, size));
    }

    public void SetManipulatorGap(OcctManipulator manipulator, double gap)
    {
        EnsureManipulator(manipulator);
        if (!double.IsFinite(gap) || gap < 0.0) throw new ArgumentOutOfRangeException(nameof(gap));
        Check(ManipulatorNativeMethods.occt_set_manipulator_gap(_handle, manipulator.Id, gap));
    }

    public void SetManipulatorZoomPersistence(OcctManipulator manipulator, bool enabled)
    {
        EnsureManipulator(manipulator);
        Check(ManipulatorNativeMethods.occt_set_manipulator_zoom_persistence(
            _handle,
            manipulator.Id,
            enabled ? 1 : 0));
    }

    public void SetManipulatorSkin(OcctManipulator manipulator, OcctManipulatorSkin skin)
    {
        EnsureManipulator(manipulator);
        if (!Enum.IsDefined(skin)) throw new ArgumentOutOfRangeException(nameof(skin));
        Check(ManipulatorNativeMethods.occt_set_manipulator_skin(_handle, manipulator.Id, (int)skin));
    }

    public OcctManipulatorState GetManipulatorState(OcctManipulator manipulator)
    {
        EnsureManipulator(manipulator);
        Check(ManipulatorNativeMethods.occt_get_manipulator_state(_handle, manipulator.Id, out var native));
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
        Check(ManipulatorNativeMethods.occt_get_manipulator_objects(
            _handle,
            manipulator.Id,
            null,
            0,
            out var count));
        if (count == 0) return Array.Empty<IOcctObject>();

        var ids = new long[count];
        Check(ManipulatorNativeMethods.occt_get_manipulator_objects(
            _handle,
            manipulator.Id,
            ids,
            ids.Length,
            out var copied));
        if (copied != ids.Length)
            throw new InvalidOperationException("Manipulator target set changed while reading it.");
        return ids.Select(GetObject).ToArray();
    }

    public void StartManipulatorTransform(OcctManipulator manipulator, int x, int y)
    {
        EnsureManipulator(manipulator);
        Check(ManipulatorNativeMethods.occt_start_manipulator_transform(_handle, manipulator.Id, x, y));
    }

    public void UpdateManipulatorTransform(OcctManipulator manipulator, int x, int y)
    {
        EnsureManipulator(manipulator);
        Check(ManipulatorNativeMethods.occt_update_manipulator_transform(_handle, manipulator.Id, x, y));
    }

    public void StopManipulatorTransform(OcctManipulator manipulator, bool apply = true)
    {
        EnsureManipulator(manipulator);
        Check(ManipulatorNativeMethods.occt_stop_manipulator_transform(
            _handle,
            manipulator.Id,
            apply ? 1 : 0));
    }

    public void DeactivateManipulatorMode(OcctManipulator manipulator)
    {
        EnsureManipulator(manipulator);
        Check(ManipulatorNativeMethods.occt_deactivate_manipulator_mode(_handle, manipulator.Id));
    }

    private static void ValidateManipulatorMode(OcctManipulatorMode mode)
    {
        if (mode == OcctManipulatorMode.None || !Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));
    }
}

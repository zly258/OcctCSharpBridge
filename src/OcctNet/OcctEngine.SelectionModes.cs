namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetSelectionModeActive(
        IOcctObject value,
        OcctSelectionMode mode,
        bool active,
        OcctSelectionModeConcurrency concurrency = OcctSelectionModeConcurrency.Multiple,
        bool force = false)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        if (!Enum.IsDefined(concurrency)) throw new ArgumentOutOfRangeException(nameof(concurrency));
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_object_mode_set_active(
            _handle,
            value.Id,
            (int)mode,
            active ? 1 : 0,
            (int)concurrency,
            force ? 1 : 0));
    }

    public void SetSelectionSensitivity(
        IOcctObject value,
        OcctSelectionMode mode,
        int sensitivity)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        if (sensitivity <= 0)
            throw new ArgumentOutOfRangeException(nameof(sensitivity), "Selection sensitivity must be greater than zero.");
        EnsureInitialized();
        CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_object_sensitivity_set(
            _handle,
            value.Id,
            (int)mode,
            sensitivity));
    }
}

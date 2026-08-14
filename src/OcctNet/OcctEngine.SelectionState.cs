using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetSelection(
        IEnumerable<IOcctObject> values,
        OcctSelectionOperation operation = OcctSelectionOperation.Replace)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (!Enum.IsDefined(operation)) throw new ArgumentOutOfRangeException(nameof(operation));
        EnsureInitialized();

        var ids = operation == OcctSelectionOperation.Clear
            ? Array.Empty<long>()
            : GetObjectIds(values, nameof(values));

        var buffer = IntPtr.Zero;
        try
        {
            if (ids.Length > 0)
            {
                buffer = Marshal.AllocHGlobal(sizeof(long) * ids.Length);
                Marshal.Copy(ids, 0, buffer, ids.Length);
            }

            var options = new NativeViewerObjectSelectionOptions
            {
                StructSize = (uint)Marshal.SizeOf<NativeViewerObjectSelectionOptions>(),
                ApiVersion = 1,
                ObjectIds = buffer,
                Count = ids.Length,
                Operation = (int)operation
            };
            CheckSelectionStatus(SelectionNativeMethods.occt_engine_selection_objects_update(
                _handle,
                in options));
        }
        finally
        {
            if (buffer != IntPtr.Zero) Marshal.FreeHGlobal(buffer);
        }
    }

    public IReadOnlyList<IOcctObject> GetSelectedObjects() =>
        SelectedObjects.Cast<IOcctObject>().ToArray();
}

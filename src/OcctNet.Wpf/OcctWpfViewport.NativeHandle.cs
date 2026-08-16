namespace OcctNet;

public sealed partial class OcctWpfViewport
{
    public event EventHandler<OcctNativeHandleChangedEventArgs>? NativeHandleChanged;

    private void SetNativeHandle(IntPtr nativeHandle, long generation)
    {
        if (_nativeHandle == nativeHandle) return;

        var previous = _nativeHandle;
        _nativeHandle = nativeHandle;
        try
        {
            NativeHandleChanged?.Invoke(
                this,
                new OcctNativeHandleChangedEventArgs(previous, nativeHandle, generation));
        }
        catch (Exception exception)
        {
            ReportLifecycleError(exception);
        }
    }
}

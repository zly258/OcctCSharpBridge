namespace OcctNet;

public sealed partial class OcctEngine
{
    internal void InitializeNativeSurface(OcctNativeSurfaceKind kind, IntPtr handle, IntPtr display = default)
    {
        EnsureNotDisposed();
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Native surface handle must not be zero.", nameof(handle));
        if (Volatile.Read(ref _initialized)) return;

        var surface = new NativeOcctSurface
        {
            Kind = (int)kind,
            Handle = handle,
            Display = display
        };

        Check(NativeMethods.occt_initialize_surface(_handle, in surface));
        Volatile.Write(ref _initialized, true);
    }
}

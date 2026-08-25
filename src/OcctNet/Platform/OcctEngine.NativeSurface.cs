namespace OcctNet;

public sealed partial class OcctEngine
{
    internal void InitializeNativeSurface(
        OcctNativeSurfaceKind kind,
        IntPtr handle,
        IntPtr display = default,
        bool redrawAfterInitialize = true)
    {
        lock (_lifecycleGate)
        {
            EnsureNotDisposed();
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Native surface handle must not be zero.", nameof(handle));
            if (Volatile.Read(ref _initialized)) return;

            var surface = new NativeOcctSurface
            {
                StructSize = (uint)Marshal.SizeOf<NativeOcctSurface>(),
                ApiVersion = 1,
                Kind = kind,
                Handle = handle,
                Display = display
            };

            var status = SurfaceNativeMethods.occt_engine_initialize_surface(_handle, in surface);
            if (status != OcctStatus.Ok) throw CreateException();

            _surfaceContext = SynchronizationContext.Current;
            Volatile.Write(ref _surfaceThreadId, Environment.CurrentManagedThreadId);
            Volatile.Write(ref _initialized, true);
        }

        if (redrawAfterInitialize) Redraw();
    }
}

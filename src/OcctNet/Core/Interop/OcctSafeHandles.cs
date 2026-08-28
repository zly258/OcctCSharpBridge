using Microsoft.Win32.SafeHandles;

namespace OcctNet;

internal sealed class OcctEngineSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private OcctEngineSafeHandle() : base(ownsHandle: true) { }

    internal static OcctEngineSafeHandle AdoptOwned(IntPtr nativeHandle)
    {
        var result = new OcctEngineSafeHandle();
        result.SetHandle(nativeHandle);
        return result;
    }

    protected override bool ReleaseHandle()
    {
        try { NativeMethods.occt_engine_destroy(handle); return true; }
        catch { return false; }
    }
}

internal sealed class OcctModelingSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private OcctModelingSafeHandle() : base(ownsHandle: true) { }

    internal static OcctModelingSafeHandle AdoptOwned(IntPtr nativeHandle)
    {
        var result = new OcctModelingSafeHandle();
        result.SetHandle(nativeHandle);
        return result;
    }

    protected override bool ReleaseHandle()
    {
        try { ModelNativeMethods.occt_model_session_destroy(handle); return true; }
        catch { return false; }
    }
}

internal sealed class OcctShapeSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private OcctShapeSafeHandle() : base(ownsHandle: true) { }

    internal static OcctShapeSafeHandle AdoptOwned(IntPtr nativeHandle)
    {
        var result = new OcctShapeSafeHandle();
        result.SetHandle(nativeHandle);
        return result;
    }

    protected override bool ReleaseHandle()
    {
        try { ModelNativeMethods.occt_shape_release(handle); return true; }
        catch { return false; }
    }
}

internal sealed class OcctMeshSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private OcctMeshSafeHandle() : base(ownsHandle: true) { }

    internal static OcctMeshSafeHandle AdoptOwned(IntPtr nativeHandle)
    {
        var result = new OcctMeshSafeHandle();
        result.SetHandle(nativeHandle);
        return result;
    }

    protected override bool ReleaseHandle()
    {
        try { ModelNativeMethods.occt_mesh_release(handle); return true; }
        catch { return false; }
    }
}

internal sealed class OcctAlgorithmSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private OcctAlgorithmSafeHandle() : base(ownsHandle: true) { }

    internal static OcctAlgorithmSafeHandle AdoptOwned(IntPtr nativeHandle)
    {
        var result = new OcctAlgorithmSafeHandle();
        result.SetHandle(nativeHandle);
        return result;
    }

    protected override bool ReleaseHandle()
    {
        try { ModelNativeMethods.occt_algorithm_release(handle); return true; }
        catch { return false; }
    }
}

internal sealed class OcctXdeDocumentSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private OcctXdeDocumentSafeHandle() : base(ownsHandle: true) { }

    internal static OcctXdeDocumentSafeHandle AdoptOwned(IntPtr nativeHandle)
    {
        var result = new OcctXdeDocumentSafeHandle();
        result.SetHandle(nativeHandle);
        return result;
    }

    protected override bool ReleaseHandle()
    {
        try { ModelNativeMethods.occt_xde_document_release(handle); return true; }
        catch { return false; }
    }
}

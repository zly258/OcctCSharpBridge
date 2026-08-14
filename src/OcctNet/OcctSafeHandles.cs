using Microsoft.Win32.SafeHandles;

namespace OcctNet;

internal sealed class OcctEngineSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private OcctEngineSafeHandle()
        : base(ownsHandle: true)
    {
    }
    internal static OcctEngineSafeHandle AdoptOwned(IntPtr nativeHandle)
    {
        var result = new OcctEngineSafeHandle();
        result.SetHandle(nativeHandle);
        return result;
    }

    protected override bool ReleaseHandle()
    {
        try
        {
            NativeMethods.occt_engine_destroy(handle);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

internal sealed class OcctModelingSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private OcctModelingSafeHandle()
        : base(ownsHandle: true)
    {
    }
    internal static OcctModelingSafeHandle AdoptOwned(IntPtr nativeHandle)
    {
        var result = new OcctModelingSafeHandle();
        result.SetHandle(nativeHandle);
        return result;
    }

    protected override bool ReleaseHandle()
    {
        try
        {
            ModelNativeMethods.occt_model_session_destroy(handle);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

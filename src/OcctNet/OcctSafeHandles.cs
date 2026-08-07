using Microsoft.Win32.SafeHandles;

namespace OcctNet;

internal sealed class OcctEngineSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal OcctEngineSafeHandle(IntPtr nativeHandle)
        : base(ownsHandle: true)
    {
        SetHandle(nativeHandle);
    }

    protected override bool ReleaseHandle()
    {
        try
        {
            NativeMethods.occt_destroy(handle);
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
    internal OcctModelingSafeHandle(IntPtr nativeHandle)
        : base(ownsHandle: true)
    {
        SetHandle(nativeHandle);
    }

    protected override bool ReleaseHandle()
    {
        try
        {
            ModelNativeMethods.occt_model_destroy(handle);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

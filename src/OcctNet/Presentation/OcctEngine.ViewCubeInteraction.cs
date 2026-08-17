namespace OcctNet;

public sealed partial class OcctEngine
{
    /// <summary>
    /// Lets the native AIS_ViewCube consume a click at device-pixel coordinates.
    /// Returns false when the click is not on the current ViewCube, leaving normal application input untouched.
    /// </summary>
    public bool TryHandleViewCubeClick(int x, int y)
    {
        EnsureInitialized();
        CheckViewStatus(ViewNativeMethods.occt_engine_view_cube_try_click(_handle, x, y, out var handled));
        return handled != 0;
    }
}

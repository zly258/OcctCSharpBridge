namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetObjectLineStyle(IOcctObject value, OcctLineStyle lineStyle)
    {
        EnsureObject(value);
        if (!Enum.IsDefined(lineStyle)) throw new ArgumentOutOfRangeException(nameof(lineStyle));
        CheckViewStatus(AppearanceNativeMethods.occt_engine_object_line_style_set(_handle, value.Id, (int)lineStyle));
    }
}

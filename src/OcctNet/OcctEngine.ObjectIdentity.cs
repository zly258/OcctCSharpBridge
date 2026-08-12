namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetApplicationTag(IOcctObject value, string applicationTag)
    {
        EnsureObject(value);
        CheckInitialized(() => NativeMethods.occt_set_object_application_tag(_handle, value.Id, applicationTag ?? string.Empty));
    }

    public string GetApplicationTag(IOcctObject value)
    {
        EnsureObject(value);
        return Marshal.PtrToStringUTF8(NativeMethods.occt_get_object_application_tag(_handle, value.Id)) ?? string.Empty;
    }

    public bool TryGetObjectByApplicationTag(string applicationTag, out IOcctObject? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationTag);
        EnsureNotDisposed();
        var id = NativeMethods.occt_find_object_by_application_tag(_handle, applicationTag);
        if (id <= 0 || NativeMethods.occt_object_exists(_handle, id) == 0)
        {
            value = null;
            return false;
        }

        value = CreateBoundObject(id, GetObjectKind(id));
        return true;
    }
}

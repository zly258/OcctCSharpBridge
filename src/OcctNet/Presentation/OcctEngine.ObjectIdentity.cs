using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctEngine
{
    public void SetObjectName(IOcctObject value, string? name)
    {
        EnsureObject(value);
        SetObjectUtf8(value.Id, NativeViewerObjectUpdateMask.Name, name);
    }

    public string GetObjectName(IOcctObject value)
    {
        EnsureObject(value);
        return ReadObjectUtf8(value.Id, applicationTag: false);
    }

    public void SetApplicationTag(IOcctObject value, string? tag)
    {
        EnsureObject(value);
        SetObjectUtf8(value.Id, NativeViewerObjectUpdateMask.ApplicationTag, tag);
    }

    public string GetApplicationTag(IOcctObject value)
    {
        EnsureObject(value);
        return ReadObjectUtf8(value.Id, applicationTag: true);
    }

    public IOcctObject? FindObjectByApplicationTag(string tag)
    {
        EnsureNotDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        CheckObjectStatus(ObjectNativeMethods.occt_engine_object_find_by_application_tag(
            _handle,
            tag,
            out var objectId,
            out var found));
        if (found == 0) return null;
        if (found != 1 || objectId <= 0)
            throw new OcctException("Native ApplicationTag lookup returned an invalid result.");
        return GetObject(objectId);
    }

    private void SetObjectUtf8(long objectId, NativeViewerObjectUpdateMask mask, string? value)
    {
        var pointer = Marshal.StringToCoTaskMemUTF8(value ?? string.Empty);
        try
        {
            var options = ObjectUpdateOptions(mask);
            if (mask == NativeViewerObjectUpdateMask.Name) options.Name = pointer;
            else if (mask == NativeViewerObjectUpdateMask.ApplicationTag) options.ApplicationTag = pointer;
            else throw new ArgumentOutOfRangeException(nameof(mask));
            UpdateObject(objectId, options);
        }
        finally
        {
            Marshal.FreeCoTaskMem(pointer);
        }
    }

    private string ReadObjectUtf8(long objectId, bool applicationTag)
    {
        EnsureNotDisposed();
        OcctStatus status;
        int required;
        if (applicationTag)
        {
            status = ObjectNativeMethods.occt_engine_object_application_tag_get(
                _handle,
                objectId,
                IntPtr.Zero,
                0,
                out required);
        }
        else
        {
            status = ObjectNativeMethods.occt_engine_object_name_get(
                _handle,
                objectId,
                IntPtr.Zero,
                0,
                out required);
        }
        CheckObjectStatus(status);
        if (required <= 0) throw new OcctException("Native UTF-8 string size is invalid.");

        var buffer = Marshal.AllocHGlobal(required);
        try
        {
            if (applicationTag)
            {
                status = ObjectNativeMethods.occt_engine_object_application_tag_get(
                    _handle,
                    objectId,
                    buffer,
                    required,
                    out var writtenRequired);
                if (writtenRequired != required)
                    throw new OcctException("Native ApplicationTag size changed during retrieval.");
            }
            else
            {
                status = ObjectNativeMethods.occt_engine_object_name_get(
                    _handle,
                    objectId,
                    buffer,
                    required,
                    out var writtenRequired);
                if (writtenRequired != required)
                    throw new OcctException("Native object name size changed during retrieval.");
            }
            CheckObjectStatus(status);
            return Marshal.PtrToStringUTF8(buffer) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}

using System.Text;

namespace OcctNet;

internal static class NativeError
{
    internal static (OcctStatus Status, string? Message) ReadEngine(OcctEngineSafeHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            return (OcctStatus.ErrorInvalidHandle, "Invalid OCCT engine handle.");

        var status = NativeMethods.occt_engine_last_error_code(handle);
        var message = ReadMessage((byte[]? buffer, int capacity, out int required) =>
            NativeMethods.occt_engine_last_error_message(handle, buffer, capacity, out required));
        return (status, message);
    }

    internal static (OcctStatus Status, string? Message) ReadModelingSession(OcctModelingSafeHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            return (OcctStatus.ErrorInvalidHandle, "Invalid OCCT modeling-session handle.");

        var status = ModelNativeMethods.occt_model_session_last_error_code(handle);
        var message = ReadMessage((byte[]? buffer, int capacity, out int required) =>
            ModelNativeMethods.occt_model_session_last_error_message(handle, buffer, capacity, out required));
        return (status, message);
    }

    private delegate OcctStatus CopyMessage(byte[]? buffer, int capacity, out int required);

    private static string? ReadMessage(CopyMessage copy)
    {
        var queryStatus = copy(null, 0, out var required);
        if (queryStatus != OcctStatus.Ok || required <= 1)
            return null;

        var buffer = new byte[required];
        var copyStatus = copy(buffer, buffer.Length, out var copiedRequired);
        if (copyStatus != OcctStatus.Ok || copiedRequired <= 1 || copiedRequired > buffer.Length)
            return null;

        return Encoding.UTF8.GetString(buffer, 0, copiedRequired - 1);
    }
}

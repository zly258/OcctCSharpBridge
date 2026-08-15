namespace OcctNet;

public sealed class OcctException : Exception
{
    public OcctException(string message)
        : this(message, OcctStatus.ErrorUnknown, null, null, null)
    {
    }

    public OcctException(string message, string? operation, string? nativeMessage = null, Exception? innerException = null)
        : this(message, OcctStatus.ErrorUnknown, operation, nativeMessage, innerException)
    {
    }

    public OcctException(
        string message,
        OcctStatus status,
        string? operation = null,
        string? nativeMessage = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Status = status;
        Operation = operation;
        NativeMessage = nativeMessage;
    }

    /// <summary>
    /// Gets the stable native bridge status associated with the failure.
    /// </summary>
    public OcctStatus Status { get; }

    /// <summary>
    /// Gets the managed bridge operation that reported the failure when available.
    /// </summary>
    public string? Operation { get; }

    /// <summary>
    /// Gets the original message returned by the native bridge when available.
    /// </summary>
    public string? NativeMessage { get; }
}

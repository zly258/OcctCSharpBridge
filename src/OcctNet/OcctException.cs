namespace OcctNet;

public sealed class OcctException : Exception
{
    public OcctException(string message)
        : this(message, null, null, null)
    {
    }

    public OcctException(string message, string? operation, string? nativeMessage = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Operation = operation;
        NativeMessage = nativeMessage;
    }

    /// <summary>
    /// Gets the managed bridge operation that reported the failure when available.
    /// </summary>
    public string? Operation { get; }

    /// <summary>
    /// Gets the original message returned by the native bridge when available.
    /// </summary>
    public string? NativeMessage { get; }
}

using System.Runtime.Serialization;

namespace OcctNet;

/// <summary>
/// Represents an error returned by the OcctCSharpBridge native or managed layer.
/// </summary>
[Serializable]
public sealed class OcctException : Exception
{
    /// <summary>Creates an OcctException with an unknown status.</summary>
    public OcctException(string message)
        : this(message, OcctStatus.ErrorUnknown, null, null, null)
    {
    }

    /// <summary>Creates an OcctException with an unknown status and optional operation context.</summary>
    public OcctException(string message, string? operation, string? nativeMessage = null, Exception? innerException = null)
        : this(message, OcctStatus.ErrorUnknown, operation, nativeMessage, innerException)
    {
    }

    /// <summary>Creates an OcctException with full context.</summary>
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

    /// <summary>
    /// Wraps an arbitrary exception in an <see cref="OcctException"/> if it is not already one.
    /// Returns the original exception unchanged when it is already an <see cref="OcctException"/>.
    /// </summary>
    public static OcctException Wrap(Exception exception, string? operation = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception is OcctException already) return already;
        var status = exception is ObjectDisposedException ? OcctStatus.ErrorInvalidHandle : OcctStatus.ErrorUnknown;
        return new OcctException(exception.Message, status, operation, null, exception);
    }

    /// <summary>
    /// Tries to execute <paramref name="action"/> and converts any exception to an
    /// <see cref="OcctException"/>. Returns <see langword="true"/> on success.
    /// </summary>
    public static bool TryCatch(Action action, out OcctException? error)
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            action();
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = Wrap(ex);
            return false;
        }
    }

    /// <summary>
    /// Tries to execute <paramref name="func"/> and returns a fallback value on failure,
    /// populating <paramref name="error"/> with the wrapped exception.
    /// </summary>
    public static T TryCatch<T>(Func<T> func, T fallback, out OcctException? error)
    {
        ArgumentNullException.ThrowIfNull(func);
        try
        {
            error = null;
            return func();
        }
        catch (Exception ex)
        {
            error = Wrap(ex);
            return fallback;
        }
    }

    /// <inheritdoc/>
    public override string ToString() =>
        Operation is null
            ? base.ToString()
            : $"{base.ToString()}{Environment.NewLine}Operation: {Operation}";
}

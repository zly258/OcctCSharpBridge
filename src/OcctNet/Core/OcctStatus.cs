namespace OcctNet;

/// <summary>
/// Stable native bridge result codes shared by viewer and headless modeling APIs.
/// All codes are non-positive: <see cref="Ok"/> is 0; all error codes are negative.
/// </summary>
public enum OcctStatus
{
    /// <summary>The operation succeeded.</summary>
    Ok = 0,

    /// <summary>An unclassified or unknown error occurred.</summary>
    ErrorUnknown = -1,

    /// <summary>A supplied argument value or combination is invalid.</summary>
    ErrorInvalidArgument = -2,

    /// <summary>The native handle is null, closed, or was not created successfully.</summary>
    ErrorInvalidHandle = -3,

    /// <summary>
    /// The engine or session was not initialized before performing an operation
    /// that requires it (e.g. <c>InitializeViewport</c>).
    /// </summary>
    ErrorNotInitialized = -4,

    /// <summary>The requested resource, ID, or object was not found.</summary>
    ErrorNotFound = -5,

    /// <summary>The object is in a state that does not permit the requested operation.</summary>
    ErrorInvalidState = -6,

    /// <summary>The caller-supplied buffer is too small to receive the result.</summary>
    ErrorBufferTooSmall = -7,

    /// <summary>A geometry-level error occurred (invalid curve, surface, or point).</summary>
    ErrorGeometry = -20,

    /// <summary>A topology-level error occurred (invalid shape structure).</summary>
    ErrorTopology = -21,

    /// <summary>An OCCT algorithm reported a failure (boolean, fillet, mesh, etc.).</summary>
    ErrorAlgorithm = -22,

    /// <summary>A file input/output error occurred.</summary>
    ErrorIo = -30,

    /// <summary>The file or data format is unrecognized or corrupted.</summary>
    ErrorFormat = -31,

    /// <summary>A platform-specific error occurred (window creation, OpenGL, etc.).</summary>
    ErrorPlatform = -40,

    /// <summary>The requested capability is not supported on this platform or configuration.</summary>
    ErrorNotSupported = -41,

    /// <summary>The operation was cancelled by the caller.</summary>
    ErrorCancelled = -42,

    /// <summary>A native memory allocation failed.</summary>
    ErrorOutOfMemory = -50,

    /// <summary>An exception was thrown inside the OCCT library itself.</summary>
    ErrorOcct = -60
}


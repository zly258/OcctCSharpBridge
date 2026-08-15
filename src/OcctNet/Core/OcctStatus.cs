namespace OcctNet;

/// <summary>
/// Stable native bridge result codes shared by viewer and headless modeling APIs.
/// </summary>
public enum OcctStatus
{
    Ok = 0,
    ErrorUnknown = -1,
    ErrorInvalidArgument = -2,
    ErrorInvalidHandle = -3,
    ErrorNotInitialized = -4,
    ErrorNotFound = -5,
    ErrorInvalidState = -6,
    ErrorBufferTooSmall = -7,
    ErrorGeometry = -20,
    ErrorTopology = -21,
    ErrorAlgorithm = -22,
    ErrorIo = -30,
    ErrorFormat = -31,
    ErrorPlatform = -40,
    ErrorNotSupported = -41,
    ErrorCancelled = -42,
    ErrorOutOfMemory = -50,
    ErrorOcct = -60
}

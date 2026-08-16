namespace OcctNet;

public sealed partial class OcctAvaloniaViewport
{
    private OcctViewportInitializationOptions _initialOptions = new();

    public OcctViewportInitializationOptions InitialOptions
    {
        get => _initialOptions;
        set => _initialOptions = value ?? throw new ArgumentNullException(nameof(value));
    }
}

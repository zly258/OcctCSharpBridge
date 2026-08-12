namespace OcctNet;

public sealed partial class OcctEngine
{
    /// <summary>
    /// Copies a headless modeling shape into this initialized AIS engine and displays it.
    /// The returned shape belongs to this <see cref="OcctEngine"/> instance.
    /// </summary>
    public OcctShape Display(OcctModelingSession model, OcctModelShape shape, bool fit = false)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureInitialized();
        var id = ModelNativeMethods.occt_model_display_in_engine(_handle, model.NativeHandle, shape.Id, fit ? 1 : 0);
        return CheckShape(id);
    }
}

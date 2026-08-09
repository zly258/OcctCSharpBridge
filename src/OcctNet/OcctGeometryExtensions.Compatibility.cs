using System.ComponentModel;

namespace OcctNet;

public static partial class OcctGeometryExtensions
{
    /// <summary>
    /// Bridge 2.5 source-compatibility extension. New code should use
    /// <see cref="OcctEngine.GetShapeBounds(OcctShape)"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static OcctBounds GetBounds(
        this OcctEngine engine,
        OcctShape shape)
    {
        ArgumentNullException.ThrowIfNull(engine);
        return engine.GetShapeBounds(shape);
    }
}

namespace OcctNet;

public sealed partial class OcctEngine
{
    public OcctShape MakeBRepText(string text, OcctBRepTextOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        OcctGuard.Finite(options.Position, nameof(options.Position));
        OcctGuard.NonZero(options.Normal, nameof(options.Normal));
        OcctGuard.NonZero(options.XDirection, nameof(options.XDirection));
        OcctGuard.Positive(options.Height, nameof(options.Height));
        if (!double.IsFinite(options.ExtrusionDepth) || options.ExtrusionDepth < 0.0)
            throw new ArgumentOutOfRangeException(nameof(options.ExtrusionDepth));

        EnsureInitialized();
        var status = ViewerShapeNativeMethods.occt_engine_brep_text_create(
            _handle,
            text,
            options.FontName ?? string.Empty,
            options.Position,
            options.Normal,
            options.XDirection,
            options.Height,
            options.ExtrusionDepth,
            options.Bold ? 1 : 0,
            options.Italic ? 1 : 0,
            (int)options.HorizontalAlignment,
            (int)options.VerticalAlignment,
            out var result);
        if (status != OcctStatus.Ok) throw CreateException();
        return CheckShape(result);
    }
}

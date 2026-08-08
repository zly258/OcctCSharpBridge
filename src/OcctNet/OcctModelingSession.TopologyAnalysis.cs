namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctFreeBoundsResult AnalyzeFreeBounds(
        OcctModelShape shape,
        double tolerance = 1e-7,
        bool splitClosed = true,
        bool splitOpen = true)
    {
        EnsureShape(shape);
        OcctGuard.Finite(tolerance, nameof(tolerance));
        if (tolerance <= 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "Tolerance must be greater than zero.");

        const int closedKind = 0;
        const int openKind = 1;
        var splitClosedNative = splitClosed ? 1 : 0;
        var splitOpenNative = splitOpen ? 1 : 0;

        var closedCompound = CheckShape(ModelNativeMethods.occt_model_shape_free_bounds(
            _handle,
            shape.Id,
            tolerance,
            closedKind,
            splitClosedNative,
            splitOpenNative));
        var openCompound = CheckShape(ModelNativeMethods.occt_model_shape_free_bounds(
            _handle,
            shape.Id,
            tolerance,
            openKind,
            splitClosedNative,
            splitOpenNative));

        return new OcctFreeBoundsResult(
            tolerance,
            GetWires(closedCompound),
            GetWires(openCompound));
    }
}

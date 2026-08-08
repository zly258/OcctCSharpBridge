using System.Runtime.CompilerServices;
using OcctNet;

internal static class FreeBoundsSmoke
{
    [ModuleInitializer]
    internal static void Run()
    {
        using var model = new OcctModelingSession();
        var face = model.MakeRectangleFace(120, 80);

        var candidates = model.GetBoundaryEdgeCandidates(face);
        if (candidates.Count != 4)
            throw new InvalidOperationException($"Rectangle face boundary candidate count is {candidates.Count}, expected 4.");

        var freeBounds = model.AnalyzeFreeBounds(face, tolerance: 1e-7);
        if (!freeBounds.HasFreeBounds || freeBounds.ClosedWireCount < 1)
            throw new InvalidOperationException("Rectangle face has no closed free boundary.");
        if (freeBounds.OpenWireCount != 0 || freeBounds.HasOpenFreeBounds)
            throw new InvalidOperationException("Rectangle face unexpectedly has an open free boundary.");
        if (Math.Abs(freeBounds.Tolerance - 1e-7) > 1e-15)
            throw new InvalidOperationException("Free-boundary tolerance was not preserved in the managed result.");
    }
}

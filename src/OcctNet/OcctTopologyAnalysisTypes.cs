namespace OcctNet;

/// <summary>
/// Read-only result of strict OCCT free-boundary analysis.
/// Closed and open wires are owned by the same modeling session as the analyzed shape.
/// </summary>
public sealed class OcctFreeBoundsResult
{
    internal OcctFreeBoundsResult(
        double tolerance,
        IReadOnlyList<OcctModelShape> closedWires,
        IReadOnlyList<OcctModelShape> openWires)
    {
        Tolerance = tolerance;
        ClosedWires = closedWires;
        OpenWires = openWires;
    }

    public double Tolerance { get; }
    public IReadOnlyList<OcctModelShape> ClosedWires { get; }
    public IReadOnlyList<OcctModelShape> OpenWires { get; }
    public int ClosedWireCount => ClosedWires.Count;
    public int OpenWireCount => OpenWires.Count;
    public int TotalWireCount => checked(ClosedWireCount + OpenWireCount);
    public bool HasFreeBounds => TotalWireCount > 0;
    public bool HasOpenFreeBounds => OpenWireCount > 0;
}

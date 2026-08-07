using OcctNet;
using OcctScript.Domain;

namespace OcctScript.Geometry;

internal static class ShapeBuilderUtilities
{
    public static CommandTopologyKind ToTopology(OcctShapeType type) => type switch
    {
        OcctShapeType.Vertex => CommandTopologyKind.Vertex,
        OcctShapeType.Edge => CommandTopologyKind.Edge,
        OcctShapeType.Wire => CommandTopologyKind.Wire,
        OcctShapeType.Face => CommandTopologyKind.Face,
        OcctShapeType.Shell => CommandTopologyKind.Shell,
        OcctShapeType.Solid => CommandTopologyKind.Solid,
        OcctShapeType.CompSolid => CommandTopologyKind.CompSolid,
        OcctShapeType.Compound => CommandTopologyKind.Compound,
        _ => CommandTopologyKind.None
    };

    public static bool Matches(OcctModelingSession session, OcctModelShape shape, CommandTopologyKind expected) =>
        expected == CommandTopologyKind.Any || (ToTopology(session.GetShapeType(shape)) & expected) != 0;

    public static (OcctModelShape Wire, bool OwnsWire) ToWire(OcctModelingSession session, OcctModelShape shape)
    {
        var type = session.GetShapeType(shape);
        return type switch
        {
            OcctShapeType.Wire => (shape, false),
            OcctShapeType.Edge => (session.MakeWire([shape]), true),
            OcctShapeType.Face => (session.GetOuterWire(shape), false),
            _ => throw new InvalidOperationException($"Shape type '{type}' cannot be converted to a wire.")
        };
    }

    public static IReadOnlyList<OcctModelShape> ExpandEdges(OcctModelingSession session, IEnumerable<OcctModelShape> shapes)
    {
        var result = new List<OcctModelShape>();
        foreach (var shape in shapes)
        {
            var type = session.GetShapeType(shape);
            if (type == OcctShapeType.Edge)
                result.Add(shape);
            else if (type == OcctShapeType.Wire)
                result.AddRange(session.GetSubshapes(shape, OcctShapeType.Edge));
            else
                throw new InvalidOperationException($"Shape type '{type}' cannot be used as a wire curve.");
        }
        if (result.Count == 0) throw new InvalidOperationException("No edges were supplied.");
        return result;
    }

    public static ShapeBuildResult FromAlgorithm(OcctModelAlgorithmResult result, string failureCode)
    {
        var messages = new List<ShapeBuildMessage>();
        if (result.HasWarnings)
            messages.Add(ShapeBuildMessage.Warning(failureCode + "_WARNING", string.IsNullOrWhiteSpace(result.Report) ? "OCCT completed with warnings." : result.Report));
        if (result.HasErrors)
            messages.Add(ShapeBuildMessage.Error(failureCode, string.IsNullOrWhiteSpace(result.Report) ? "OCCT reported an algorithm error." : result.Report));
        return result.Succeeded && !result.HasErrors
            ? ShapeBuildResult.Succeeded(result.Shape, messages)
            : new ShapeBuildResult(false, default, messages.Count == 0 ? [ShapeBuildMessage.Error(failureCode, "OCCT algorithm failed.")] : messages);
    }

    public static OcctModelShape ApplyTransform(OcctModelingSession session, OcctModelShape source, TransformDefinition transform)
    {
        if (!double.IsFinite(transform.Scale) || transform.Scale <= 0)
            throw new InvalidOperationException("Command scale must be finite and greater than zero.");

        var result = source;
        var origin = OcctPoint3d.Origin;
        if (Math.Abs(transform.Scale - 1.0) > 1e-12)
            result = Replace(session, result, session.Scale(result, origin, transform.Scale));
        if (Math.Abs(transform.RotationX) > 1e-12)
            result = Replace(session, result, session.Rotate(result, origin, OcctVector3d.UnitX, transform.RotationX));
        if (Math.Abs(transform.RotationY) > 1e-12)
            result = Replace(session, result, session.Rotate(result, origin, OcctVector3d.UnitY, transform.RotationY));
        if (Math.Abs(transform.RotationZ) > 1e-12)
            result = Replace(session, result, session.Rotate(result, origin, OcctVector3d.UnitZ, transform.RotationZ));
        if (Math.Abs(transform.X) > 1e-12 || Math.Abs(transform.Y) > 1e-12 || Math.Abs(transform.Z) > 1e-12)
            result = Replace(session, result, session.Translate(result, new OcctVector3d(transform.X, transform.Y, transform.Z)));
        return result;
    }

    public static OcctModelShape Replace(OcctModelingSession session, OcctModelShape previous, OcctModelShape next)
    {
        if (previous.IsValid && previous.Id != next.Id && session.Exists(previous)) session.Delete(previous);
        return next;
    }

    public static void DeleteTemporary(OcctModelingSession session, OcctModelShape shape, bool owned)
    {
        if (owned && shape.IsValid && session.Exists(shape)) session.Delete(shape);
    }
}

namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelParameterRange GetEdgeParameterRange(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_parameter_range(_handle, edge.Id, out var result));
        return result;
    }

    public OcctModelCurveDifferential EvaluateEdgeAtParameter(OcctModelShape edge, double parameter)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_differential(_handle, edge.Id, parameter, out var result));
        return result;
    }

    public OcctModelCurveCurvature GetEdgeCurvature(
        OcctModelShape edge,
        double parameter,
        double resolution = 1e-9)
    {
        EnsureShape(edge);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(resolution);
        Check(ModelNativeMethods.occt_model_edge_curvature(
            _handle, edge.Id, parameter, resolution, out var result));
        return result;
    }

    public OcctModelSurfacePeriodicity GetFacePeriodicity(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_periodicity(_handle, face.Id, out var result));
        return result;
    }

    public OcctModelSurfaceDifferential EvaluateFaceDifferential(
        OcctModelShape face,
        double u,
        double v,
        double resolution = 1e-9)
    {
        EnsureShape(face);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(resolution);
        Check(ModelNativeMethods.occt_model_face_differential(
            _handle, face.Id, u, v, resolution, out var result));
        return result;
    }

    public OcctModelSurfaceCurvature GetFaceCurvature(
        OcctModelShape face,
        double u,
        double v,
        double resolution = 1e-9)
    {
        EnsureShape(face);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(resolution);
        Check(ModelNativeMethods.occt_model_face_curvature(
            _handle, face.Id, u, v, resolution, out var result));
        return result;
    }
}

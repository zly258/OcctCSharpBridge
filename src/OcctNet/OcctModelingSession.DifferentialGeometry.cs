namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelParameterRange GetEdgeParameterRange(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_parameter_range(_handle, edge.Id, out var native));
        return native.ToManaged();
    }

    public OcctModelCurveDifferential EvaluateEdgeAtParameter(OcctModelShape edge, double parameter)
    {
        EnsureShape(edge);
        OcctGuard.Finite(parameter, nameof(parameter));
        Check(ModelNativeMethods.occt_model_edge_differential(_handle, edge.Id, parameter, out var native));
        return native.ToManaged();
    }

    public OcctModelCurveCurvature GetEdgeCurvature(
        OcctModelShape edge,
        double parameter,
        double resolution = 1e-9)
    {
        EnsureShape(edge);
        OcctGuard.Finite(parameter, nameof(parameter));
        OcctGuard.Positive(resolution, nameof(resolution));
        Check(ModelNativeMethods.occt_model_edge_curvature(
            _handle,
            edge.Id,
            parameter,
            resolution,
            out var native));
        return native.ToManaged();
    }

    public OcctModelSurfacePeriodicity GetFacePeriodicity(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_periodicity(_handle, face.Id, out var native));
        return native.ToManaged();
    }

    public OcctModelSurfaceDifferential EvaluateFaceDifferential(
        OcctModelShape face,
        double u,
        double v,
        double resolution = 1e-9)
    {
        EnsureShape(face);
        OcctGuard.Finite(u, nameof(u));
        OcctGuard.Finite(v, nameof(v));
        OcctGuard.Positive(resolution, nameof(resolution));
        Check(ModelNativeMethods.occt_model_face_differential(
            _handle,
            face.Id,
            u,
            v,
            resolution,
            out var native));
        return native.ToManaged();
    }

    public OcctModelSurfaceCurvature GetFaceCurvature(
        OcctModelShape face,
        double u,
        double v,
        double resolution = 1e-9)
    {
        EnsureShape(face);
        OcctGuard.Finite(u, nameof(u));
        OcctGuard.Finite(v, nameof(v));
        OcctGuard.Positive(resolution, nameof(resolution));
        Check(ModelNativeMethods.occt_model_face_curvature(
            _handle,
            face.Id,
            u,
            v,
            resolution,
            out var native));
        return native.ToManaged();
    }
}

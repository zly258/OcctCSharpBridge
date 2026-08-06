namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctLineGeometry GetLineGeometry(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_line_geometry(_handle, edge.Id, out var result));
        return result;
    }

    public OcctCircleGeometry GetCircleGeometry(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_circle_geometry(_handle, edge.Id, out var result));
        return result;
    }

    public OcctEllipseGeometry GetEllipseGeometry(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_ellipse_geometry(_handle, edge.Id, out var result));
        return result;
    }

    public OcctPlaneGeometry GetPlaneGeometry(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_plane_geometry(_handle, face.Id, out var result));
        return result;
    }

    public OcctCylinderGeometry GetCylinderGeometry(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_cylinder_geometry(_handle, face.Id, out var result));
        return result;
    }

    public OcctConeGeometry GetConeGeometry(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_cone_geometry(_handle, face.Id, out var result));
        return result;
    }

    public OcctSphereGeometry GetSphereGeometry(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_sphere_geometry(_handle, face.Id, out var result));
        return result;
    }

    public OcctTorusGeometry GetTorusGeometry(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_torus_geometry(_handle, face.Id, out var result));
        return result;
    }
}

#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepTools.hxx>
#include <TopAbs_Orientation.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    int occt_model_vertex_point(OcctModelHandle handle, OcctObjectId vertexId, OcctPoint3d* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(vertexId);
            if (shape.ShapeType() != TopAbs_VERTEX) throw std::invalid_argument("Input must be a vertex.");
            const gp_Pnt point = BRep_Tool::Pnt(TopoDS::Vertex(shape));
            *result = {point.X(), point.Y(), point.Z()};
        });
    }

    int occt_model_edge_endpoints(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end)
    {
        ModelSession* model = modelOf(handle);
        if (start == nullptr || end == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            BRepAdaptor_Curve curve(TopoDS::Edge(shape));
            const gp_Pnt first = curve.Value(curve.FirstParameter());
            const gp_Pnt last = curve.Value(curve.LastParameter());
            *start = {first.X(), first.Y(), first.Z()};
            *end = {last.X(), last.Y(), last.Z()};
        });
    }

    int occt_model_edge_point_at(OcctModelHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* resultPoint, OcctVector3d* resultTangent)
    {
        ModelSession* model = modelOf(handle);
        if (resultPoint == nullptr || resultTangent == nullptr) return 0;
        return execute(model, [&]
        {
            if (normalizedParameter < 0.0 || normalizedParameter > 1.0)
                throw std::invalid_argument("Normalized parameter must be between 0 and 1.");
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            BRepAdaptor_Curve curve(TopoDS::Edge(shape));
            const double parameter = curve.FirstParameter() + (curve.LastParameter() - curve.FirstParameter()) * normalizedParameter;
            gp_Pnt point;
            gp_Vec tangent;
            curve.D1(parameter, point, tangent);
            if (tangent.SquareMagnitude() <= Precision::SquareConfusion()) throw std::runtime_error("Edge tangent is undefined at this parameter.");
            tangent.Normalize();
            *resultPoint = {point.X(), point.Y(), point.Z()};
            *resultTangent = {tangent.X(), tangent.Y(), tangent.Z()};
        });
    }

    int occt_model_edge_curve_type(OcctModelHandle handle, OcctObjectId edgeId)
    {
        ModelSession* model = modelOf(handle);
        return executeValue(model, static_cast<int>(OcctCurve_Other), [&]
        {
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            return static_cast<int>(BRepAdaptor_Curve(TopoDS::Edge(shape)).GetType());
        });
    }

    int occt_model_face_surface_type(OcctModelHandle handle, OcctObjectId faceId)
    {
        ModelSession* model = modelOf(handle);
        return executeValue(model, static_cast<int>(OcctSurface_Other), [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            return static_cast<int>(BRepAdaptor_Surface(TopoDS::Face(shape), Standard_True).GetType());
        });
    }

    int occt_model_face_uv_bounds(OcctModelHandle handle, OcctObjectId faceId, OcctUvBounds* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            BRepTools::UVBounds(TopoDS::Face(shape), result->uMin, result->uMax, result->vMin, result->vMax);
        });
    }

    int occt_model_face_point_normal(OcctModelHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* resultPoint, OcctVector3d* resultNormal)
    {
        ModelSession* model = modelOf(handle);
        if (resultPoint == nullptr || resultNormal == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            const TopoDS_Face face = TopoDS::Face(shape);
            BRepAdaptor_Surface surface(face, Standard_True);
            gp_Pnt point;
            gp_Vec dU;
            gp_Vec dV;
            surface.D1(u, v, point, dU, dV);
            gp_Vec normal = dU.Crossed(dV);
            if (normal.SquareMagnitude() <= Precision::SquareConfusion()) throw std::runtime_error("Face normal is undefined at this UV position.");
            normal.Normalize();
            if (face.Orientation() == TopAbs_REVERSED) normal.Reverse();
            *resultPoint = {point.X(), point.Y(), point.Z()};
            *resultNormal = {normal.X(), normal.Y(), normal.Z()};
        });
    }
}

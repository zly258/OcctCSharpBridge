#include "OcctInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRep_Tool.hxx>
#include <BRepTools.hxx>
#include <GeomAbs_CurveType.hxx>
#include <GeomAbs_SurfaceType.hxx>
#include <Precision.hxx>
#include <TopAbs_Orientation.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>
#include <TopoDS_Vertex.hxx>

using namespace OcctBridge;

namespace
{
    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
        return *entry;
    }
}

extern "C"
{
    std::int64_t occt_shape_hash(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h);
        const ObjectEntry* entry = e == nullptr ? nullptr : e->findShape(id);
        if (entry == nullptr) return 0;
        return static_cast<std::int64_t>(TopTools_ShapeMapHasher{}(entry->shape));
    }

    int occt_vertex_point(OcctHandle h, OcctObjectId id, OcctPoint3d* result)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredShape(e, id);
            if (entry.shape.ShapeType() != TopAbs_VERTEX) throw std::invalid_argument("Input must be a vertex.");
            const gp_Pnt value = BRep_Tool::Pnt(TopoDS::Vertex(entry.shape));
            *result = {value.X(), value.Y(), value.Z()};
        });
    }

    int occt_edge_endpoints(OcctHandle h, OcctObjectId id, OcctPoint3d* start, OcctPoint3d* end)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || start == nullptr || end == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredShape(e, id);
            if (entry.shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            BRepAdaptor_Curve curve(TopoDS::Edge(entry.shape));
            const gp_Pnt first = curve.Value(curve.FirstParameter());
            const gp_Pnt last = curve.Value(curve.LastParameter());
            *start = {first.X(), first.Y(), first.Z()};
            *end = {last.X(), last.Y(), last.Z()};
        });
    }

    int occt_edge_point_at(OcctHandle h, OcctObjectId id, double normalizedParameter, OcctPoint3d* resultPoint, OcctVector3d* resultTangent)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || resultPoint == nullptr || resultTangent == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredShape(e, id);
            if (entry.shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            if (normalizedParameter < 0.0 || normalizedParameter > 1.0) throw std::invalid_argument("Normalized parameter must be between 0 and 1.");
            BRepAdaptor_Curve curve(TopoDS::Edge(entry.shape));
            const double parameter = curve.FirstParameter() + (curve.LastParameter() - curve.FirstParameter()) * normalizedParameter;
            gp_Pnt value; gp_Vec tangent; curve.D1(parameter, value, tangent);
            if (tangent.SquareMagnitude() <= Precision::SquareConfusion()) throw std::runtime_error("Edge tangent is undefined at this parameter.");
            tangent.Normalize();
            *resultPoint = {value.X(), value.Y(), value.Z()};
            *resultTangent = {tangent.X(), tangent.Y(), tangent.Z()};
        });
    }

    int occt_edge_curve_type(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); const ObjectEntry* entry = e == nullptr ? nullptr : e->findShape(id);
        if (entry == nullptr || entry->shape.ShapeType() != TopAbs_EDGE) return OcctCurve_Other;
        return static_cast<int>(BRepAdaptor_Curve(TopoDS::Edge(entry->shape)).GetType());
    }

    int occt_face_surface_type(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); const ObjectEntry* entry = e == nullptr ? nullptr : e->findShape(id);
        if (entry == nullptr || entry->shape.ShapeType() != TopAbs_FACE) return OcctSurface_Other;
        return static_cast<int>(BRepAdaptor_Surface(TopoDS::Face(entry->shape), Standard_True).GetType());
    }

    int occt_face_uv_bounds(OcctHandle h, OcctObjectId id, OcctUvBounds* result)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredShape(e, id);
            if (entry.shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            BRepTools::UVBounds(TopoDS::Face(entry.shape), result->uMin, result->uMax, result->vMin, result->vMax);
        });
    }

    int occt_face_point_normal(OcctHandle h, OcctObjectId id, double u, double v, OcctPoint3d* resultPoint, OcctVector3d* resultNormal)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || resultPoint == nullptr || resultNormal == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredShape(e, id);
            if (entry.shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            const TopoDS_Face face = TopoDS::Face(entry.shape);
            BRepAdaptor_Surface surface(face, Standard_True);
            gp_Pnt value; gp_Vec dU; gp_Vec dV; surface.D1(u, v, value, dU, dV);
            gp_Vec normal = dU.Crossed(dV);
            if (normal.SquareMagnitude() <= Precision::SquareConfusion()) throw std::runtime_error("Face normal is undefined at this UV position.");
            normal.Normalize();
            if (face.Orientation() == TopAbs_REVERSED) normal.Reverse();
            *resultPoint = {value.X(), value.Y(), value.Z()};
            *resultNormal = {normal.X(), normal.Y(), normal.Z()};
        });
    }
}

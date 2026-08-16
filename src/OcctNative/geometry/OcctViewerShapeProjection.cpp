#include "geometry/OcctViewerShapeQueries.h"
#include "core/OcctInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepBuilderAPI_MakeVertex.hxx>
#include <BRepExtrema_DistShapeShape.hxx>
#include <BRepExtrema_SupportType.hxx>
#include <GeomAdaptor.hxx>
#include <GeomAPI_ProjectPointOnSurf.hxx>
#include <Precision.hxx>
#include <TopoDS.hxx>

#include <algorithm>
#include <cmath>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeShapeStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr || entry->shape.IsNull())
            throw std::invalid_argument("Shape ID does not exist.");
        return *entry;
    }

    void requireFinitePoint(OcctPoint3d value)
    {
        if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
            throw std::invalid_argument("Projection source point must be finite.");
    }

    BRepExtrema_DistShapeShape closestPointOnShape(
        const gp_Pnt& source,
        const TopoDS_Shape& target)
    {
        BRepBuilderAPI_MakeVertex sourceBuilder(source);
        if (!sourceBuilder.IsDone())
            throw std::runtime_error("Unable to construct the projection source vertex.");

        BRepExtrema_DistShapeShape distance(sourceBuilder.Vertex(), target);
        if (!distance.IsDone() || distance.NbSolution() < 1)
            throw std::runtime_error("Point projection distance calculation failed.");
        return distance;
    }

    double normalizedEdgeParameter(
        const BRepExtrema_DistShapeShape& distance,
        const TopoDS_Edge& edge,
        const gp_Pnt& projected)
    {
        BRepAdaptor_Curve curve(edge);
        const double first = curve.FirstParameter();
        const double last = curve.LastParameter();
        if (!std::isfinite(first) || !std::isfinite(last) || first == last)
            throw std::runtime_error("Edge has an invalid parameter range.");

        double parameter = first;
        switch (distance.SupportTypeShape2(1))
        {
            case BRepExtrema_IsOnEdge:
                distance.ParOnEdgeS2(1, parameter);
                break;
            case BRepExtrema_IsVertex:
            {
                const gp_Pnt firstPoint = curve.Value(first);
                const gp_Pnt lastPoint = curve.Value(last);
                parameter = projected.SquareDistance(firstPoint) <= projected.SquareDistance(lastPoint)
                    ? first
                    : last;
                break;
            }
            default:
                throw std::runtime_error("Edge projection returned an unexpected support type.");
        }

        if (!std::isfinite(parameter))
            throw std::runtime_error("Edge projection returned a non-finite parameter.");
        return std::clamp((parameter - first) / (last - first), 0.0, 1.0);
    }

    void faceParameters(
        const BRepExtrema_DistShapeShape& distance,
        const TopoDS_Face& face,
        const gp_Pnt& projected,
        double& u,
        double& v)
    {
        if (distance.SupportTypeShape2(1) == BRepExtrema_IsInFace)
        {
            distance.ParOnFaceS2(1, u, v);
        }
        else
        {
            BRepAdaptor_Surface adaptor(face, Standard_True);
            const Handle(Geom_Surface) surface = GeomAdaptor::MakeSurface(adaptor, false);
            if (surface.IsNull())
                throw std::runtime_error("Unable to access the face surface for projection parameters.");

            GeomAPI_ProjectPointOnSurf projection(
                projected,
                surface,
                adaptor.FirstUParameter(),
                adaptor.LastUParameter(),
                adaptor.FirstVParameter(),
                adaptor.LastVParameter(),
                Precision::Confusion());
            if (!projection.IsDone() || projection.NbPoints() < 1)
                throw std::runtime_error("Unable to recover face parameters for the projected point.");
            projection.LowerDistanceParameters(u, v);
        }

        if (!std::isfinite(u) || !std::isfinite(v))
            throw std::runtime_error("Face projection returned non-finite parameters.");
    }
}

extern "C"
{
    OcctStatus occt_engine_shape_edge_project_point(
        OcctEngineHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d sourcePoint,
        OcctEdgeProjectionResult* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Edge projection output is null.");
            requireFinitePoint(sourcePoint);

            const TopoDS_Shape transformed =
                shapeWithPresentationTransformation(requiredShape(engine, edgeId));
            if (transformed.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Input must be an edge.");

            const TopoDS_Edge edge = TopoDS::Edge(transformed);
            BRepExtrema_DistShapeShape distance = closestPointOnShape(point(sourcePoint), edge);
            const gp_Pnt projected = distance.PointOnShape2(1);
            const double normalizedParameter = normalizedEdgeParameter(distance, edge, projected);
            const double value = distance.Value();
            if (!std::isfinite(value))
                throw std::runtime_error("Edge projection returned a non-finite distance.");

            result->point = {projected.X(), projected.Y(), projected.Z()};
            result->normalizedParameter = normalizedParameter;
            result->distance = value;
        });
    }

    OcctStatus occt_engine_shape_face_project_point(
        OcctEngineHandle handle,
        OcctObjectId faceId,
        OcctPoint3d sourcePoint,
        OcctFaceProjectionResult* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Face projection output is null.");
            requireFinitePoint(sourcePoint);

            const TopoDS_Shape transformed =
                shapeWithPresentationTransformation(requiredShape(engine, faceId));
            if (transformed.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Input must be a face.");

            const TopoDS_Face face = TopoDS::Face(transformed);
            BRepExtrema_DistShapeShape distance = closestPointOnShape(point(sourcePoint), face);
            const gp_Pnt projected = distance.PointOnShape2(1);
            double u = 0.0;
            double v = 0.0;
            faceParameters(distance, face, projected, u, v);
            const double value = distance.Value();
            if (!std::isfinite(value))
                throw std::runtime_error("Face projection returned a non-finite distance.");

            result->point = {projected.X(), projected.Y(), projected.Z()};
            result->u = u;
            result->v = v;
            result->distance = value;
        });
    }
}

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
#include <IntTools_CommonPrt.hxx>
#include <IntTools_EdgeEdge.hxx>
#include <IntTools_Range.hxx>

#include <algorithm>
#include <cmath>
#include <limits>
#include <stdexcept>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeShapeStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
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

    gp_Vec edgeTangent(const TopoDS_Edge& edge, double normalizedParameter)
    {
        BRepAdaptor_Curve curve(edge);
        const double first = curve.FirstParameter();
        const double last = curve.LastParameter();
        const double parameter = first + (last - first) * normalizedParameter;
        gp_Pnt value;
        gp_Vec tangent;
        curve.D1(parameter, value, tangent);
        if (tangent.SquareMagnitude() <= Precision::SquareConfusion())
            throw std::runtime_error("Edge tangent is undefined at the projected point.");
        tangent.Normalize();
        return tangent;
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

    OcctEdgeIntersection makeEdgeIntersectionPoint(
        const BRepAdaptor_Curve& firstCurve,
        const IntTools_CommonPrt& commonPart)
    {
        const double firstParameter = commonPart.VertexParameter1();
        const double secondParameter = commonPart.VertexParameter2();
        const gp_Pnt pointValue = firstCurve.Value(firstParameter);
        return {
            OcctIntersection_Point,
            {pointValue.X(), pointValue.Y(), pointValue.Z()},
            {pointValue.X(), pointValue.Y(), pointValue.Z()},
            firstParameter,
            firstParameter,
            secondParameter,
            secondParameter};
    }

    OcctEdgeIntersection makeEdgeIntersectionOverlap(
        const BRepAdaptor_Curve& firstCurve,
        const IntTools_Range& firstRange,
        const IntTools_Range& secondRange)
    {
        const double firstStart = firstRange.First();
        const double firstEnd = firstRange.Last();
        const gp_Pnt startPoint = firstCurve.Value(firstStart);
        const gp_Pnt endPoint = firstCurve.Value(firstEnd);
        return {
            OcctIntersection_Overlap,
            {startPoint.X(), startPoint.Y(), startPoint.Z()},
            {endPoint.X(), endPoint.Y(), endPoint.Z()},
            firstStart,
            firstEnd,
            secondRange.First(),
            secondRange.Last()};
    }

    gp_Vec faceNormal(const TopoDS_Face& face, double u, double v)
    {
        BRepAdaptor_Surface surface(face, Standard_True);
        gp_Pnt value;
        gp_Vec du;
        gp_Vec dv;
        surface.D1(u, v, value, du, dv);
        gp_Vec normal = du.Crossed(dv);
        if (normal.SquareMagnitude() <= Precision::SquareConfusion())
            throw std::runtime_error("Face normal is undefined at the projected UV position.");
        if (face.Orientation() == TopAbs_REVERSED) normal.Reverse();
        normal.Normalize();
        return normal;
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
            BRepBuilderAPI_MakeVertex sourceBuilder(point(sourcePoint));
            if (!sourceBuilder.IsDone())
                throw std::runtime_error("Unable to construct the projection source vertex.");
            BRepExtrema_DistShapeShape distance(sourceBuilder.Vertex(), edge);
            if (!distance.IsDone() || distance.NbSolution() < 1)
                throw std::runtime_error("Point-to-edge projection failed.");

            const gp_Pnt projected = distance.PointOnShape2(1);
            const double normalizedParameter = normalizedEdgeParameter(distance, edge, projected);
            const gp_Vec tangent = edgeTangent(edge, normalizedParameter);
            const double value = distance.Value();
            if (!std::isfinite(value))
                throw std::runtime_error("Edge projection returned a non-finite distance.");

            result->point = {projected.X(), projected.Y(), projected.Z()};
            result->tangent = {tangent.X(), tangent.Y(), tangent.Z()};
            result->normalizedParameter = normalizedParameter;
            result->distance = value;
        });
    }

    OcctStatus occt_engine_shape_intersect_edges_snapshot_get(
        OcctEngineHandle handle,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        double tolerance,
        OcctEdgeIntersection* results,
        int capacity,
        int* required)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (capacity < 0 || required == nullptr)
                throw std::invalid_argument("Edge intersection output buffer is invalid.");
            if (!std::isfinite(tolerance) || tolerance < 0.0)
                throw std::invalid_argument(
                    "Intersection tolerance must be finite and non-negative.");

            const TopoDS_Shape firstShape =
                shapeWithPresentationTransformation(requiredShape(engine, firstEdgeId));
            const TopoDS_Shape secondShape =
                shapeWithPresentationTransformation(requiredShape(engine, secondEdgeId));
            if (firstShape.ShapeType() != TopAbs_EDGE ||
                secondShape.ShapeType() != TopAbs_EDGE)
            {
                throw std::invalid_argument("Both inputs must be edges.");
            }

            const TopoDS_Edge firstEdge = TopoDS::Edge(firstShape);
            const TopoDS_Edge secondEdge = TopoDS::Edge(secondShape);
            const BRepAdaptor_Curve firstCurve(firstEdge);
            std::vector<OcctEdgeIntersection> intersections;

            IntTools_EdgeEdge intersector(firstEdge, secondEdge);
            intersector.SetFuzzyValue(tolerance);
            intersector.Perform();
            if (intersector.IsDone())
            {
                const auto& commonParts = intersector.CommonParts();
                for (int partIndex = 1; partIndex <= commonParts.Length(); ++partIndex)
                {
                    const IntTools_CommonPrt& commonPart =
                        commonParts.Value(partIndex);
                    if (commonPart.Type() == TopAbs_VERTEX)
                    {
                        intersections.push_back(
                            makeEdgeIntersectionPoint(firstCurve, commonPart));
                        continue;
                    }

                    if (commonPart.Type() != TopAbs_EDGE)
                        continue;

                    const IntTools_Range& firstRange = commonPart.Range1();
                    const auto& secondRanges = commonPart.Ranges2();
                    for (int rangeIndex = 1;
                         rangeIndex <= secondRanges.Length();
                         ++rangeIndex)
                    {
                        intersections.push_back(
                            makeEdgeIntersectionOverlap(
                                firstCurve,
                                firstRange,
                                secondRanges.Value(rangeIndex)));
                    }
                }
            }

            std::sort(
                intersections.begin(),
                intersections.end(),
                [](const OcctEdgeIntersection& left,
                   const OcctEdgeIntersection& right)
                {
                    if (left.firstParameterStart != right.firstParameterStart)
                        return left.firstParameterStart < right.firstParameterStart;
                    if (left.firstParameterEnd != right.firstParameterEnd)
                        return left.firstParameterEnd < right.firstParameterEnd;
                    return left.secondParameterStart < right.secondParameterStart;
                });

            if (intersections.size() >
                static_cast<std::size_t>(std::numeric_limits<int>::max()))
            {
                throw std::length_error(
                    "Edge-intersection result exceeds the ABI buffer size limit.");
            }

            const int count = static_cast<int>(intersections.size());
            *required = count;

            if (results == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument(
                        "Null intersection buffer requires zero capacity.");
                return;
            }

            if (capacity < count)
                throw std::invalid_argument(
                    "Intersection buffer capacity is smaller than the result count.");

            for (int index = 0; index < count; ++index)
                results[index] = intersections[static_cast<std::size_t>(index)];
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
            BRepBuilderAPI_MakeVertex sourceBuilder(point(sourcePoint));
            if (!sourceBuilder.IsDone())
                throw std::runtime_error("Unable to construct the projection source vertex.");
            BRepExtrema_DistShapeShape distance(sourceBuilder.Vertex(), face);
            if (!distance.IsDone() || distance.NbSolution() < 1)
                throw std::runtime_error("Point-to-face projection failed.");

            const gp_Pnt projected = distance.PointOnShape2(1);
            double u = 0.0;
            double v = 0.0;
            faceParameters(distance, face, projected, u, v);
            const gp_Vec normal = faceNormal(face, u, v);
            const double value = distance.Value();
            if (!std::isfinite(value))
                throw std::runtime_error("Face projection returned a non-finite distance.");

            result->point = {projected.X(), projected.Y(), projected.Z()};
            result->normal = {normal.X(), normal.Y(), normal.Z()};
            result->u = u;
            result->v = v;
            result->distance = value;
        });
    }
}

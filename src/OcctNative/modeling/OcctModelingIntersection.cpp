#include "modeling/OcctModelingIntersection.h"
#include "modeling/OcctModelingSessionInternal.hxx"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepClass_FaceClassifier.hxx>
#include <BRep_Tool.hxx>
#include <GeomAPI_IntCS.hxx>
#include <Geom_Curve.hxx>
#include <Geom_Surface.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <gp_Pnt2d.hxx>
#include <IntTools_CommonPrt.hxx>
#include <IntTools_EdgeEdge.hxx>
#include <IntTools_Range.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>

#include <algorithm>
#include <cmath>
#include <limits>
#include <stdexcept>
#include <vector>

using namespace OcctModelingInternal;

namespace
{
    TopoDS_Edge requireEdge(ModelSession* model, OcctObjectId edgeId)
    {
        const TopoDS_Shape& shape = model->requireShape(edgeId);
        if (shape.ShapeType() != TopAbs_EDGE)
            throw std::invalid_argument("Input must be an edge.");
        return TopoDS::Edge(shape);
    }

    OcctPoint3d toNativePoint(const gp_Pnt& point)
    {
        return {point.X(), point.Y(), point.Z()};
    }

    OcctModelEdgeIntersection makePoint(
        const BRepAdaptor_Curve& firstCurve,
        const IntTools_CommonPrt& commonPart)
    {
        const double firstParameter = commonPart.VertexParameter1();
        const double secondParameter = commonPart.VertexParameter2();
        const gp_Pnt point = firstCurve.Value(firstParameter);
        return {
            OcctModelIntersection_Point,
            toNativePoint(point),
            toNativePoint(point),
            firstParameter,
            firstParameter,
            secondParameter,
            secondParameter};
    }

    OcctModelEdgeIntersection makeOverlap(
        const BRepAdaptor_Curve& firstCurve,
        const IntTools_Range& firstRange,
        const IntTools_Range& secondRange)
    {
        const double firstStart = firstRange.First();
        const double firstEnd = firstRange.Last();
        const gp_Pnt startPoint = firstCurve.Value(firstStart);
        const gp_Pnt endPoint = firstCurve.Value(firstEnd);
        return {
            OcctModelIntersection_Overlap,
            toNativePoint(startPoint),
            toNativePoint(endPoint),
            firstStart,
            firstEnd,
            secondRange.First(),
            secondRange.Last()};
    }
    OcctStatus intersectEdgeFaceSnapshot(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctObjectId faceId,
        double tolerance,
        OcctModelEdgeFaceIntersection* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;
        if (!std::isfinite(tolerance) || tolerance < 0.0) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const TopoDS_Edge edge = requireEdge(model, edgeId);
            const TopoDS_Shape& faceShape = model->requireShape(faceId);
            if (faceShape.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Second input must be a face.");
            const TopoDS_Face face = TopoDS::Face(faceShape);

            Standard_Real first = 0.0;
            Standard_Real last = 0.0;
            Handle(Geom_Curve) curve = BRep_Tool::Curve(edge, first, last);
            Handle(Geom_Surface) surface = BRep_Tool::Surface(face);
            if (curve.IsNull()) throw std::runtime_error("Edge has no 3D curve.");
            if (surface.IsNull()) throw std::runtime_error("Face has no surface.");

            Handle(Geom_TrimmedCurve) trimmedCurve = new Geom_TrimmedCurve(curve, first, last);
            GeomAPI_IntCS intersector(trimmedCurve, surface);
            if (!intersector.IsDone())
                throw std::runtime_error("Edge/face intersection failed.");

            std::vector<OcctModelEdgeFaceIntersection> accepted;
            accepted.reserve(static_cast<std::size_t>(intersector.NbPoints()));
            for (int index = 1; index <= intersector.NbPoints(); ++index)
            {
                Standard_Real u = 0.0;
                Standard_Real v = 0.0;
                Standard_Real parameter = 0.0;
                intersector.Parameters(index, u, v, parameter);

                BRepClass_FaceClassifier classifier(face, gp_Pnt2d(u, v), tolerance);
                const TopAbs_State state = classifier.State();
                if (state != TopAbs_IN && state != TopAbs_ON)
                    continue;

                const gp_Pnt point = intersector.Point(index);
                accepted.push_back({
                    toNativePoint(point),
                    parameter,
                    u,
                    v});
            }

            if (accepted.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
                throw std::length_error("Edge/face intersection result exceeds the ABI buffer size limit.");

            const int count = static_cast<int>(accepted.size());
            *required = count;
            if (results == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Null edge/face intersection buffer requires zero capacity.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Edge/face intersection buffer capacity is smaller than the result count.");

            for (int index = 0; index < count; ++index)
                results[index] = accepted[static_cast<std::size_t>(index)];
        });
    }
}

extern "C"
{
    OcctStatus occt_model_intersect_edges(
        OcctModelingSessionHandle handle,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        double tolerance,
        int* resultCount)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (resultCount == nullptr) return OcctStatus_ErrorInvalidArgument;

        *resultCount = 0;
        return executeStatus(model, [&]
        {
            if (!std::isfinite(tolerance) || tolerance < 0.0)
                throw std::invalid_argument("Intersection tolerance must be finite and non-negative.");

            const TopoDS_Edge firstEdge = requireEdge(model, firstEdgeId);
            const TopoDS_Edge secondEdge = requireEdge(model, secondEdgeId);
            const BRepAdaptor_Curve firstCurve(firstEdge);

            model->edgeIntersections.clear();

            IntTools_EdgeEdge intersector(firstEdge, secondEdge);
            intersector.SetFuzzyValue(tolerance);
            intersector.Perform();
            if (!intersector.IsDone())
                return;

            const auto& commonParts = intersector.CommonParts();
            for (int partIndex = 1; partIndex <= commonParts.Length(); ++partIndex)
            {
                const IntTools_CommonPrt& commonPart = commonParts.Value(partIndex);
                if (commonPart.Type() == TopAbs_VERTEX)
                {
                    model->edgeIntersections.push_back(makePoint(firstCurve, commonPart));
                    continue;
                }

                if (commonPart.Type() != TopAbs_EDGE)
                    continue;

                const IntTools_Range& firstRange = commonPart.Range1();
                const auto& secondRanges = commonPart.Ranges2();
                for (int rangeIndex = 1; rangeIndex <= secondRanges.Length(); ++rangeIndex)
                    model->edgeIntersections.push_back(makeOverlap(firstCurve, firstRange, secondRanges.Value(rangeIndex)));
            }

            std::sort(
                model->edgeIntersections.begin(),
                model->edgeIntersections.end(),
                [](const OcctModelEdgeIntersection& left, const OcctModelEdgeIntersection& right)
                {
                    if (left.firstParameterStart != right.firstParameterStart)
                        return left.firstParameterStart < right.firstParameterStart;
                    if (left.firstParameterEnd != right.firstParameterEnd)
                        return left.firstParameterEnd < right.firstParameterEnd;
                    return left.secondParameterStart < right.secondParameterStart;
                });

            if (model->edgeIntersections.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
                throw std::length_error("Edge-intersection result exceeds the ABI buffer size limit.");
            *resultCount = static_cast<int>(model->edgeIntersections.size());
        });
    }

    OcctStatus occt_model_edge_intersections_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctModelEdgeIntersection* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            if (model->edgeIntersections.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
                throw std::length_error("Edge-intersection result exceeds the ABI buffer size limit.");

            const int count = static_cast<int>(model->edgeIntersections.size());
            *required = count;
            if (results == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Null intersection buffer requires zero capacity.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Intersection buffer capacity is smaller than the result count.");

            for (int index = 0; index < count; ++index)
                results[index] = model->edgeIntersections[static_cast<std::size_t>(index)];
        });
    }    OcctStatus occt_model_intersect_edge_face_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctObjectId faceId,
        double tolerance,
        OcctModelEdgeFaceIntersection* results,
        int capacity,
        int* required)
    {
        return intersectEdgeFaceSnapshot(
            handle,
            edgeId,
            faceId,
            tolerance,
            results,
            capacity,
            required);
    }


}

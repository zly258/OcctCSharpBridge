#include "OcctModelingIntersection.h"
#include "OcctModelingSessionInternal.hxx"
#include "OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <IntTools_CommonPrt.hxx>
#include <IntTools_EdgeEdge.hxx>
#include <IntTools_Range.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>

#include <algorithm>

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
}

extern "C"
{
    int occt_model_intersect_edges(
        OcctModelHandle handle,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        double tolerance)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;

        int count = 0;
        if (execute(model, [&]
        {
            if (tolerance < 0.0)
                throw std::invalid_argument("Intersection tolerance must not be negative.");

            const TopoDS_Edge firstEdge = requireEdge(model, firstEdgeId);
            const TopoDS_Edge secondEdge = requireEdge(model, secondEdgeId);
            const BRepAdaptor_Curve firstCurve(firstEdge);

            IntTools_EdgeEdge intersector(firstEdge, secondEdge);
            intersector.SetFuzzyValue(tolerance);
            intersector.Perform();

            model->edgeIntersections.clear();
            if (!intersector.IsDone())
            {
                count = 0;
                return;
            }

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

            count = static_cast<int>(model->edgeIntersections.size());
        }) == 0)
            return -1;

        return count;
    }

    int occt_model_edge_intersections_copy(
        OcctModelHandle handle,
        OcctModelEdgeIntersection* results,
        int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;

        int copied = 0;
        if (execute(model, [&]
        {
            if (capacity < 0)
                throw std::invalid_argument("Intersection buffer capacity must not be negative.");

            const int count = static_cast<int>(model->edgeIntersections.size());
            if (results == nullptr && capacity == 0)
            {
                copied = count;
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Intersection buffer capacity is smaller than the result count.");
            if (count > 0 && results == nullptr)
                throw std::invalid_argument("Intersection result buffer is null.");

            for (int index = 0; index < count; ++index)
                results[index] = model->edgeIntersections[static_cast<std::size_t>(index)];
            copied = count;
        }) == 0)
            return -1;

        return copied;
    }
}

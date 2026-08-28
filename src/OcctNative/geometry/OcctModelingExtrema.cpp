#include "geometry/OcctModelingExtrema.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRep_Tool.hxx>
#include <GeomAPI_ExtremaCurveCurve.hxx>
#include <Geom_Curve.hxx>

#include <limits>
#include <stdexcept>

using namespace OcctModelingInternal;

namespace
{
    TopoDS_Edge requireExtremaEdge(ModelSession* model, OcctObjectId edgeId)
    {
        const TopoDS_Shape& shape = model->requireShape(edgeId);
        if (shape.ShapeType() != TopAbs_EDGE)
            throw std::invalid_argument("Input must be an edge.");
        return TopoDS::Edge(shape);
    }

    OcctPoint3d extremaPoint(const gp_Pnt& point)
    {
        return {point.X(), point.Y(), point.Z()};
    }
}

extern "C"
{
    OcctStatus occt_model_edge_extrema_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        OcctModelCurveCurveExtremum* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const TopoDS_Edge firstEdge = requireExtremaEdge(model, firstEdgeId);
            const TopoDS_Edge secondEdge = requireExtremaEdge(model, secondEdgeId);

            Standard_Real firstMin = 0.0;
            Standard_Real firstMax = 0.0;
            Standard_Real secondMin = 0.0;
            Standard_Real secondMax = 0.0;
            Handle(Geom_Curve) firstCurve = BRep_Tool::Curve(firstEdge, firstMin, firstMax);
            Handle(Geom_Curve) secondCurve = BRep_Tool::Curve(secondEdge, secondMin, secondMax);
            if (firstCurve.IsNull() || secondCurve.IsNull())
                throw std::runtime_error("Edge extrema require two 3D curves.");

            GeomAPI_ExtremaCurveCurve extrema(
                firstCurve,
                secondCurve,
                firstMin,
                firstMax,
                secondMin,
                secondMax);

            const int count = extrema.NbExtrema();
            *required = count;
            if (results == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Null extrema buffer requires zero capacity.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Extrema buffer capacity is smaller than the result count.");

            for (int index = 1; index <= count; ++index)
            {
                gp_Pnt firstPoint;
                gp_Pnt secondPoint;
                Standard_Real firstParameter = 0.0;
                Standard_Real secondParameter = 0.0;
                extrema.Points(index, firstPoint, secondPoint);
                extrema.Parameters(index, firstParameter, secondParameter);

                OcctModelCurveCurveExtremum& result = results[index - 1];
                result.pointOnFirst = extremaPoint(firstPoint);
                result.pointOnSecond = extremaPoint(secondPoint);
                result.distance = firstPoint.Distance(secondPoint);
                result.firstParameter = firstParameter;
                result.secondParameter = secondParameter;
            }
        });
    }
}

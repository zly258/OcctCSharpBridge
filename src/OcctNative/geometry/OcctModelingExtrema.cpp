#include "geometry/OcctModelingExtrema.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepClass_FaceClassifier.hxx>
#include <BRepTools.hxx>
#include <BRep_Tool.hxx>
#include <GeomAPI_ExtremaCurveCurve.hxx>
#include <GeomAPI_ExtremaCurveSurface.hxx>
#include <GeomAPI_ExtremaSurfaceSurface.hxx>
#include <Geom_Curve.hxx>
#include <Geom_Surface.hxx>
#include <gp_Pnt2d.hxx>

#include <limits>
#include <stdexcept>
#include <string>
#include <vector>

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

    TopoDS_Face requireExtremaFace(ModelSession* model, OcctObjectId faceId)
    {
        const TopoDS_Shape& shape = model->requireShape(faceId);
        if (shape.ShapeType() != TopAbs_FACE)
            throw std::invalid_argument("Input must be a face.");
        return TopoDS::Face(shape);
    }

    OcctPoint3d extremaPoint(const gp_Pnt& point)
    {
        return {point.X(), point.Y(), point.Z()};
    }

    bool isOnFace(const TopoDS_Face& face, double u, double v)
    {
        BRepClass_FaceClassifier classifier(
            face,
            gp_Pnt2d(u, v),
            BRep_Tool::Tolerance(face));
        const TopAbs_State state = classifier.State();
        return state == TopAbs_IN || state == TopAbs_ON;
    }

    template <typename T>
    void copySnapshot(
        const std::vector<T>& values,
        T* results,
        int capacity,
        int* required,
        const char* name)
    {
        if (values.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
            throw std::length_error(std::string(name) + " result exceeds the ABI buffer size limit.");

        const int count = static_cast<int>(values.size());
        *required = count;
        if (results == nullptr)
        {
            if (capacity != 0)
                throw std::invalid_argument(std::string("Null ") + name + " buffer requires zero capacity.");
            return;
        }
        if (capacity < count)
            throw std::invalid_argument(std::string(name) + " buffer capacity is smaller than the result count.");

        for (int index = 0; index < count; ++index)
            results[index] = values[static_cast<std::size_t>(index)];
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

            std::vector<OcctModelCurveCurveExtremum> values;
            values.reserve(static_cast<std::size_t>(extrema.NbExtrema()));
            for (int index = 1; index <= extrema.NbExtrema(); ++index)
            {
                gp_Pnt firstPoint;
                gp_Pnt secondPoint;
                Standard_Real firstParameter = 0.0;
                Standard_Real secondParameter = 0.0;
                extrema.Points(index, firstPoint, secondPoint);
                extrema.Parameters(index, firstParameter, secondParameter);
                values.push_back({
                    extremaPoint(firstPoint),
                    extremaPoint(secondPoint),
                    extrema.Distance(index),
                    firstParameter,
                    secondParameter});
            }

            copySnapshot(values, results, capacity, required, "edge-extrema");
        });
    }

    OcctStatus occt_model_edge_face_extrema_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctObjectId faceId,
        OcctModelCurveSurfaceExtremum* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const TopoDS_Edge edge = requireExtremaEdge(model, edgeId);
            const TopoDS_Face face = requireExtremaFace(model, faceId);

            Standard_Real first = 0.0;
            Standard_Real last = 0.0;
            Handle(Geom_Curve) curve = BRep_Tool::Curve(edge, first, last);
            Handle(Geom_Surface) surface = BRep_Tool::Surface(face);
            if (curve.IsNull()) throw std::runtime_error("Edge has no 3D curve.");
            if (surface.IsNull()) throw std::runtime_error("Face has no surface.");

            Standard_Real uMin = 0.0;
            Standard_Real uMax = 0.0;
            Standard_Real vMin = 0.0;
            Standard_Real vMax = 0.0;
            BRepTools::UVBounds(face, uMin, uMax, vMin, vMax);

            GeomAPI_ExtremaCurveSurface extrema(
                curve,
                surface,
                first,
                last,
                uMin,
                uMax,
                vMin,
                vMax);

            std::vector<OcctModelCurveSurfaceExtremum> values;
            values.reserve(static_cast<std::size_t>(extrema.NbExtrema()));
            for (int index = 1; index <= extrema.NbExtrema(); ++index)
            {
                gp_Pnt curvePoint;
                gp_Pnt surfacePoint;
                Standard_Real curveParameter = 0.0;
                Standard_Real u = 0.0;
                Standard_Real v = 0.0;
                extrema.Points(index, curvePoint, surfacePoint);
                extrema.Parameters(index, curveParameter, u, v);
                if (!isOnFace(face, u, v))
                    continue;

                values.push_back({
                    extremaPoint(curvePoint),
                    extremaPoint(surfacePoint),
                    extrema.Distance(index),
                    curveParameter,
                    u,
                    v});
            }

            copySnapshot(values, results, capacity, required, "edge-face-extrema");
        });
    }

    OcctStatus occt_model_face_extrema_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId firstFaceId,
        OcctObjectId secondFaceId,
        OcctModelSurfaceSurfaceExtremum* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const TopoDS_Face firstFace = requireExtremaFace(model, firstFaceId);
            const TopoDS_Face secondFace = requireExtremaFace(model, secondFaceId);
            Handle(Geom_Surface) firstSurface = BRep_Tool::Surface(firstFace);
            Handle(Geom_Surface) secondSurface = BRep_Tool::Surface(secondFace);
            if (firstSurface.IsNull() || secondSurface.IsNull())
                throw std::runtime_error("Face extrema require two surfaces.");

            Standard_Real firstUMin = 0.0;
            Standard_Real firstUMax = 0.0;
            Standard_Real firstVMin = 0.0;
            Standard_Real firstVMax = 0.0;
            Standard_Real secondUMin = 0.0;
            Standard_Real secondUMax = 0.0;
            Standard_Real secondVMin = 0.0;
            Standard_Real secondVMax = 0.0;
            BRepTools::UVBounds(firstFace, firstUMin, firstUMax, firstVMin, firstVMax);
            BRepTools::UVBounds(secondFace, secondUMin, secondUMax, secondVMin, secondVMax);

            GeomAPI_ExtremaSurfaceSurface extrema(
                firstSurface,
                secondSurface,
                firstUMin,
                firstUMax,
                firstVMin,
                firstVMax,
                secondUMin,
                secondUMax,
                secondVMin,
                secondVMax);

            std::vector<OcctModelSurfaceSurfaceExtremum> values;
            values.reserve(static_cast<std::size_t>(extrema.NbExtrema()));
            for (int index = 1; index <= extrema.NbExtrema(); ++index)
            {
                gp_Pnt firstPoint;
                gp_Pnt secondPoint;
                Standard_Real firstU = 0.0;
                Standard_Real firstV = 0.0;
                Standard_Real secondU = 0.0;
                Standard_Real secondV = 0.0;
                extrema.Points(index, firstPoint, secondPoint);
                extrema.Parameters(index, firstU, firstV, secondU, secondV);
                if (!isOnFace(firstFace, firstU, firstV) ||
                    !isOnFace(secondFace, secondU, secondV))
                {
                    continue;
                }

                values.push_back({
                    extremaPoint(firstPoint),
                    extremaPoint(secondPoint),
                    extrema.Distance(index),
                    firstU,
                    firstV,
                    secondU,
                    secondV});
            }

            copySnapshot(values, results, capacity, required, "face-extrema");
        });
    }
}

#include "modeling/OcctModelingAnalysis.h"
#include "modeling/OcctModelingSessionInternal.hxx"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepClass3d_SolidClassifier.hxx>
#include <BRepTools.hxx>
#include <GeomAPI_ProjectPointOnCurve.hxx>
#include <GeomAPI_ProjectPointOnSurf.hxx>
#include <Geom_Curve.hxx>
#include <Geom_Surface.hxx>
#include <IntCurvesFace_ShapeIntersector.hxx>
#include <TopoDS_Solid.hxx>
#include <gp_Lin.hxx>

#include <cmath>
#include <limits>
#include <stdexcept>

using namespace OcctModelingInternal;

namespace
{
    void requireNonNegativeTolerance(double tolerance)
    {
        if (!std::isfinite(tolerance) || tolerance < 0.0)
            throw std::invalid_argument("Tolerance must be finite and non-negative.");
    }

    struct EdgeProjectionContext
    {
        Handle(Geom_Curve) curve;
        Standard_Real first = 0.0;
        Standard_Real last = 0.0;
    };

    struct FaceProjectionContext
    {
        Handle(Geom_Surface) surface;
        Standard_Real uMin = 0.0;
        Standard_Real uMax = 0.0;
        Standard_Real vMin = 0.0;
        Standard_Real vMax = 0.0;
    };

    EdgeProjectionContext edgeProjectionContext(ModelSession* model, OcctObjectId edgeId)
    {
        const TopoDS_Shape& shape = model->requireShape(edgeId);
        if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");

        EdgeProjectionContext context;
        context.curve = BRep_Tool::Curve(TopoDS::Edge(shape), context.first, context.last);
        if (context.curve.IsNull()) throw std::runtime_error("Edge has no 3D curve.");
        return context;
    }

    FaceProjectionContext faceProjectionContext(ModelSession* model, OcctObjectId faceId)
    {
        const TopoDS_Shape& shape = model->requireShape(faceId);
        if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");

        const TopoDS_Face face = TopoDS::Face(shape);
        FaceProjectionContext context;
        context.surface = BRep_Tool::Surface(face);
        if (context.surface.IsNull()) throw std::runtime_error("Face has no surface.");
        BRepTools::UVBounds(face, context.uMin, context.uMax, context.vMin, context.vMax);
        return context;
    }

    OcctModelProjectionResult projectPointOnEdge(const EdgeProjectionContext& context, OcctPoint3d pointValue)
    {
        GeomAPI_ProjectPointOnCurve projection(
            toPoint(pointValue),
            context.curve,
            context.first,
            context.last);
        if (projection.NbPoints() < 1) throw std::runtime_error("Point projection on edge failed.");

        const gp_Pnt projected = projection.NearestPoint();
        OcctModelProjectionResult result{};
        result.point = {projected.X(), projected.Y(), projected.Z()};
        result.distance = projection.LowerDistance();
        result.parameter = projection.LowerDistanceParameter();
        return result;
    }

    OcctModelProjectionResult projectPointOnFace(const FaceProjectionContext& context, OcctPoint3d pointValue)
    {
        GeomAPI_ProjectPointOnSurf projection;
        projection.Init(
            toPoint(pointValue),
            context.surface,
            context.uMin,
            context.uMax,
            context.vMin,
            context.vMax);
        if (projection.NbPoints() < 1) throw std::runtime_error("Point projection on face failed.");

        const gp_Pnt projected = projection.NearestPoint();
        OcctModelProjectionResult result{};
        result.point = {projected.X(), projected.Y(), projected.Z()};
        result.distance = projection.LowerDistance();
        projection.LowerDistanceParameters(result.u, result.v);
        return result;
    }
}

extern "C"
{
    OcctStatus occt_model_project_point_on_edge(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d pointValue,
        OcctModelProjectionResult* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            const EdgeProjectionContext context = edgeProjectionContext(model, edgeId);
            *result = projectPointOnEdge(context, pointValue);
        });
    }

    OcctStatus occt_model_project_point_on_face(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctPoint3d pointValue,
        OcctModelProjectionResult* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            const FaceProjectionContext context = faceProjectionContext(model, faceId);
            *result = projectPointOnFace(context, pointValue);
        });
    }


    OcctStatus occt_model_project_points_on_edge(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        const OcctPoint3d* points,
        int count,
        OcctModelProjectionResult* results)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (count < 0 || (count > 0 && (points == nullptr || results == nullptr)))
            return OcctStatus_ErrorInvalidArgument;
        return executeStatus(model, [&]
        {
            const EdgeProjectionContext context = edgeProjectionContext(model, edgeId);
            for (int index = 0; index < count; ++index)
                results[index] = projectPointOnEdge(context, points[index]);
        });
    }

    OcctStatus occt_model_project_points_on_face(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        const OcctPoint3d* points,
        int count,
        OcctModelProjectionResult* results)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (count < 0 || (count > 0 && (points == nullptr || results == nullptr)))
            return OcctStatus_ErrorInvalidArgument;
        return executeStatus(model, [&]
        {
            const FaceProjectionContext context = faceProjectionContext(model, faceId);
            for (int index = 0; index < count; ++index)
                results[index] = projectPointOnFace(context, points[index]);
        });
    }

    OcctStatus occt_model_ray_intersections(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d origin,
        OcctVector3d directionValue,
        double minimumParameter,
        double maximumParameter,
        double tolerance,
        int* resultCount)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (resultCount == nullptr) return OcctStatus_ErrorInvalidArgument;

        *resultCount = 0;
        return executeStatus(model, [&]
        {
            if (maximumParameter < minimumParameter)
                throw std::invalid_argument("Ray parameter range is invalid.");
            requireNonNegativeTolerance(tolerance);

            model->rayHits.clear();

            IntCurvesFace_ShapeIntersector intersector;
            intersector.Load(model->requireShape(shapeId), tolerance);
            intersector.Perform(
                gp_Lin(toPoint(origin), toDirection(directionValue)),
                minimumParameter,
                maximumParameter);
            if (!intersector.IsDone())
                throw std::runtime_error("Ray intersection failed.");

            intersector.SortResult();
            if (intersector.NbPnt() > std::numeric_limits<int>::max())
                throw std::length_error("Ray-hit result exceeds the ABI buffer size limit.");

            model->rayHits.reserve(static_cast<std::size_t>(intersector.NbPnt()));
            for (int index = 1; index <= intersector.NbPnt(); ++index)
            {
                const gp_Pnt point = intersector.Pnt(index);
                const OcctObjectId faceId = model->addShape(intersector.Face(index));
                model->rayHits.push_back({
                    {point.X(), point.Y(), point.Z()},
                    faceId,
                    intersector.WParameter(index),
                    intersector.UParameter(index),
                    intersector.VParameter(index),
                    toModelState(intersector.State(index))});
            }

            *resultCount = static_cast<int>(model->rayHits.size());
        });
    }

    OcctStatus occt_model_ray_hits_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctModelRayHit* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            if (model->rayHits.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
                throw std::length_error("Ray-hit result exceeds the ABI buffer size limit.");

            const int count = static_cast<int>(model->rayHits.size());
            *required = count;
            if (results == nullptr)
            {
                if (capacity != 0)
                    throw std::invalid_argument("Null ray-hit buffer requires zero capacity.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Ray-hit buffer capacity is smaller than the result count.");

            for (int index = 0; index < count; ++index)
                results[index] = model->rayHits[static_cast<std::size_t>(index)];
        });
    }

    OcctStatus occt_model_classify_point(
        OcctModelingSessionHandle handle,
        OcctObjectId solidId,
        OcctPoint3d pointValue,
        double tolerance,
        OcctModelState* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = OcctModelState_Unknown;
        return executeStatus(model, [&]
        {
            requireNonNegativeTolerance(tolerance);
            const TopoDS_Shape& shape = model->requireShape(solidId);
            if (shape.ShapeType() != TopAbs_SOLID)
                throw std::invalid_argument("Input must be a solid.");

            BRepClass3d_SolidClassifier classifier(
                TopoDS::Solid(shape),
                toPoint(pointValue),
                tolerance);
            *result = static_cast<OcctModelState>(toModelState(classifier.State()));
        });
    }
}

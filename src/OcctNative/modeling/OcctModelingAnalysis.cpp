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

using namespace OcctModelingInternal;

extern "C"
{
    int occt_model_project_point_on_edge(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d pointValue, OcctModelProjectionResult* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            Standard_Real first = 0.0;
            Standard_Real last = 0.0;
            Handle(Geom_Curve) curve = BRep_Tool::Curve(TopoDS::Edge(shape), first, last);
            if (curve.IsNull()) throw std::runtime_error("Edge has no 3D curve.");
            GeomAPI_ProjectPointOnCurve projection(toPoint(pointValue), curve, first, last);
            if (projection.NbPoints() < 1) throw std::runtime_error("Point projection on edge failed.");
            const gp_Pnt projected = projection.NearestPoint();
            result->point = {projected.X(), projected.Y(), projected.Z()};
            result->distance = projection.LowerDistance();
            result->parameter = projection.LowerDistanceParameter();
            result->u = 0.0;
            result->v = 0.0;
        });
    }

    int occt_model_project_point_on_face(OcctModelHandle handle, OcctObjectId faceId, OcctPoint3d pointValue, OcctModelProjectionResult* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            const TopoDS_Face face = TopoDS::Face(shape);
            Handle(Geom_Surface) surface = BRep_Tool::Surface(face);
            if (surface.IsNull()) throw std::runtime_error("Face has no surface.");
            Standard_Real uMin = 0.0;
            Standard_Real uMax = 0.0;
            Standard_Real vMin = 0.0;
            Standard_Real vMax = 0.0;
            BRepTools::UVBounds(face, uMin, uMax, vMin, vMax);
            GeomAPI_ProjectPointOnSurf projection;
            projection.Init(toPoint(pointValue), surface, uMin, uMax, vMin, vMax);
            if (projection.NbPoints() < 1) throw std::runtime_error("Point projection on face failed.");
            const gp_Pnt projected = projection.NearestPoint();
            Standard_Real u = 0.0;
            Standard_Real v = 0.0;
            projection.LowerDistanceParameters(u, v);
            result->point = {projected.X(), projected.Y(), projected.Z()};
            result->distance = projection.LowerDistance();
            result->parameter = 0.0;
            result->u = u;
            result->v = v;
        });
    }

    int occt_model_ray_intersections(OcctModelHandle handle, OcctObjectId shapeId, OcctPoint3d origin, OcctVector3d directionValue, double minimumParameter, double maximumParameter, double tolerance)
    {
        ModelSession* model = modelOf(handle);
        int count = 0;
        const int succeeded = execute(model, [&]
        {
            if (maximumParameter < minimumParameter) throw std::invalid_argument("Ray parameter range is invalid.");
            requirePositive(tolerance, "Tolerance");
            IntCurvesFace_ShapeIntersector intersector;
            intersector.Load(model->requireShape(shapeId), tolerance);
            intersector.Perform(gp_Lin(toPoint(origin), toDirection(directionValue)), minimumParameter, maximumParameter);
            if (!intersector.IsDone()) throw std::runtime_error("Ray intersection failed.");
            intersector.SortResult();
            model->rayHits.clear();
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
            count = static_cast<int>(model->rayHits.size());
        });
        return succeeded == 0 ? -1 : count;
    }

    int occt_model_ray_hits_copy(OcctModelHandle handle, OcctModelRayHit* results, int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;
        int copied = 0;
        if (execute(model, [&]
        {
            if (capacity < 0) throw std::invalid_argument("Ray-hit buffer capacity must not be negative.");
            const int count = static_cast<int>(model->rayHits.size());
            if (capacity < count) throw std::invalid_argument("Ray-hit buffer capacity is smaller than the result count.");
            if (count > 0 && results == nullptr) throw std::invalid_argument("Ray-hit result buffer is null.");
            for (int index = 0; index < count; ++index)
                results[index] = model->rayHits[static_cast<std::size_t>(index)];
            copied = count;
        }) == 0)
            return -1;
        return copied;
    }

    int occt_model_classify_point(OcctModelHandle handle, OcctObjectId solidId, OcctPoint3d pointValue, double tolerance)
    {
        ModelSession* model = modelOf(handle);
        return executeValue(model, static_cast<int>(OcctModelState_Unknown), [&]
        {
            const TopoDS_Shape& shape = model->requireShape(solidId);
            if (shape.ShapeType() != TopAbs_SOLID) throw std::invalid_argument("Input must be a solid.");
            BRepClass3d_SolidClassifier classifier(TopoDS::Solid(shape), toPoint(pointValue), tolerance);
            return toModelState(classifier.State());
        });
    }
}

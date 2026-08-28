#include "geometry/OcctModelingBSpline.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <Geom_BSplineCurve.hxx>
#include <Geom_BSplineSurface.hxx>
#include <TColgp_Array1OfPnt.hxx>
#include <TColgp_Array2OfPnt.hxx>
#include <TColStd_Array1OfInteger.hxx>
#include <TColStd_Array1OfReal.hxx>
#include <TColStd_Array2OfReal.hxx>


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

    TopoDS_Face requireFace(ModelSession* model, OcctObjectId faceId)
    {
        const TopoDS_Shape& shape = model->requireShape(faceId);
        if (shape.ShapeType() != TopAbs_FACE)
            throw std::invalid_argument("Input must be a face.");
        return TopoDS::Face(shape);
    }

    Handle(Geom_BSplineCurve) requireBSplineCurve(ModelSession* model, OcctObjectId edgeId)
    {
        const BRepAdaptor_Curve adaptor(requireEdge(model, edgeId));
        if (adaptor.GetType() != GeomAbs_BSplineCurve)
            throw std::invalid_argument("Edge curve is not a B-Spline.");

        Handle(Geom_BSplineCurve) curve = adaptor.BSpline();
        if (curve.IsNull())
            throw std::runtime_error("B-Spline curve data is unavailable.");
        return curve;
    }

    Handle(Geom_BSplineSurface) requireBSplineSurface(ModelSession* model, OcctObjectId faceId)
    {
        const BRepAdaptor_Surface adaptor(requireFace(model, faceId), Standard_False);
        if (adaptor.GetType() != GeomAbs_BSplineSurface)
            throw std::invalid_argument("Face surface is not a B-Spline.");

        Handle(Geom_BSplineSurface) surface = adaptor.BSpline();
        if (surface.IsNull())
            throw std::runtime_error("B-Spline surface data is unavailable.");
        return surface;
    }
}

extern "C"
{
    OcctStatus occt_model_edge_bspline_info(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelBSplineCurveInfo* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineCurve) curve = requireBSplineCurve(model, edgeId);
            result->degree = curve->Degree();
            result->poleCount = curve->NbPoles();
            result->knotCount = curve->NbKnots();
            result->rational = curve->IsRational() ? 1 : 0;
            result->periodic = curve->IsPeriodic() ? 1 : 0;
        });
    }

    OcctStatus occt_model_face_bspline_info(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelBSplineSurfaceInfo* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineSurface) surface = requireBSplineSurface(model, faceId);
            result->uDegree = surface->UDegree();
            result->vDegree = surface->VDegree();
            result->uPoleCount = surface->NbUPoles();
            result->vPoleCount = surface->NbVPoles();
            result->uKnotCount = surface->NbUKnots();
            result->vKnotCount = surface->NbVKnots();
            result->uRational = surface->IsURational() ? 1 : 0;
            result->vRational = surface->IsVRational() ? 1 : 0;
            result->uPeriodic = surface->IsUPeriodic() ? 1 : 0;
            result->vPeriodic = surface->IsVPeriodic() ? 1 : 0;
        });
    }

    OcctStatus occt_model_edge_bspline_poles_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d* poles,
        double* weights,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineCurve) curve = requireBSplineCurve(model, edgeId);
            const int count = curve->NbPoles();
            *required = count;
            if (poles == nullptr || weights == nullptr)
            {
                if (capacity != 0 || poles != nullptr || weights != nullptr)
                    throw std::invalid_argument("B-Spline pole buffers must both be null for a count query.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("B-Spline pole buffer capacity is smaller than the result count.");

            for (int index = 0; index < count; ++index)
            {
                const int oneBased = index + 1;
                const gp_Pnt point = curve->Pole(oneBased);
                poles[index] = {point.X(), point.Y(), point.Z()};
                weights[index] = curve->Weight(oneBased);
            }
        });
    }

    OcctStatus occt_model_edge_bspline_knots_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        double* knots,
        int* multiplicities,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineCurve) curve = requireBSplineCurve(model, edgeId);
            const int count = curve->NbKnots();
            *required = count;
            if (knots == nullptr || multiplicities == nullptr)
            {
                if (capacity != 0 || knots != nullptr || multiplicities != nullptr)
                    throw std::invalid_argument("B-Spline knot buffers must both be null for a count query.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("B-Spline knot buffer capacity is smaller than the result count.");

            for (int index = 0; index < count; ++index)
            {
                const int oneBased = index + 1;
                knots[index] = curve->Knot(oneBased);
                multiplicities[index] = curve->Multiplicity(oneBased);
            }
        });
    }

    OcctStatus occt_model_face_bspline_poles_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctPoint3d* poles,
        double* weights,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineSurface) surface = requireBSplineSurface(model, faceId);
            const int uCount = surface->NbUPoles();
            const int vCount = surface->NbVPoles();
            const int count = uCount * vCount;
            *required = count;
            if (poles == nullptr || weights == nullptr)
            {
                if (capacity != 0 || poles != nullptr || weights != nullptr)
                    throw std::invalid_argument("B-Spline surface pole buffers must both be null for a count query.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("B-Spline surface pole buffer capacity is smaller than the result count.");

            for (int u = 0; u < uCount; ++u)
            {
                for (int v = 0; v < vCount; ++v)
                {
                    const int index = u * vCount + v;
                    const gp_Pnt point = surface->Pole(u + 1, v + 1);
                    poles[index] = {point.X(), point.Y(), point.Z()};
                    weights[index] = surface->Weight(u + 1, v + 1);
                }
            }
        });
    }

    OcctStatus occt_model_face_bspline_u_knots_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        double* knots,
        int* multiplicities,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineSurface) surface = requireBSplineSurface(model, faceId);
            const int count = surface->NbUKnots();
            *required = count;
            if (knots == nullptr || multiplicities == nullptr)
            {
                if (capacity != 0 || knots != nullptr || multiplicities != nullptr)
                    throw std::invalid_argument("B-Spline U-knot buffers must both be null for a count query.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("B-Spline U-knot buffer capacity is smaller than the result count.");

            for (int index = 0; index < count; ++index)
            {
                knots[index] = surface->UKnot(index + 1);
                multiplicities[index] = surface->UMultiplicity(index + 1);
            }
        });
    }

    OcctStatus occt_model_face_bspline_v_knots_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        double* knots,
        int* multiplicities,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineSurface) surface = requireBSplineSurface(model, faceId);
            const int count = surface->NbVKnots();
            *required = count;
            if (knots == nullptr || multiplicities == nullptr)
            {
                if (capacity != 0 || knots != nullptr || multiplicities != nullptr)
                    throw std::invalid_argument("B-Spline V-knot buffers must both be null for a count query.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("B-Spline V-knot buffer capacity is smaller than the result count.");

            for (int index = 0; index < count; ++index)
            {
                knots[index] = surface->VKnot(index + 1);
                multiplicities[index] = surface->VMultiplicity(index + 1);
            }
        });
    }

    OcctStatus occt_model_curve_bspline_explicit_create(
        OcctModelingSessionHandle handle,
        const OcctBSplineCurveDefinition* def,
        const OcctPoint3d* poles,
        const double* weights,
        const double* knots,
        const int* multiplicities,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]() -> TopoDS_Shape
        {
            constexpr uint32_t kApiVersion = 1;
            if (def == nullptr) throw std::invalid_argument("BSpline definition is null.");
            if (def->structSize < sizeof(OcctBSplineCurveDefinition))
                throw std::invalid_argument("Unsupported BSpline definition size.");
            if (def->apiVersion != kApiVersion)
                throw std::invalid_argument("Unsupported BSpline definition API version.");
            if (def->degree < 1) throw std::invalid_argument("BSpline degree must be >= 1.");
            if (def->poleCount < 2) throw std::invalid_argument("BSpline must have at least 2 poles.");
            if (def->knotCount < 2) throw std::invalid_argument("BSpline must have at least 2 knots.");
            if (poles == nullptr) throw std::invalid_argument("BSpline pole array is null.");
            if (knots == nullptr) throw std::invalid_argument("BSpline knot array is null.");
            if (multiplicities == nullptr) throw std::invalid_argument("BSpline multiplicity array is null.");

            // Build OCCT arrays (1-based)
            TColgp_Array1OfPnt occtPoles(1, def->poleCount);
            for (int i = 0; i < def->poleCount; ++i)
                occtPoles.SetValue(i + 1, gp_Pnt(poles[i].x, poles[i].y, poles[i].z));

            TColStd_Array1OfReal occtKnots(1, def->knotCount);
            TColStd_Array1OfInteger occtMults(1, def->knotCount);
            for (int i = 0; i < def->knotCount; ++i) {
                occtKnots.SetValue(i + 1, knots[i]);
                occtMults.SetValue(i + 1, multiplicities[i]);
            }

            Handle(Geom_BSplineCurve) curve;
            if (def->rational != 0 && weights != nullptr) {
                TColStd_Array1OfReal occtWeights(1, def->poleCount);
                for (int i = 0; i < def->poleCount; ++i)
                    occtWeights.SetValue(i + 1, weights[i]);
                curve = new Geom_BSplineCurve(
                    occtPoles, occtWeights, occtKnots, occtMults,
                    def->degree, def->periodic != 0);
            } else {
                curve = new Geom_BSplineCurve(
                    occtPoles, occtKnots, occtMults,
                    def->degree, def->periodic != 0);
            }

            BRepBuilderAPI_MakeEdge edgeMaker(curve);
            if (!edgeMaker.IsDone()) throw std::runtime_error("Failed to create BSpline edge.");
            return edgeMaker.Edge();
        });
    }

    OcctStatus occt_model_face_bspline_explicit_create(
        OcctModelingSessionHandle handle,
        const OcctBSplineSurfaceDefinition* def,
        const OcctPoint3d* poles,
        const double* weights,
        const double* uKnots, const int* uMults,
        const double* vKnots, const int* vMults,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]() -> TopoDS_Shape
        {
            constexpr uint32_t kApiVersion = 1;
            if (def == nullptr) throw std::invalid_argument("BSpline definition is null.");
            if (def->structSize < sizeof(OcctBSplineSurfaceDefinition))
                throw std::invalid_argument("Unsupported BSpline surface definition size.");
            if (def->apiVersion != kApiVersion)
                throw std::invalid_argument("Unsupported BSpline surface definition API version.");
            if (poles == nullptr) throw std::invalid_argument("Poles array is null.");
            
            TColgp_Array2OfPnt poles2d(1, def->uPoleCount, 1, def->vPoleCount);
            for (int u = 0; u < def->uPoleCount; ++u) {
                for (int v = 0; v < def->vPoleCount; ++v) {
                    int idx = u * def->vPoleCount + v;
                    poles2d.SetValue(u + 1, v + 1, gp_Pnt(poles[idx].x, poles[idx].y, poles[idx].z));
                }
            }

            TColStd_Array1OfReal occtUKnots(1, def->uKnotCount);
            TColStd_Array1OfInteger occtUMults(1, def->uKnotCount);
            for (int i = 0; i < def->uKnotCount; ++i) {
                occtUKnots.SetValue(i + 1, uKnots[i]);
                occtUMults.SetValue(i + 1, uMults[i]);
            }

            TColStd_Array1OfReal occtVKnots(1, def->vKnotCount);
            TColStd_Array1OfInteger occtVMults(1, def->vKnotCount);
            for (int i = 0; i < def->vKnotCount; ++i) {
                occtVKnots.SetValue(i + 1, vKnots[i]);
                occtVMults.SetValue(i + 1, vMults[i]);
            }

            Handle(Geom_BSplineSurface) surface;
            if ((def->uRational != 0 || def->vRational != 0) && weights != nullptr) {
                TColStd_Array2OfReal weights2d(1, def->uPoleCount, 1, def->vPoleCount);
                for (int u = 0; u < def->uPoleCount; ++u) {
                    for (int v = 0; v < def->vPoleCount; ++v) {
                        int idx = u * def->vPoleCount + v;
                        weights2d.SetValue(u + 1, v + 1, weights[idx]);
                    }
                }
                surface = new Geom_BSplineSurface(
                    poles2d, weights2d, occtUKnots, occtVKnots, occtUMults, occtVMults,
                    def->uDegree, def->vDegree, def->uPeriodic != 0, def->vPeriodic != 0);
            } else {
                surface = new Geom_BSplineSurface(
                    poles2d, occtUKnots, occtVKnots, occtUMults, occtVMults,
                    def->uDegree, def->vDegree, def->uPeriodic != 0, def->vPeriodic != 0);
            }

            BRepBuilderAPI_MakeFace faceMaker(surface, Precision::Confusion());
            if (!faceMaker.IsDone()) throw std::runtime_error("Failed to create BSpline face.");
            return faceMaker.Face();
        });
    }
}

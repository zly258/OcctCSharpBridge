#include "geometry/OcctModelingBSpline.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <Geom_BSplineCurve.hxx>
#include <Geom_BSplineSurface.hxx>

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

    OcctStatus occt_model_edge_bspline_pole_at(OcctModelingSessionHandle handle, OcctObjectId edgeId, int index, OcctPoint3d* pole, double* weight)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (pole == nullptr || weight == nullptr) return OcctStatus_ErrorInvalidArgument;
        *pole = {};
        *weight = 0.0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineCurve) curve = requireBSplineCurve(model, edgeId);
            if (index < 0 || index >= curve->NbPoles())
                throw std::out_of_range("B-Spline pole index is out of range.");
            const int occtIndex = index + 1;
            const gp_Pnt point = curve->Pole(occtIndex);
            *pole = {point.X(), point.Y(), point.Z()};
            *weight = curve->Weight(occtIndex);
        });
    }

    OcctStatus occt_model_edge_bspline_knot_at(OcctModelingSessionHandle handle, OcctObjectId edgeId, int index, double* knot, int* multiplicity)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (knot == nullptr || multiplicity == nullptr) return OcctStatus_ErrorInvalidArgument;
        *knot = 0.0;
        *multiplicity = 0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineCurve) curve = requireBSplineCurve(model, edgeId);
            if (index < 0 || index >= curve->NbKnots())
                throw std::out_of_range("B-Spline knot index is out of range.");
            const int occtIndex = index + 1;
            *knot = curve->Knot(occtIndex);
            *multiplicity = curve->Multiplicity(occtIndex);
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

    OcctStatus occt_model_face_bspline_pole_at(OcctModelingSessionHandle handle, OcctObjectId faceId, int uIndex, int vIndex, OcctPoint3d* pole, double* weight)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (pole == nullptr || weight == nullptr) return OcctStatus_ErrorInvalidArgument;
        *pole = {};
        *weight = 0.0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineSurface) surface = requireBSplineSurface(model, faceId);
            if (uIndex < 0 || uIndex >= surface->NbUPoles())
                throw std::out_of_range("B-Spline surface U pole index is out of range.");
            if (vIndex < 0 || vIndex >= surface->NbVPoles())
                throw std::out_of_range("B-Spline surface V pole index is out of range.");
            const int occtUIndex = uIndex + 1;
            const int occtVIndex = vIndex + 1;
            const gp_Pnt point = surface->Pole(occtUIndex, occtVIndex);
            *pole = {point.X(), point.Y(), point.Z()};
            *weight = surface->Weight(occtUIndex, occtVIndex);
        });
    }

    OcctStatus occt_model_face_bspline_u_knot_at(OcctModelingSessionHandle handle, OcctObjectId faceId, int index, double* knot, int* multiplicity)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (knot == nullptr || multiplicity == nullptr) return OcctStatus_ErrorInvalidArgument;
        *knot = 0.0;
        *multiplicity = 0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineSurface) surface = requireBSplineSurface(model, faceId);
            if (index < 0 || index >= surface->NbUKnots())
                throw std::out_of_range("B-Spline surface U knot index is out of range.");
            const int occtIndex = index + 1;
            *knot = surface->UKnot(occtIndex);
            *multiplicity = surface->UMultiplicity(occtIndex);
        });
    }

    OcctStatus occt_model_face_bspline_v_knot_at(OcctModelingSessionHandle handle, OcctObjectId faceId, int index, double* knot, int* multiplicity)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (knot == nullptr || multiplicity == nullptr) return OcctStatus_ErrorInvalidArgument;
        *knot = 0.0;
        *multiplicity = 0;
        return executeStatus(model, [&]
        {
            const Handle(Geom_BSplineSurface) surface = requireBSplineSurface(model, faceId);
            if (index < 0 || index >= surface->NbVKnots())
                throw std::out_of_range("B-Spline surface V knot index is out of range.");
            const int occtIndex = index + 1;
            *knot = surface->VKnot(occtIndex);
            *multiplicity = surface->VMultiplicity(occtIndex);
        });
    }
}

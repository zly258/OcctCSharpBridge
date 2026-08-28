#include "geometry/OcctModelingFreeformGeometry.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRep_Tool.hxx>
#include <GeomAdaptor_Curve.hxx>
#include <GeomAdaptor_Surface.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_BezierSurface.hxx>
#include <Geom_OffsetSurface.hxx>
#include <Geom_RectangularTrimmedSurface.hxx>
#include <Geom_SurfaceOfLinearExtrusion.hxx>
#include <Geom_SurfaceOfRevolution.hxx>
#include <gp_Hypr.hxx>
#include <gp_Parab.hxx>

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

    OcctPoint3d pointValue(const gp_Pnt& value)
    {
        return {value.X(), value.Y(), value.Z()};
    }

    OcctVector3d directionValue(const gp_Dir& value)
    {
        return {value.X(), value.Y(), value.Z()};
    }

    Handle(Geom_Surface) untrimmedSurface(const TopoDS_Face& face)
    {
        Handle(Geom_Surface) surface = BRep_Tool::Surface(face);
        if (surface.IsNull())
            throw std::runtime_error("Face has no surface.");

        while (true)
        {
            Handle(Geom_RectangularTrimmedSurface) trimmed =
                Handle(Geom_RectangularTrimmedSurface)::DownCast(surface);
            if (trimmed.IsNull())
                return surface;
            surface = trimmed->BasisSurface();
        }
    }

    Handle(Geom_BezierCurve) requireBezierCurve(ModelSession* model, OcctObjectId edgeId)
    {
        const BRepAdaptor_Curve adaptor(requireEdge(model, edgeId));
        if (adaptor.GetType() != GeomAbs_BezierCurve)
            throw std::invalid_argument("Edge curve is not a Bezier curve.");
        Handle(Geom_BezierCurve) curve = adaptor.Bezier();
        if (curve.IsNull())
            throw std::runtime_error("Bezier curve data is unavailable.");
        return curve;
    }

    Handle(Geom_BezierSurface) requireBezierSurface(ModelSession* model, OcctObjectId faceId)
    {
        const BRepAdaptor_Surface adaptor(requireFace(model, faceId), Standard_False);
        if (adaptor.GetType() != GeomAbs_BezierSurface)
            throw std::invalid_argument("Face surface is not a Bezier surface.");
        Handle(Geom_BezierSurface) surface = adaptor.Bezier();
        if (surface.IsNull())
            throw std::runtime_error("Bezier surface data is unavailable.");
        return surface;
    }

    OcctCurveType curveTypeOf(const Handle(Geom_Curve)& curve)
    {
        if (curve.IsNull())
            return static_cast<OcctCurveType>(OcctCurve_Other);
        return static_cast<OcctCurveType>(toOcctCurveType(GeomAdaptor_Curve(curve).GetType()));
    }

    OcctSurfaceType surfaceTypeOf(const Handle(Geom_Surface)& surface)
    {
        if (surface.IsNull())
            return static_cast<OcctSurfaceType>(OcctSurface_Other);
        return static_cast<OcctSurfaceType>(toOcctSurfaceType(GeomAdaptor_Surface(surface).GetType()));
    }
}

extern "C"
{
    OcctStatus occt_model_edge_parabola_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctModelParabolaGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};

        return executeStatus(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            if (curve.GetType() != GeomAbs_Parabola)
                throw std::invalid_argument("Edge curve is not a parabola.");

            const gp_Parab value = curve.Parabola();
            result->center = pointValue(value.Location());
            result->normal = directionValue(value.Position().Direction());
            result->xDirection = directionValue(value.Position().XDirection());
            result->focalLength = value.Focal();
            result->firstParameter = curve.FirstParameter();
            result->lastParameter = curve.LastParameter();
        });
    }

    OcctStatus occt_model_edge_hyperbola_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctModelHyperbolaGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};

        return executeStatus(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            if (curve.GetType() != GeomAbs_Hyperbola)
                throw std::invalid_argument("Edge curve is not a hyperbola.");

            const gp_Hypr value = curve.Hyperbola();
            result->center = pointValue(value.Location());
            result->normal = directionValue(value.Position().Direction());
            result->xDirection = directionValue(value.Position().XDirection());
            result->majorRadius = value.MajorRadius();
            result->minorRadius = value.MinorRadius();
            result->firstParameter = curve.FirstParameter();
            result->lastParameter = curve.LastParameter();
        });
    }

    OcctStatus occt_model_edge_bezier_info(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctModelBezierCurveInfo* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};

        return executeStatus(model, [&]
        {
            const Handle(Geom_BezierCurve) curve = requireBezierCurve(model, edgeId);
            result->degree = curve->Degree();
            result->poleCount = curve->NbPoles();
            result->rational = curve->IsRational() ? 1 : 0;
            result->closed = curve->IsClosed() ? 1 : 0;
        });
    }

    OcctStatus occt_model_edge_bezier_poles_snapshot_get(
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
            const Handle(Geom_BezierCurve) curve = requireBezierCurve(model, edgeId);
            const int count = curve->NbPoles();
            *required = count;
            if (poles == nullptr || weights == nullptr)
            {
                if (capacity != 0 || poles != nullptr || weights != nullptr)
                    throw std::invalid_argument("Bezier pole buffers must both be null for a count query.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Bezier pole buffer capacity is smaller than the result count.");

            for (int index = 0; index < count; ++index)
            {
                const int oneBased = index + 1;
                poles[index] = pointValue(curve->Pole(oneBased));
                weights[index] = curve->Weight(oneBased);
            }
        });
    }

    OcctStatus occt_model_face_bezier_info(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelBezierSurfaceInfo* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};

        return executeStatus(model, [&]
        {
            const Handle(Geom_BezierSurface) surface = requireBezierSurface(model, faceId);
            result->uDegree = surface->UDegree();
            result->vDegree = surface->VDegree();
            result->uPoleCount = surface->NbUPoles();
            result->vPoleCount = surface->NbVPoles();
            result->uRational = surface->IsURational() ? 1 : 0;
            result->vRational = surface->IsVRational() ? 1 : 0;
        });
    }

    OcctStatus occt_model_face_bezier_poles_snapshot_get(
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
            const Handle(Geom_BezierSurface) surface = requireBezierSurface(model, faceId);
            const int uCount = surface->NbUPoles();
            const int vCount = surface->NbVPoles();
            const int count = uCount * vCount;
            *required = count;
            if (poles == nullptr || weights == nullptr)
            {
                if (capacity != 0 || poles != nullptr || weights != nullptr)
                    throw std::invalid_argument("Bezier surface pole buffers must both be null for a count query.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Bezier surface pole buffer capacity is smaller than the result count.");

            for (int u = 0; u < uCount; ++u)
            {
                for (int v = 0; v < vCount; ++v)
                {
                    const int index = u * vCount + v;
                    poles[index] = pointValue(surface->Pole(u + 1, v + 1));
                    weights[index] = surface->Weight(u + 1, v + 1);
                }
            }
        });
    }

    OcctStatus occt_model_face_extrusion_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelExtrusionSurfaceGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};

        return executeStatus(model, [&]
        {
            const TopoDS_Face face = requireFace(model, faceId);
            const BRepAdaptor_Surface adaptor(face, Standard_False);
            if (adaptor.GetType() != GeomAbs_SurfaceOfExtrusion)
                throw std::invalid_argument("Face surface is not a surface of extrusion.");

            const Handle(Geom_SurfaceOfLinearExtrusion) surface =
                Handle(Geom_SurfaceOfLinearExtrusion)::DownCast(untrimmedSurface(face));
            if (surface.IsNull())
                throw std::runtime_error("Surface-of-extrusion data is unavailable.");

            result->direction = directionValue(surface->Direction());
            result->basisCurveType = curveTypeOf(surface->BasisCurve());
        });
    }

    OcctStatus occt_model_face_revolution_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelRevolutionSurfaceGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};

        return executeStatus(model, [&]
        {
            const TopoDS_Face face = requireFace(model, faceId);
            const BRepAdaptor_Surface adaptor(face, Standard_False);
            if (adaptor.GetType() != GeomAbs_SurfaceOfRevolution)
                throw std::invalid_argument("Face surface is not a surface of revolution.");

            const Handle(Geom_SurfaceOfRevolution) surface =
                Handle(Geom_SurfaceOfRevolution)::DownCast(untrimmedSurface(face));
            if (surface.IsNull())
                throw std::runtime_error("Surface-of-revolution data is unavailable.");

            const gp_Ax1 axis = surface->Axis();
            result->origin = pointValue(axis.Location());
            result->axis = directionValue(axis.Direction());
            result->basisCurveType = curveTypeOf(surface->BasisCurve());
        });
    }

    OcctStatus occt_model_face_offset_geometry(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelOffsetSurfaceGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};

        return executeStatus(model, [&]
        {
            const TopoDS_Face face = requireFace(model, faceId);
            const BRepAdaptor_Surface adaptor(face, Standard_False);
            if (adaptor.GetType() != GeomAbs_OffsetSurface)
                throw std::invalid_argument("Face surface is not an offset surface.");

            const Handle(Geom_OffsetSurface) surface =
                Handle(Geom_OffsetSurface)::DownCast(untrimmedSurface(face));
            if (surface.IsNull())
                throw std::runtime_error("Offset-surface data is unavailable.");

            result->offset = surface->Offset();
            result->basisSurfaceType = surfaceTypeOf(surface->BasisSurface());
        });
    }
}

#include "geometry/OcctModelingAnalyticGeometry.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <gp_Circ.hxx>
#include <gp_Cone.hxx>
#include <gp_Cylinder.hxx>
#include <gp_Elips.hxx>
#include <gp_Pln.hxx>
#include <gp_Sphere.hxx>
#include <gp_Torus.hxx>

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

    OcctPoint3d toNativePoint(const gp_Pnt& point)
    {
        return {point.X(), point.Y(), point.Z()};
    }

    OcctVector3d toNativeVector(const gp_Dir& direction)
    {
        return {direction.X(), direction.Y(), direction.Z()};
    }
}

extern "C"
{
    OcctStatus occt_model_edge_line_geometry(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelLineGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            if (curve.GetType() != GeomAbs_Line) throw std::invalid_argument("Edge curve is not a line.");
            const gp_Lin value = curve.Line();
            result->origin = toNativePoint(value.Location());
            result->direction = toNativeVector(value.Direction());
            result->firstParameter = curve.FirstParameter();
            result->lastParameter = curve.LastParameter();
        });
    }

    OcctStatus occt_model_edge_circle_geometry(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelCircleGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            if (curve.GetType() != GeomAbs_Circle) throw std::invalid_argument("Edge curve is not a circle.");
            const gp_Circ value = curve.Circle();
            result->center = toNativePoint(value.Position().Location());
            result->normal = toNativeVector(value.Position().Direction());
            result->xDirection = toNativeVector(value.Position().XDirection());
            result->radius = value.Radius();
            result->firstParameter = curve.FirstParameter();
            result->lastParameter = curve.LastParameter();
        });
    }

    OcctStatus occt_model_edge_ellipse_geometry(OcctModelingSessionHandle handle, OcctObjectId edgeId, OcctModelEllipseGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            if (curve.GetType() != GeomAbs_Ellipse) throw std::invalid_argument("Edge curve is not an ellipse.");
            const gp_Elips value = curve.Ellipse();
            result->center = toNativePoint(value.Position().Location());
            result->normal = toNativeVector(value.Position().Direction());
            result->xDirection = toNativeVector(value.Position().XDirection());
            result->majorRadius = value.MajorRadius();
            result->minorRadius = value.MinorRadius();
            result->firstParameter = curve.FirstParameter();
            result->lastParameter = curve.LastParameter();
        });
    }

    OcctStatus occt_model_face_plane_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelPlaneGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const BRepAdaptor_Surface surface(requireFace(model, faceId));
            if (surface.GetType() != GeomAbs_Plane) throw std::invalid_argument("Face surface is not a plane.");
            const gp_Pln value = surface.Plane();
            result->origin = toNativePoint(value.Position().Location());
            result->normal = toNativeVector(value.Position().Direction());
            result->xDirection = toNativeVector(value.Position().XDirection());
        });
    }

    OcctStatus occt_model_face_cylinder_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelCylinderGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const BRepAdaptor_Surface surface(requireFace(model, faceId));
            if (surface.GetType() != GeomAbs_Cylinder) throw std::invalid_argument("Face surface is not a cylinder.");
            const gp_Cylinder value = surface.Cylinder();
            result->origin = toNativePoint(value.Position().Location());
            result->axis = toNativeVector(value.Position().Direction());
            result->xDirection = toNativeVector(value.Position().XDirection());
            result->radius = value.Radius();
        });
    }

    OcctStatus occt_model_face_cone_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelConeGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const BRepAdaptor_Surface surface(requireFace(model, faceId));
            if (surface.GetType() != GeomAbs_Cone) throw std::invalid_argument("Face surface is not a cone.");
            const gp_Cone value = surface.Cone();
            result->apex = toNativePoint(value.Apex());
            result->axis = toNativeVector(value.Position().Direction());
            result->xDirection = toNativeVector(value.Position().XDirection());
            result->referenceRadius = value.RefRadius();
            result->semiAngleRadians = value.SemiAngle();
        });
    }

    OcctStatus occt_model_face_sphere_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelSphereGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const BRepAdaptor_Surface surface(requireFace(model, faceId));
            if (surface.GetType() != GeomAbs_Sphere) throw std::invalid_argument("Face surface is not a sphere.");
            const gp_Sphere value = surface.Sphere();
            result->center = toNativePoint(value.Position().Location());
            result->axis = toNativeVector(value.Position().Direction());
            result->xDirection = toNativeVector(value.Position().XDirection());
            result->radius = value.Radius();
        });
    }

    OcctStatus occt_model_face_torus_geometry(OcctModelingSessionHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            const BRepAdaptor_Surface surface(requireFace(model, faceId));
            if (surface.GetType() != GeomAbs_Torus) throw std::invalid_argument("Face surface is not a torus.");
            const gp_Torus value = surface.Torus();
            result->center = toNativePoint(value.Position().Location());
            result->axis = toNativeVector(value.Position().Direction());
            result->xDirection = toNativeVector(value.Position().XDirection());
            result->majorRadius = value.MajorRadius();
            result->minorRadius = value.MinorRadius();
        });
    }
}

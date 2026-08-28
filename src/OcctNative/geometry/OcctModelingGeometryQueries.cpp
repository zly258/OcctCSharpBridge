#include "geometry/OcctModelingGeometryQueries.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepTools.hxx>
#include <GCPnts_AbscissaPoint.hxx>
#include <TopAbs_Orientation.hxx>

#include <cmath>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_vertex_point(
        OcctModelingSessionHandle handle,
        OcctObjectId vertexId,
        OcctPoint3d* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(vertexId);
            if (shape.ShapeType() != TopAbs_VERTEX)
                throw std::invalid_argument("Input must be a vertex.");
            const gp_Pnt point = BRep_Tool::Pnt(TopoDS::Vertex(shape));
            *result = {point.X(), point.Y(), point.Z()};
        });
    }

    OcctStatus occt_model_edge_endpoints(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d* start,
        OcctPoint3d* end)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (start == nullptr || end == nullptr) return OcctStatus_ErrorInvalidArgument;

        *start = {};
        *end = {};
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Input must be an edge.");
            BRepAdaptor_Curve curve(TopoDS::Edge(shape));
            const gp_Pnt first = curve.Value(curve.FirstParameter());
            const gp_Pnt last = curve.Value(curve.LastParameter());
            *start = {first.X(), first.Y(), first.Z()};
            *end = {last.X(), last.Y(), last.Z()};
        });
    }

    OcctStatus occt_model_edge_point_at(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        double normalizedParameter,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultTangent)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (resultPoint == nullptr || resultTangent == nullptr) return OcctStatus_ErrorInvalidArgument;

        *resultPoint = {};
        *resultTangent = {};
        return executeStatus(model, [&]
        {
            if (normalizedParameter < 0.0 || normalizedParameter > 1.0)
                throw std::invalid_argument("Normalized parameter must be between 0 and 1.");
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Input must be an edge.");

            BRepAdaptor_Curve curve(TopoDS::Edge(shape));
            const double parameter = curve.FirstParameter() +
                (curve.LastParameter() - curve.FirstParameter()) * normalizedParameter;
            gp_Pnt point;
            gp_Vec tangent;
            curve.D1(parameter, point, tangent);
            if (tangent.SquareMagnitude() <= Precision::SquareConfusion())
                throw std::runtime_error("Edge tangent is undefined at this parameter.");
            tangent.Normalize();
            *resultPoint = {point.X(), point.Y(), point.Z()};
            *resultTangent = {tangent.X(), tangent.Y(), tangent.Z()};
        });
    }

    OcctStatus occt_model_edge_length(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        double* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = 0.0;
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Input must be an edge.");

            BRepAdaptor_Curve curve(TopoDS::Edge(shape));
            *result = GCPnts_AbscissaPoint::Length(
                curve,
                curve.FirstParameter(),
                curve.LastParameter());
        });
    }

    OcctStatus occt_model_edge_point_at_length(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        double length,
        double* curveParameter,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultTangent)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (curveParameter == nullptr || resultPoint == nullptr || resultTangent == nullptr)
            return OcctStatus_ErrorInvalidArgument;

        *curveParameter = 0.0;
        *resultPoint = {};
        *resultTangent = {};
        return executeStatus(model, [&]
        {
            if (!std::isfinite(length) || length < 0.0)
                throw std::invalid_argument("Arc length must be finite and non-negative.");

            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Input must be an edge.");

            BRepAdaptor_Curve curve(TopoDS::Edge(shape));
            const double totalLength = GCPnts_AbscissaPoint::Length(
                curve,
                curve.FirstParameter(),
                curve.LastParameter());
            if (length > totalLength + Precision::Confusion())
                throw std::out_of_range("Arc length exceeds the edge length.");

            double parameter = curve.FirstParameter();
            if (length >= totalLength - Precision::Confusion())
            {
                parameter = curve.LastParameter();
            }
            else if (length > Precision::Confusion())
            {
                GCPnts_AbscissaPoint abscissa(curve, length, curve.FirstParameter());
                if (!abscissa.IsDone())
                    throw std::runtime_error("Unable to resolve edge parameter from arc length.");
                parameter = abscissa.Parameter();
            }

            gp_Pnt point;
            gp_Vec tangent;
            curve.D1(parameter, point, tangent);
            if (tangent.SquareMagnitude() <= Precision::SquareConfusion())
                throw std::runtime_error("Edge tangent is undefined at the requested arc length.");
            tangent.Normalize();

            *curveParameter = parameter;
            *resultPoint = {point.X(), point.Y(), point.Z()};
            *resultTangent = {tangent.X(), tangent.Y(), tangent.Z()};
        });
    }

    OcctStatus occt_model_edge_curve_type(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        OcctCurveType* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = OcctCurve_Other;
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Input must be an edge.");
            *result = static_cast<OcctCurveType>(toOcctCurveType(BRepAdaptor_Curve(TopoDS::Edge(shape)).GetType()));
        });
    }

    OcctStatus occt_model_face_surface_type(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctSurfaceType* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = OcctSurface_Other;
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Input must be a face.");
            *result = static_cast<OcctSurfaceType>(toOcctSurfaceType(
                BRepAdaptor_Surface(TopoDS::Face(shape), Standard_True).GetType()));
        });
    }

    OcctStatus occt_model_face_uv_bounds(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctUvBounds* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Input must be a face.");
            BRepTools::UVBounds(TopoDS::Face(shape), result->uMin, result->uMax, result->vMin, result->vMax);
        });
    }

    OcctStatus occt_model_face_point_normal(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        double u,
        double v,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultNormal)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (resultPoint == nullptr || resultNormal == nullptr) return OcctStatus_ErrorInvalidArgument;

        *resultPoint = {};
        *resultNormal = {};
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Input must be a face.");

            const TopoDS_Face face = TopoDS::Face(shape);
            BRepAdaptor_Surface surface(face, Standard_True);
            gp_Pnt point;
            gp_Vec dU;
            gp_Vec dV;
            surface.D1(u, v, point, dU, dV);
            gp_Vec normal = dU.Crossed(dV);
            if (normal.SquareMagnitude() <= Precision::SquareConfusion())
                throw std::runtime_error("Face normal is undefined at this UV position.");
            normal.Normalize();
            if (face.Orientation() == TopAbs_REVERSED) normal.Reverse();
            *resultPoint = {point.X(), point.Y(), point.Z()};
            *resultNormal = {normal.X(), normal.Y(), normal.Z()};
        });
    }
}

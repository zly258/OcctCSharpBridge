#include "OcctModelingInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepLProp_CLProps.hxx>
#include <BRepLProp_SLProps.hxx>
#include <Precision.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>

#include <cmath>
#include <limits>
#include <stdexcept>

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

    OcctVector3d toNativeVector(const gp_Vec& vector)
    {
        return {vector.X(), vector.Y(), vector.Z()};
    }

    OcctVector3d toNativeVector(const gp_Dir& direction)
    {
        return {direction.X(), direction.Y(), direction.Z()};
    }

    void validateCurveParameter(const BRepAdaptor_Curve& curve, double parameter)
    {
        const double first = curve.FirstParameter();
        const double last = curve.LastParameter();
        const double tolerance = Precision::PConfusion();
        if ((std::isfinite(first) && parameter < first - tolerance) ||
            (std::isfinite(last) && parameter > last + tolerance))
        {
            throw std::out_of_range("Curve parameter is outside the edge range.");
        }
    }

    bool isReversed(const TopoDS_Face& face)
    {
        return face.Orientation() == TopAbs_REVERSED;
    }
}

extern "C"
{
    int occt_model_edge_parameter_range(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        OcctModelParameterRange* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            result->firstParameter = curve.FirstParameter();
            result->lastParameter = curve.LastParameter();
            result->isClosed = curve.IsClosed() ? 1 : 0;
            result->isPeriodic = curve.IsPeriodic() ? 1 : 0;
            result->period = result->isPeriodic != 0 ? curve.Period() : 0.0;
        });
    }

    int occt_model_edge_differential(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        double parameter,
        OcctModelCurveDifferential* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            validateCurveParameter(curve, parameter);

            gp_Pnt point;
            gp_Vec firstDerivative;
            gp_Vec secondDerivative;
            curve.D2(parameter, point, firstDerivative, secondDerivative);

            result->parameter = parameter;
            result->point = toNativePoint(point);
            result->firstDerivative = toNativeVector(firstDerivative);
            result->secondDerivative = toNativeVector(secondDerivative);
        });
    }

    int occt_model_edge_curvature(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        double parameter,
        double resolution,
        OcctModelCurveCurvature* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            requirePositive(resolution, "Resolution");
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            validateCurveParameter(curve, parameter);
            BRepLProp_CLProps properties(curve, parameter, 2, resolution);

            result->parameter = parameter;
            result->point = toNativePoint(properties.Value());
            result->tangent = {0.0, 0.0, 0.0};
            result->normal = {0.0, 0.0, 0.0};
            result->centerOfCurvature = result->point;
            result->curvature = 0.0;
            result->hasTangent = 0;
            result->hasNormal = 0;
            result->hasCenterOfCurvature = 0;

            if (!properties.IsTangentDefined()) return;

            gp_Dir tangent;
            properties.Tangent(tangent);
            result->tangent = toNativeVector(tangent);
            result->hasTangent = 1;
            result->curvature = properties.Curvature();

            if (std::abs(result->curvature) <= std::numeric_limits<double>::epsilon()) return;

            gp_Dir normal;
            gp_Pnt center;
            properties.Normal(normal);
            properties.CentreOfCurvature(center);
            result->normal = toNativeVector(normal);
            result->centerOfCurvature = toNativePoint(center);
            result->hasNormal = 1;
            result->hasCenterOfCurvature = 1;
        });
    }

    int occt_model_face_periodicity(
        OcctModelHandle handle,
        OcctObjectId faceId,
        OcctModelSurfacePeriodicity* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const BRepAdaptor_Surface surface(requireFace(model, faceId));
            result->isUClosed = surface.IsUClosed() ? 1 : 0;
            result->isVClosed = surface.IsVClosed() ? 1 : 0;
            result->isUPeriodic = surface.IsUPeriodic() ? 1 : 0;
            result->isVPeriodic = surface.IsVPeriodic() ? 1 : 0;
            result->uPeriod = result->isUPeriodic != 0 ? surface.UPeriod() : 0.0;
            result->vPeriod = result->isVPeriodic != 0 ? surface.VPeriod() : 0.0;
        });
    }

    int occt_model_face_differential(
        OcctModelHandle handle,
        OcctObjectId faceId,
        double u,
        double v,
        double resolution,
        OcctModelSurfaceDifferential* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            requirePositive(resolution, "Resolution");
            const TopoDS_Face face = requireFace(model, faceId);
            const BRepAdaptor_Surface surface(face);

            gp_Pnt point;
            gp_Vec uDerivative;
            gp_Vec vDerivative;
            gp_Vec uSecondDerivative;
            gp_Vec vSecondDerivative;
            gp_Vec uvDerivative;
            surface.D2(
                u,
                v,
                point,
                uDerivative,
                vDerivative,
                uSecondDerivative,
                vSecondDerivative,
                uvDerivative);

            result->u = u;
            result->v = v;
            result->point = toNativePoint(point);
            result->normal = {0.0, 0.0, 0.0};
            result->uDerivative = toNativeVector(uDerivative);
            result->vDerivative = toNativeVector(vDerivative);
            result->uSecondDerivative = toNativeVector(uSecondDerivative);
            result->vSecondDerivative = toNativeVector(vSecondDerivative);
            result->uvDerivative = toNativeVector(uvDerivative);
            result->hasNormal = 0;

            const gp_Vec cross = uDerivative.Crossed(vDerivative);
            if (cross.SquareMagnitude() <= resolution * resolution) return;

            gp_Dir normal(cross);
            if (isReversed(face)) normal.Reverse();
            result->normal = toNativeVector(normal);
            result->hasNormal = 1;
        });
    }

    int occt_model_face_curvature(
        OcctModelHandle handle,
        OcctObjectId faceId,
        double u,
        double v,
        double resolution,
        OcctModelSurfaceCurvature* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            requirePositive(resolution, "Resolution");
            const TopoDS_Face face = requireFace(model, faceId);
            const BRepAdaptor_Surface surface(face);
            BRepLProp_SLProps properties(surface, u, v, 2, resolution);

            result->u = u;
            result->v = v;
            result->point = toNativePoint(properties.Value());
            result->normal = {0.0, 0.0, 0.0};
            result->maximumDirection = {0.0, 0.0, 0.0};
            result->minimumDirection = {0.0, 0.0, 0.0};
            result->maximumCurvature = 0.0;
            result->minimumCurvature = 0.0;
            result->meanCurvature = 0.0;
            result->gaussianCurvature = 0.0;
            result->isUmbilic = 0;
            result->hasNormal = 0;
            result->hasCurvature = 0;

            if (properties.IsNormalDefined())
            {
                gp_Dir normal = properties.Normal();
                if (isReversed(face)) normal.Reverse();
                result->normal = toNativeVector(normal);
                result->hasNormal = 1;
            }

            if (!properties.IsCurvatureDefined()) return;

            gp_Dir maximumDirection;
            gp_Dir minimumDirection;
            properties.CurvatureDirections(maximumDirection, minimumDirection);
            const double maximumCurvature = properties.MaxCurvature();
            const double minimumCurvature = properties.MinCurvature();

            if (isReversed(face))
            {
                result->maximumDirection = toNativeVector(minimumDirection);
                result->minimumDirection = toNativeVector(maximumDirection);
                result->maximumCurvature = -minimumCurvature;
                result->minimumCurvature = -maximumCurvature;
                result->meanCurvature = -properties.MeanCurvature();
            }
            else
            {
                result->maximumDirection = toNativeVector(maximumDirection);
                result->minimumDirection = toNativeVector(minimumDirection);
                result->maximumCurvature = maximumCurvature;
                result->minimumCurvature = minimumCurvature;
                result->meanCurvature = properties.MeanCurvature();
            }

            result->gaussianCurvature = properties.GaussianCurvature();
            result->isUmbilic = properties.IsUmbilic() ? 1 : 0;
            result->hasCurvature = 1;
        });
    }
}

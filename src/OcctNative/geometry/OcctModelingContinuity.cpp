#include "geometry/OcctModelingContinuity.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <Precision.hxx>
#include <Standard_Failure.hxx>
#include <TopoDS.hxx>

#include <algorithm>
#include <cmath>
#include <limits>
#include <stdexcept>

using namespace OcctModelingInternal;

namespace
{
    struct EndpointSample
    {
        gp_Pnt point;
        gp_Vec first;
        gp_Vec second;
        bool hasFirst = false;
        bool hasSecond = false;
    };

    EndpointSample sampleEndpoint(const TopoDS_Edge& edge, bool atEnd)
    {
        BRepAdaptor_Curve curve(edge);
        const double parameter = atEnd ? curve.LastParameter() : curve.FirstParameter();
        EndpointSample sample;
        curve.D0(parameter, sample.point);
        try
        {
            curve.D1(parameter, sample.point, sample.first);
            sample.hasFirst = sample.first.SquareMagnitude() > Precision::SquareConfusion();
            try
            {
                curve.D2(parameter, sample.point, sample.first, sample.second);
                sample.hasSecond = sample.hasFirst;
            }
            catch (const Standard_Failure&)
            {
                sample.hasSecond = false;
            }
        }
        catch (const Standard_Failure&)
        {
            sample.hasFirst = false;
            sample.hasSecond = false;
        }
        return sample;
    }

    gp_Vec curvatureVector(const gp_Vec& first, const gp_Vec& second)
    {
        const double squaredSpeed = first.SquareMagnitude();
        if (squaredSpeed <= Precision::SquareConfusion()) return gp_Vec();
        return first.Crossed(second).Crossed(first) / (squaredSpeed * squaredSpeed);
    }

    void validateOptions(const OcctModelContinuityOptions& options)
    {
        const double values[] =
        {
            options.positionTolerance,
            options.angularTolerance,
            options.curvatureTolerance,
            options.firstDerivativeTolerance,
            options.secondDerivativeTolerance
        };
        for (const double value : values)
        {
            if (!std::isfinite(value) || value < 0.0)
                throw std::invalid_argument("Continuity tolerances must be finite and non-negative.");
        }
    }
}

extern "C"
{
    OcctStatus occt_model_curve_continuity_analyze(
        OcctModelingSessionHandle handle,
        OcctObjectId firstEdgeId,
        OcctBool firstAtEnd,
        OcctObjectId secondEdgeId,
        OcctBool secondAtStart,
        const OcctModelContinuityOptions* options,
        OcctModelCurveContinuityResult* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (options == nullptr || result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            validateOptions(*options);
            const TopoDS_Shape& firstShape = model->requireShape(firstEdgeId);
            const TopoDS_Shape& secondShape = model->requireShape(secondEdgeId);
            if (firstShape.ShapeType() != TopAbs_EDGE || secondShape.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Curve continuity analysis requires two edge shapes.");

            EndpointSample first = sampleEndpoint(TopoDS::Edge(firstShape), firstAtEnd != 0);
            EndpointSample second = sampleEndpoint(TopoDS::Edge(secondShape), secondAtStart == 0);
            result->positionGap = first.point.Distance(second.point);
            result->tangentAngleRadians = std::numeric_limits<double>::quiet_NaN();
            result->curvatureVectorGap = std::numeric_limits<double>::quiet_NaN();
            result->firstSpeed = first.hasFirst ? first.first.Magnitude() : 0.0;
            result->secondSpeed = second.hasFirst ? second.first.Magnitude() : 0.0;
            result->hasFirstDerivatives = first.hasFirst && second.hasFirst ? 1 : 0;
            result->hasSecondDerivatives = first.hasSecond && second.hasSecond ? 1 : 0;

            const bool c0 = result->positionGap <= options->positionTolerance;
            const bool g0 = c0;
            bool c1 = false;
            bool c2 = false;
            bool g1 = false;
            bool g2 = false;
            if (result->hasFirstDerivatives != 0)
            {
                gp_Vec firstOriented = first.first;
                gp_Vec secondOriented = second.first;
                if (firstAtEnd == 0) firstOriented.Reverse();
                if (secondAtStart == 0) secondOriented.Reverse();
                result->tangentAngleRadians = firstOriented.Angle(secondOriented);
                c1 = c0 && (firstOriented - secondOriented).Magnitude() <=
                    options->firstDerivativeTolerance;
                g1 = g0 && result->tangentAngleRadians <= options->angularTolerance;

                if (result->hasSecondDerivatives != 0)
                {
                    const gp_Vec firstCurvature = curvatureVector(first.first, first.second);
                    const gp_Vec secondCurvature = curvatureVector(second.first, second.second);
                    result->firstCurvature = firstCurvature.Magnitude();
                    result->secondCurvature = secondCurvature.Magnitude();
                    result->curvatureVectorGap = (firstCurvature - secondCurvature).Magnitude();
                    c2 = c1 && (first.second - second.second).Magnitude() <=
                        options->secondDerivativeTolerance;
                    g2 = g1 && result->curvatureVectorGap <= options->curvatureTolerance;
                }
            }

            result->parametricLevel = c2 ? 3 : (c1 ? 2 : (c0 ? 1 : 0));
            result->geometricLevel = g2 ? 3 : (g1 ? 2 : (g0 ? 1 : 0));
        });
    }
}

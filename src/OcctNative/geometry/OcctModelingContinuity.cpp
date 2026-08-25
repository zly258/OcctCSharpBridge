#include "geometry/OcctModelingContinuity.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepClass_FaceClassifier.hxx>
#include <BRepLProp_CLProps.hxx>
#include <BRepLProp_SLProps.hxx>
#include <BRep_Tool.hxx>
#include <Geom2d_Curve.hxx>
#include <GeomAbs_Shape.hxx>
#include <GeomLProp_SLProps.hxx>
#include <Geom_Surface.hxx>
#include <Precision.hxx>
#include <Standard_Failure.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>
#include <gp_Pnt2d.hxx>

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

    TopoDS_Edge requireEdge(ModelSession* model, OcctObjectId id)
    {
        const TopoDS_Shape& shape = model->requireShape(id);
        if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
        return TopoDS::Edge(shape);
    }

    TopoDS_Face requireFace(ModelSession* model, OcctObjectId id)
    {
        const TopoDS_Shape& shape = model->requireShape(id);
        if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
        return TopoDS::Face(shape);
    }

    OcctPoint3d nativePoint(const gp_Pnt& point)
    {
        return {point.X(), point.Y(), point.Z()};
    }

    OcctVector3d nativeVector(const gp_Vec& vector)
    {
        return {vector.X(), vector.Y(), vector.Z()};
    }

    OcctVector3d nativeVector(const gp_Dir& direction)
    {
        return {direction.X(), direction.Y(), direction.Z()};
    }

    int parametricLevel(GeomAbs_Shape continuity)
    {
        switch (continuity)
        {
            case GeomAbs_C1: return 2;
            case GeomAbs_C2:
            case GeomAbs_C3:
            case GeomAbs_CN: return 3;
            case GeomAbs_C0: return 1;
            default: return 0;
        }
    }

    int geometricLevel(GeomAbs_Shape continuity)
    {
        switch (continuity)
        {
            case GeomAbs_G1:
            case GeomAbs_C1: return 2;
            case GeomAbs_G2:
            case GeomAbs_C2:
            case GeomAbs_C3:
            case GeomAbs_CN: return 3;
            case GeomAbs_C0: return 1;
            default: return 0;
        }
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
    struct SurfaceBoundarySample
    {
        gp_Pnt point;
        gp_Dir normal;
        double maximumCurvature = 0.0;
        double minimumCurvature = 0.0;
        bool valid = false;
        bool hasCurvature = false;
    };

    SurfaceBoundarySample sampleBoundary(
        const TopoDS_Face& face,
        const Handle(Geom2d_Curve)& curve,
        double parameter,
        double resolution)
    {
        SurfaceBoundarySample result;
        const gp_Pnt2d uv = curve->Value(parameter);
        TopLoc_Location location;
        const Handle(Geom_Surface) surface = BRep_Tool::Surface(face, location);
        if (surface.IsNull()) return result;
        GeomLProp_SLProps properties(surface, uv.X(), uv.Y(), 2, resolution);
        if (!properties.IsNormalDefined()) return result;

        result.point = properties.Value().Transformed(location.Transformation());
        result.normal = properties.Normal();
        result.normal.Transform(location.Transformation());
        if (face.Orientation() == TopAbs_REVERSED) result.normal.Reverse();
        result.valid = true;
        if (properties.IsCurvatureDefined())
        {
            result.maximumCurvature = properties.MaxCurvature();
            result.minimumCurvature = properties.MinCurvature();
            if (face.Orientation() == TopAbs_REVERSED)
            {
                result.maximumCurvature = -properties.MinCurvature();
                result.minimumCurvature = -properties.MaxCurvature();
            }
            result.hasCurvature = true;
        }
        return result;
    }

    double tangentPlaneAngle(const gp_Dir& first, const gp_Dir& second)
    {
        const double angle = first.Angle(second);
        return std::min(angle, std::abs(3.14159265358979323846 - angle));
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

    OcctStatus occt_model_surface_continuity_analyze(
        OcctModelingSessionHandle handle,
        OcctObjectId firstFaceId,
        OcctObjectId secondFaceId,
        OcctObjectId sharedEdgeId,
        int sampleCount,
        const OcctModelContinuityOptions* options,
        OcctModelSurfaceContinuityResult* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (options == nullptr || result == nullptr || sampleCount < 2)
            return OcctStatus_ErrorInvalidArgument;
        *result = {};
        return executeStatus(model, [&]
        {
            validateOptions(*options);
            const TopoDS_Face firstFace = requireFace(model, firstFaceId);
            const TopoDS_Face secondFace = requireFace(model, secondFaceId);
            const TopoDS_Edge edge = requireEdge(model, sharedEdgeId);

            double firstStart = 0.0;
            double firstEnd = 0.0;
            double secondStart = 0.0;
            double secondEnd = 0.0;
            const Handle(Geom2d_Curve) firstCurve =
                BRep_Tool::CurveOnSurface(edge, firstFace, firstStart, firstEnd);
            const Handle(Geom2d_Curve) secondCurve =
                BRep_Tool::CurveOnSurface(edge, secondFace, secondStart, secondEnd);
            if (firstCurve.IsNull() || secondCurve.IsNull())
                throw std::invalid_argument("The edge is not shared by both supplied faces.");

            const GeomAbs_Shape declared = BRep_Tool::Continuity(edge, firstFace, secondFace);
            result->declaredParametricLevel = parametricLevel(declared);
            result->declaredGeometricLevel = geometricLevel(declared);
            result->sampleCount = sampleCount;
            const double boundaryResolution =
                std::max(options->positionTolerance, Precision::Confusion());
            const SurfaceBoundarySample firstStartSample =
                sampleBoundary(firstFace, firstCurve, firstStart, boundaryResolution);
            const SurfaceBoundarySample firstEndSample =
                sampleBoundary(firstFace, firstCurve, firstEnd, boundaryResolution);
            const SurfaceBoundarySample secondStartSample =
                sampleBoundary(secondFace, secondCurve, secondStart, boundaryResolution);
            const SurfaceBoundarySample secondEndSample =
                sampleBoundary(secondFace, secondCurve, secondEnd, boundaryResolution);
            const bool reverseSecond =
                firstStartSample.valid && firstEndSample.valid &&
                secondStartSample.valid && secondEndSample.valid &&
                firstStartSample.point.Distance(secondEndSample.point) +
                    firstEndSample.point.Distance(secondStartSample.point) <
                firstStartSample.point.Distance(secondStartSample.point) +
                    firstEndSample.point.Distance(secondEndSample.point);

            bool allPosition = true;
            bool allTangent = true;
            bool allCurvature = true;

            for (int index = 0; index < sampleCount; ++index)
            {
                const double ratio = static_cast<double>(index) / static_cast<double>(sampleCount - 1);
                const SurfaceBoundarySample first = sampleBoundary(
                    firstFace, firstCurve, firstStart + (firstEnd - firstStart) * ratio,
                    std::max(options->positionTolerance, Precision::Confusion()));
                const double secondRatio = reverseSecond ? 1.0 - ratio : ratio;
                const SurfaceBoundarySample second = sampleBoundary(
                    secondFace, secondCurve,
                    secondStart + (secondEnd - secondStart) * secondRatio,
                    boundaryResolution);
                if (!first.valid || !second.valid)
                {
                    ++result->invalidSampleCount;
                    allPosition = allTangent = allCurvature = false;
                    continue;
                }

                const double gap = first.point.Distance(second.point);
                const double angle = tangentPlaneAngle(first.normal, second.normal);
                result->maximumPositionGap = std::max(result->maximumPositionGap, gap);
                result->maximumNormalAngleRadians =
                    std::max(result->maximumNormalAngleRadians, angle);
                allPosition = allPosition && gap <= options->positionTolerance;
                allTangent = allTangent && angle <= options->angularTolerance;

                if (!first.hasCurvature || !second.hasCurvature)
                {
                    allCurvature = false;
                    continue;
                }
                const double direct = std::max(
                    std::abs(first.maximumCurvature - second.maximumCurvature),
                    std::abs(first.minimumCurvature - second.minimumCurvature));
                const double reversed = std::max(
                    std::abs(first.maximumCurvature + second.minimumCurvature),
                    std::abs(first.minimumCurvature + second.maximumCurvature));
                const double curvatureGap = std::min(direct, reversed);
                result->maximumCurvatureGap =
                    std::max(result->maximumCurvatureGap, curvatureGap);
                allCurvature = allCurvature && curvatureGap <= options->curvatureTolerance;
            }

            result->measuredGeometricLevel = allPosition
                ? (allTangent ? (allCurvature ? 3 : 2) : 1)
                : 0;
        });
    }

    OcctStatus occt_model_curvature_comb_copy(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        int sampleCount,
        double scale,
        double resolution,
        OcctModelCurvatureCombSample* samples,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (sampleCount < 2 || sampleCount > 1000000 || !std::isfinite(scale) || scale < 0.0 ||
            !std::isfinite(resolution) || resolution <= 0.0 ||
            capacity < 0 || required == nullptr)
            return OcctStatus_ErrorInvalidArgument;
        *required = sampleCount;
        if (samples == nullptr) return capacity == 0 ? OcctStatus_Ok : OcctStatus_ErrorInvalidArgument;
        if (capacity < sampleCount) return OcctStatus_ErrorBufferTooSmall;

        return executeStatus(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            const double first = curve.FirstParameter();
            const double last = curve.LastParameter();
            for (int index = 0; index < sampleCount; ++index)
            {
                OcctModelCurvatureCombSample& sample = samples[index];
                sample = {};
                const double ratio = static_cast<double>(index) / static_cast<double>(sampleCount - 1);
                sample.parameter = first + (last - first) * ratio;
                BRepLProp_CLProps properties(curve, sample.parameter, 2, resolution);
                sample.point = nativePoint(properties.Value());
                if (!properties.IsTangentDefined()) continue;
                const double curvature = properties.Curvature();
                sample.curvature = curvature;
                if (std::abs(curvature) <= std::numeric_limits<double>::epsilon())
                {
                    sample.valid = 1;
                    continue;
                }
                gp_Dir normal;
                properties.Normal(normal);
                sample.combVector = nativeVector(gp_Vec(normal) * (curvature * scale));
                sample.valid = 1;
            }
        });
    }

    OcctStatus occt_model_surface_quality_copy(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        const OcctModelSurfaceQualityOptions* options,
        OcctModelSurfaceQualitySample* samples,
        int capacity,
        int* required,
        OcctModelSurfaceQualitySummary* summary)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (options == nullptr || required == nullptr || summary == nullptr ||
            options->uSamples < 2 || options->vSamples < 2 ||
            options->uSamples > 4096 || options->vSamples > 4096 ||
            !std::isfinite(options->resolution) || options->resolution <= 0.0 ||
            !std::isfinite(options->zebraFrequency) || options->zebraFrequency <= 0.0 ||
            !std::isfinite(options->zebraPhase) || capacity < 0)
            return OcctStatus_ErrorInvalidArgument;
        const std::int64_t wideCount =
            static_cast<std::int64_t>(options->uSamples) * options->vSamples;
        if (wideCount <= 0 || wideCount > std::numeric_limits<int>::max())
            return OcctStatus_ErrorInvalidArgument;
        const int count = static_cast<int>(wideCount);
        *required = count;
        *summary = {};
        if (samples == nullptr) return capacity == 0 ? OcctStatus_Ok : OcctStatus_ErrorInvalidArgument;
        if (capacity < count) return OcctStatus_ErrorBufferTooSmall;

        return executeStatus(model, [&]
        {
            const TopoDS_Face face = requireFace(model, faceId);
            const BRepAdaptor_Surface surface(face);
            const double uFirst = surface.FirstUParameter();
            const double uLast = surface.LastUParameter();
            const double vFirst = surface.FirstVParameter();
            const double vLast = surface.LastVParameter();
            if (!std::isfinite(uFirst) || !std::isfinite(uLast) ||
                !std::isfinite(vFirst) || !std::isfinite(vLast))
                throw std::invalid_argument("Surface quality analysis requires finite face parameter bounds.");

            gp_Vec view(
                options->viewDirection.x,
                options->viewDirection.y,
                options->viewDirection.z);
            if (view.SquareMagnitude() <= Precision::SquareConfusion())
                throw std::invalid_argument("Zebra view direction must not be zero.");
            view.Normalize();
            summary->minimumMeanCurvature = std::numeric_limits<double>::infinity();
            summary->maximumMeanCurvature = -std::numeric_limits<double>::infinity();
            summary->minimumGaussianCurvature = std::numeric_limits<double>::infinity();
            summary->maximumGaussianCurvature = -std::numeric_limits<double>::infinity();

            gp_Vec previousNormal;
            bool hasPreviousNormal = false;
            int outputIndex = 0;
            for (int vIndex = 0; vIndex < options->vSamples; ++vIndex)
            {
                const double vRatio =
                    static_cast<double>(vIndex) / static_cast<double>(options->vSamples - 1);
                const double v = vFirst + (vLast - vFirst) * vRatio;
                for (int uIndex = 0; uIndex < options->uSamples; ++uIndex, ++outputIndex)
                {
                    OcctModelSurfaceQualitySample& sample = samples[outputIndex];
                    sample = {};
                    const double uRatio =
                        static_cast<double>(uIndex) / static_cast<double>(options->uSamples - 1);
                    const double u = uFirst + (uLast - uFirst) * uRatio;
                    sample.u = u;
                    sample.v = v;

                    BRepClass_FaceClassifier classifier(face, gp_Pnt2d(u, v), options->resolution);
                    if (classifier.State() != TopAbs_IN && classifier.State() != TopAbs_ON)
                    {
                        ++summary->invalidSampleCount;
                        continue;
                    }

                    BRepLProp_SLProps properties(surface, u, v, 2, options->resolution);
                    sample.point = nativePoint(properties.Value());
                    if (!properties.IsNormalDefined() || !properties.IsCurvatureDefined())
                    {
                        ++summary->invalidSampleCount;
                        continue;
                    }

                    gp_Dir normal = properties.Normal();
                    if (face.Orientation() == TopAbs_REVERSED) normal.Reverse();
                    sample.normal = nativeVector(normal);
                    sample.maximumCurvature = properties.MaxCurvature();
                    sample.minimumCurvature = properties.MinCurvature();
                    sample.meanCurvature = properties.MeanCurvature();
                    if (face.Orientation() == TopAbs_REVERSED)
                    {
                        sample.maximumCurvature = -properties.MinCurvature();
                        sample.minimumCurvature = -properties.MaxCurvature();
                        sample.meanCurvature = -sample.meanCurvature;
                    }
                    sample.gaussianCurvature = properties.GaussianCurvature();

                    const gp_Vec n(normal);
                    const gp_Vec reflected = view - n * (2.0 * view.Dot(n));
                    const double stripeCoordinate = std::acos(std::clamp(
                        reflected.Z() / reflected.Magnitude(), -1.0, 1.0));
                    sample.zebraIntensity = 0.5 + 0.5 * std::cos(
                        options->zebraFrequency * stripeCoordinate + options->zebraPhase);
                    sample.valid = 1;
                    ++summary->validSampleCount;

                    summary->minimumMeanCurvature =
                        std::min(summary->minimumMeanCurvature, sample.meanCurvature);
                    summary->maximumMeanCurvature =
                        std::max(summary->maximumMeanCurvature, sample.meanCurvature);
                    summary->minimumGaussianCurvature =
                        std::min(summary->minimumGaussianCurvature, sample.gaussianCurvature);
                    summary->maximumGaussianCurvature =
                        std::max(summary->maximumGaussianCurvature, sample.gaussianCurvature);
                    summary->maximumAbsoluteCurvature = std::max(
                        summary->maximumAbsoluteCurvature,
                        std::max(std::abs(sample.maximumCurvature),
                                 std::abs(sample.minimumCurvature)));

                    if (hasPreviousNormal && previousNormal.Dot(n) < 0.0)
                        ++summary->normalFlipCount;
                    previousNormal = n;
                    hasPreviousNormal = true;
                }
            }
            if (summary->validSampleCount == 0)
            {
                summary->minimumMeanCurvature = 0.0;
                summary->maximumMeanCurvature = 0.0;
                summary->minimumGaussianCurvature = 0.0;
                summary->maximumGaussianCurvature = 0.0;
            }
        });
    }
}

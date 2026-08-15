#pragma once

#include "OcctNative.h"

#include <cstdint>

extern "C"
{
    using OcctOperationId = std::int64_t;

    enum OcctModelState
    {
        OcctModelState_Unknown = 0,
        OcctModelState_Inside = 1,
        OcctModelState_Outside = 2,
        OcctModelState_On = 3
    };

    enum OcctModelOrientation
    {
        OcctModelOrientation_Forward = 0,
        OcctModelOrientation_Reversed = 1,
        OcctModelOrientation_Internal = 2,
        OcctModelOrientation_External = 3
    };

    enum OcctModelBooleanOperation
    {
        OcctModelBoolean_Fuse = 0,
        OcctModelBoolean_Cut = 1,
        OcctModelBoolean_Common = 2,
        OcctModelBoolean_Section = 3
    };

    enum OcctModelBooleanGlue
    {
        OcctModelGlue_Off = 0,
        OcctModelGlue_Shift = 1,
        OcctModelGlue_Full = 2
    };

    struct OcctModelBooleanOptions
    {
        double fuzzyValue;
        double angularTolerance;
        int runParallel;
        int nonDestructive;
        int glue;
        int checkInverted;
        int simplifyEdges;
        int simplifyFaces;
    };

    struct OcctModelAlgorithmResult
    {
        OcctObjectId shapeId;
        OcctOperationId operationId;
        int succeeded;
        int hasWarnings;
        int hasErrors;
    };

    struct OcctModelTopologyHistorySummary
    {
        int generatedCount;
        int modifiedCount;
        OcctBool removed;
    };

    struct OcctModelProjectionResult
    {
        OcctPoint3d point;
        double distance;
        double parameter;
        double u;
        double v;
    };

    struct OcctModelRayHit
    {
        OcctPoint3d point;
        OcctObjectId faceId;
        double rayParameter;
        double u;
        double v;
        int state;
    };

    struct OcctModelMeshParameters
    {
        double linearDeflection;
        double angularDeflection;
        double minSize;
        int relative;
        int parallel;
        int internalVertices;
        int controlSurfaceDeflection;
    };

    struct OcctModelMeshNode
    {
        OcctPoint3d point;
        double u;
        double v;
        OcctVector3d normal;
        int hasUv;
        int hasNormal;
    };

    struct OcctModelMeshTriangle
    {
        int node1;
        int node2;
        int node3;
    };

    struct OcctModelLineGeometry
    {
        OcctPoint3d origin;
        OcctVector3d direction;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelCircleGeometry
    {
        OcctPoint3d center;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double radius;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelEllipseGeometry
    {
        OcctPoint3d center;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double majorRadius;
        double minorRadius;
        double firstParameter;
        double lastParameter;
    };

    struct OcctModelPlaneGeometry
    {
        OcctPoint3d origin;
        OcctVector3d normal;
        OcctVector3d xDirection;
    };

    struct OcctModelCylinderGeometry
    {
        OcctPoint3d origin;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double radius;
    };

    struct OcctModelConeGeometry
    {
        OcctPoint3d apex;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double referenceRadius;
        double semiAngleRadians;
    };

    struct OcctModelSphereGeometry
    {
        OcctPoint3d center;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double radius;
    };

    struct OcctModelTorusGeometry
    {
        OcctPoint3d center;
        OcctVector3d axis;
        OcctVector3d xDirection;
        double majorRadius;
        double minorRadius;
    };

    struct OcctModelParameterRange
    {
        double firstParameter;
        double lastParameter;
        int isClosed;
        int isPeriodic;
        double period;
    };

    struct OcctModelCurveDifferential
    {
        double parameter;
        OcctPoint3d point;
        OcctVector3d firstDerivative;
        OcctVector3d secondDerivative;
    };

    struct OcctModelCurveCurvature
    {
        double parameter;
        OcctPoint3d point;
        OcctVector3d tangent;
        OcctVector3d normal;
        OcctPoint3d centerOfCurvature;
        double curvature;
        int hasTangent;
        int hasNormal;
        int hasCenterOfCurvature;
    };

    struct OcctModelSurfacePeriodicity
    {
        int isUClosed;
        int isVClosed;
        int isUPeriodic;
        int isVPeriodic;
        double uPeriod;
        double vPeriod;
    };

    struct OcctModelSurfaceDifferential
    {
        double u;
        double v;
        OcctPoint3d point;
        OcctVector3d normal;
        OcctVector3d uDerivative;
        OcctVector3d vDerivative;
        OcctVector3d uSecondDerivative;
        OcctVector3d vSecondDerivative;
        OcctVector3d uvDerivative;
        int hasNormal;
    };

    struct OcctModelSurfaceCurvature
    {
        double u;
        double v;
        OcctPoint3d point;
        OcctVector3d normal;
        OcctVector3d maximumDirection;
        OcctVector3d minimumDirection;
        double maximumCurvature;
        double minimumCurvature;
        double meanCurvature;
        double gaussianCurvature;
        int isUmbilic;
        int hasNormal;
        int hasCurvature;
    };

    struct OcctModelLocation
    {
        double m11; double m12; double m13; double m14;
        double m21; double m22; double m23; double m24;
        double m31; double m32; double m33; double m34;
        double m41; double m42; double m43; double m44;
    };
}

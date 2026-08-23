#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_feature_extrude_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId profileId,
        OcctVector3d vector,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_feature_revolve_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId profileId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_feature_sweep_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId spineWireId,
        OcctObjectId profileId,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_feature_loft_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* wireIds,
        int count,
        OcctBool makeSolid,
        OcctBool ruled,
        double tolerance,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_feature_fillet_edges_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const int* edgeIndices,
        int count,
        double radius,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_feature_chamfer_edges_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const int* edgeIndices,
        int count,
        double distance,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_feature_offset_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double offset,
        double tolerance,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_feature_thick_solid_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId solidId,
        const int* faceIndicesToRemove,
        int count,
        double thickness,
        double tolerance,
        OcctModelAlgorithmResult* result);

    struct OcctDraftOptions {
        uint32_t structSize;
        uint32_t apiVersion;
        double angleDegrees;
        OcctVector3d pullDirection;
        OcctPoint3d  neutralPlanePoint;
        OcctVector3d neutralPlaneNormal;
    };

    OCCTBRIDGE_API OcctStatus occt_model_feature_draft_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId solidId,
        const int* faceIndices, int faceCount,
        const OcctDraftOptions* options,
        OcctModelAlgorithmResult* result);

    struct OcctEdgeFilletSpec {
        int edgeIndex;
        double r1;   // 起点半径
        double r2;   // 终点半径
    };

    OCCTBRIDGE_API OcctStatus occt_model_feature_fillet_variable_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const OcctEdgeFilletSpec* specs,
        int count,
        OcctModelAlgorithmResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_feature_loft_guided_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* sectionWireIds, int sectionCount,
        const OcctObjectId* guideWireIds,   int guideCount,
        OcctBool makeSolid,
        double tolerance,
        OcctModelAlgorithmResult* result);
}

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
}

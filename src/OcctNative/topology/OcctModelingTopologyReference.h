#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    enum OcctModelTopologyReferenceStatus
    {
        OcctModelTopologyReference_Resolved = 0,
        OcctModelTopologyReference_Ambiguous = 1,
        OcctModelTopologyReference_Removed = 2,
        OcctModelTopologyReference_NotFound = 3,
        OcctModelTopologyReference_Invalid = 4
    };

    struct OcctModelTopologyReference
    {
        int version;
        int shapeType;
        int runtimeIndexHint;
        int curveType;
        int surfaceType;
        double measure;
        OcctPoint3d center;
        OcctBounds bounds;
        double tolerance;
        int orientation;
        int vertexCount;
        int edgeCount;
        int faceCount;
    };

    struct OcctModelTopologyReferenceResult
    {
        int status;
        OcctObjectId shapeId;
        double score;
        int candidateCount;
        int usedOperationHistory;
        int runtimeIndexMatched;
    };

    OCCTBRIDGE_API OcctStatus occt_model_create_topology_reference(
        OcctModelingSessionHandle handle,
        OcctObjectId rootShapeId,
        OcctObjectId subshapeId,
        OcctModelTopologyReference* result);

    OCCTBRIDGE_API OcctStatus occt_model_resolve_topology_reference(
        OcctModelingSessionHandle handle,
        OcctObjectId rootShapeId,
        const OcctModelTopologyReference* reference,
        double matchingTolerance,
        OcctModelTopologyReferenceResult* result);

    OCCTBRIDGE_API OcctStatus occt_model_resolve_topology_reference_with_history(
        OcctModelingSessionHandle handle,
        OcctObjectId rootShapeId,
        OcctOperationId operationId,
        OcctObjectId sourceShapeId,
        const OcctModelTopologyReference* reference,
        double matchingTolerance,
        OcctModelTopologyReferenceResult* result);
}

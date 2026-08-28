#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_subshapes_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        int shapeType,
        OcctObjectId* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_outer_wire_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_inner_wires_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctObjectId* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_wire_edges_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId wireId,
        OcctObjectId* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_ancestors_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId rootId,
        OcctObjectId childId,
        int ancestorType,
        OcctObjectId* results,
        int capacity,
        int* required);
}

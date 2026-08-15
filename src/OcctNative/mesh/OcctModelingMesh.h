#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_mesh(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const OcctModelMeshParameters* parameters);

    OCCTBRIDGE_API OcctStatus occt_model_clear_mesh(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId);

    OCCTBRIDGE_API OcctStatus occt_model_face_mesh_nodes_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelMeshNode* results,
        int capacity,
        int* required);

    OCCTBRIDGE_API OcctStatus occt_model_face_mesh_triangles_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctModelMeshTriangle* results,
        int capacity,
        int* required);
}

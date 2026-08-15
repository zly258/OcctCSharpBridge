#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    OCCTBRIDGE_API int occt_model_mesh(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        const OcctModelMeshParameters* parameters);

    OCCTBRIDGE_API int occt_model_clear_mesh(OcctModelHandle handle, OcctObjectId shapeId);

    OCCTBRIDGE_API int occt_model_face_mesh_nodes_copy(
        OcctModelHandle handle,
        OcctObjectId faceId,
        OcctModelMeshNode* results,
        int capacity);

    OCCTBRIDGE_API int occt_model_face_mesh_triangles_copy(
        OcctModelHandle handle,
        OcctObjectId faceId,
        OcctModelMeshTriangle* results,
        int capacity);
}

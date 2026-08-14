#pragma once

#include "modeling/OcctModeling.h"

#include <cstdint>

extern "C"
{
    struct OcctMeshBuildOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        double linearDeflection;
        double angularDeflection;
        double minSize;
        OcctBool relative;
        OcctBool parallel;
        OcctBool internalVertices;
        OcctBool controlSurfaceDeflection;
    };

    // Builds an owned mesh snapshot independent from the source session registry.
    OCCTBRIDGE_API OcctStatus occt_model_mesh_create(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const OcctMeshBuildOptions* options,
        OcctMeshHandle* result);

    OCCTBRIDGE_API void occt_mesh_release(OcctMeshHandle handle);

    OCCTBRIDGE_API OcctStatus occt_mesh_get_counts(
        OcctMeshHandle handle,
        int* nodeCount,
        int* triangleCount);

    OCCTBRIDGE_API OcctStatus occt_mesh_nodes_copy(
        OcctMeshHandle handle,
        OcctModelMeshNode* results,
        int capacity,
        int* written);

    OCCTBRIDGE_API OcctStatus occt_mesh_triangles_copy(
        OcctMeshHandle handle,
        OcctModelMeshTriangle* results,
        int capacity,
        int* written);
}

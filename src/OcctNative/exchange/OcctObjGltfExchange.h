#pragma once
#include "OcctNative.h"
#include "OcctModelingExchange.h"
#include <cstdint>

extern "C" {
    struct OcctGltfExportOptions {
        uint32_t structSize;
        uint32_t apiVersion;
        OcctBool writeBinary;       // 1 = .glb, 0 = .gltf
        OcctBool transformToGltfCs; // 1 = Y-up
        double deflection;          // mesh deflection, <= 0 means auto (0.01)
    };

    // OBJ import
    OCCTBRIDGE_API OcctStatus occt_model_obj_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId);

    // OBJ export
    OCCTBRIDGE_API OcctStatus occt_model_obj_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path);

    // glTF import
    OCCTBRIDGE_API OcctStatus occt_model_gltf_import(
        OcctModelingSessionHandle session,
        const char* utf8Path,
        OcctObjectId* resultShapeId);

    // glTF export (supports .gltf and .glb)
    OCCTBRIDGE_API OcctStatus occt_model_gltf_export(
        OcctModelingSessionHandle session,
        OcctObjectId shapeId,
        const char* utf8Path,
        const OcctGltfExportOptions* options);

    // Batch STL export (multiple shapes into one file)
    OCCTBRIDGE_API OcctStatus occt_model_stl_export_multiple(
        OcctModelingSessionHandle session,
        const OcctObjectId* shapeIds,
        int count,
        const char* utf8Path,
        const OcctStlExportOptions* options);
}

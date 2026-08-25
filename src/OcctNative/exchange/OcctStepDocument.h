#pragma once

#include "OcctNative.h"

extern "C"
{
    struct OcctStepTransform3d
    {
        double m00; double m01; double m02; double m03;
        double m10; double m11; double m12; double m13;
        double m20; double m21; double m22; double m23;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_step_node_name_set(
        OcctEngineHandle handle,
        const char* nodeId,
        const char* utf8Name);

    OCCTBRIDGE_API OcctStatus occt_engine_step_node_visibility_set(
        OcctEngineHandle handle,
        const char* nodeId,
        OcctBool visible);

    OCCTBRIDGE_API OcctStatus occt_engine_step_node_transform_set(
        OcctEngineHandle handle,
        const char* nodeId,
        const OcctStepTransform3d* transform);

    OCCTBRIDGE_API OcctStatus occt_engine_step_component_add(
        OcctEngineHandle handle,
        const char* parentNodeId,
        const char* referenceNodeId,
        const OcctStepTransform3d* transform,
        OcctObjectId* viewerObjectId);

    OCCTBRIDGE_API OcctStatus occt_engine_step_component_remove(
        OcctEngineHandle handle,
        const char* componentNodeId);

    // Returns a UTF-8 JSON snapshot of the most recently imported STEP/XDE document.
    // Call once with buffer=null/capacity=0 to query requiredBytes, then call again
    // with a buffer of at least requiredBytes bytes including the null terminator.
    OCCTBRIDGE_API OcctStatus occt_engine_step_document_json_get(
        OcctEngineHandle handle,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes);
}

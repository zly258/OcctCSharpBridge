#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewerObjectUpdateMask : std::uint32_t
    {
        OcctViewerObjectUpdate_Name = 1u << 0,
        OcctViewerObjectUpdate_ApplicationTag = 1u << 1,
        OcctViewerObjectUpdate_Color = 1u << 2,
        OcctViewerObjectUpdate_Transparency = 1u << 3,
        OcctViewerObjectUpdate_Visibility = 1u << 4,
        OcctViewerObjectUpdate_LineWidth = 1u << 5,
        OcctViewerObjectUpdate_Material = 1u << 6,
        OcctViewerObjectUpdate_Selectable = 1u << 7
    };

    struct OcctViewerObjectUpdateOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        const char* name;
        const char* applicationTag;
        OcctColorRgb color;
        double transparency;
        int visible;
        double lineWidth;
        int material;
        int selectable;
    };

    enum OcctViewerObjectPresentationAction
    {
        OcctViewerObjectPresentation_Redisplay = 0,
        OcctViewerObjectPresentation_Highlight = 1,
        OcctViewerObjectPresentation_Unhighlight = 2
    };

    OCCTBRIDGE_API OcctStatus occt_engine_objects_snapshot_get(
        OcctEngineHandle handle,
        OcctObjectDescriptor* items,
        int capacity,
        int* objectCount,
        int* shapeCount);

    OCCTBRIDGE_API OcctStatus occt_engine_object_exists(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int* exists);

    OCCTBRIDGE_API OcctStatus occt_engine_object_kind_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int* kind);

    OCCTBRIDGE_API OcctStatus occt_engine_object_update(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerObjectUpdateOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_object_name_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes);

    OCCTBRIDGE_API OcctStatus occt_engine_object_application_tag_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes);

    OCCTBRIDGE_API OcctStatus occt_engine_object_find_by_application_tag(
        OcctEngineHandle handle,
        const char* utf8Tag,
        OcctObjectId* objectId,
        int* found);

    OCCTBRIDGE_API OcctStatus occt_engine_objects_delete(
        OcctEngineHandle handle,
        const OcctObjectId* objectIds,
        int count);

    OCCTBRIDGE_API OcctStatus occt_engine_objects_clear(OcctEngineHandle handle);

    OCCTBRIDGE_API OcctStatus occt_engine_objects_visibility_all_set(
        OcctEngineHandle handle,
        int visible);

    OCCTBRIDGE_API OcctStatus occt_engine_object_presentation_action(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int action);
}

#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctSelectionModeConcurrency
    {
        OcctSelectionConcurrency_Single = 0,
        OcctSelectionConcurrency_GlobalOrLocal = 1,
        OcctSelectionConcurrency_Multiple = 2
    };

    enum OcctTransformPersistenceMode
    {
        OcctTransformPersistence_None = 0,
        OcctTransformPersistence_Zoom = 1,
        OcctTransformPersistence_Rotate = 2,
        OcctTransformPersistence_ZoomRotate = 3,
        OcctTransformPersistence_Screen2d = 4,
        OcctTransformPersistence_Triedron = 5
    };

    struct OcctTransformPersistenceState
    {
        int mode;
        OcctPoint3d anchor;
        int position;
        int offsetX;
        int offsetY;
    };

    struct OcctViewClipPlane
    {
        OcctPoint3d point;
        OcctVector3d normal;
        int enabled;
        int capping;
        double cappingR;
        double cappingG;
        double cappingB;
    };

    struct OcctPointStateUpdate
    {
        OcctObjectId pointId;
        OcctPoint3d position;
        int visible;
    };

    OCCTBRIDGE_API int occt_set_object_selection_mode_active(
        OcctHandle handle,
        OcctObjectId objectId,
        int mode,
        int active,
        int concurrency,
        int force);

    OCCTBRIDGE_API int occt_set_object_selection_sensitivity(
        OcctHandle handle,
        OcctObjectId objectId,
        int mode,
        int sensitivity);

    OCCTBRIDGE_API int occt_set_object_display_priority(
        OcctHandle handle,
        OcctObjectId objectId,
        int priority);

    OCCTBRIDGE_API int occt_set_objects_display_priority(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int priority);

    OCCTBRIDGE_API int occt_get_object_display_priority(
        OcctHandle handle,
        OcctObjectId objectId,
        int* priority);

    OCCTBRIDGE_API int occt_set_object_transform_persistence_3d(
        OcctHandle handle,
        OcctObjectId objectId,
        int mode,
        OcctPoint3d anchor);

    OCCTBRIDGE_API int occt_set_object_transform_persistence_2d(
        OcctHandle handle,
        OcctObjectId objectId,
        int mode,
        int position,
        int offsetX,
        int offsetY);

    OCCTBRIDGE_API int occt_clear_object_transform_persistence(
        OcctHandle handle,
        OcctObjectId objectId);

    OCCTBRIDGE_API int occt_get_object_transform_persistence(
        OcctHandle handle,
        OcctObjectId objectId,
        OcctTransformPersistenceState* result);

    OCCTBRIDGE_API int occt_set_view_clip_planes(
        OcctHandle handle,
        const OcctViewClipPlane* planes,
        int count);

    OCCTBRIDGE_API int occt_get_view_clip_plane_limit(
        OcctHandle handle,
        int* limit);

    OCCTBRIDGE_API int occt_update_points(
        OcctHandle handle,
        const OcctPointStateUpdate* updates,
        int count);
}

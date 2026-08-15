#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewerSelectionOperation
    {
        OcctViewerSelection_Replace = 0,
        OcctViewerSelection_Add = 1,
        OcctViewerSelection_Remove = 2,
        OcctViewerSelection_Toggle = 3,
        OcctViewerSelection_Clear = 4
    };

    enum OcctSelectionModeConcurrency
    {
        OcctSelectionConcurrency_Single = 0,
        OcctSelectionConcurrency_GlobalOrLocal = 1,
        OcctSelectionConcurrency_Multiple = 2
    };

    enum OcctViewerSelectionSettingsUpdateMask : std::uint32_t
    {
        OcctViewerSelectionSettingsUpdate_Mode = 1u << 0,
        OcctViewerSelectionSettingsUpdate_Tolerance = 1u << 1
    };

    struct OcctViewerSelectionSettingsOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        int selectionMode;
        int pixelTolerance;
    };

    struct OcctViewerRectangleSelectionOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int x1;
        int y1;
        int x2;
        int y2;
        int append;
        int allowOverlap;
    };

    struct OcctViewerObjectSelectionOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        const OcctObjectId* objectIds;
        int count;
        int operation;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_selection_settings_update(
        OcctEngineHandle handle,
        const OcctViewerSelectionSettingsOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_move_to(
        OcctEngineHandle handle,
        int x,
        int y);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_point_select(
        OcctEngineHandle handle,
        int x,
        int y,
        int append);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_rectangle_select(
        OcctEngineHandle handle,
        const OcctViewerRectangleSelectionOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_object_select(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int append);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_objects_update(
        OcctEngineHandle handle,
        const OcctViewerObjectSelectionOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_clear(OcctEngineHandle handle);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_subshape_copy(
        OcctEngineHandle handle,
        int index,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_all_visible(OcctEngineHandle handle);
    OCCTBRIDGE_API OcctStatus occt_engine_selection_invert(OcctEngineHandle handle);
    OCCTBRIDGE_API OcctStatus occt_engine_selection_hide_selected(OcctEngineHandle handle);
    OCCTBRIDGE_API OcctStatus occt_engine_selection_automatic_highlight_set(
        OcctEngineHandle handle,
        int enabled);

    // Frozen ABI4 compatibility. Implemented by the selection domain.
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
}

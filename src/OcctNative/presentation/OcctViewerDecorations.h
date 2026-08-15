#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewerZLayer
    {
        OcctViewerZLayer_Bottom = 0,
        OcctViewerZLayer_Default = 1,
        OcctViewerZLayer_Top = 2,
        OcctViewerZLayer_Topmost = 3
    };

    enum OcctCornerPosition
    {
        OcctCorner_LeftLower = 0,
        OcctCorner_LeftUpper = 1,
        OcctCorner_RightLower = 2,
        OcctCorner_RightUpper = 3
    };

    struct OcctViewerTriedronOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int visible;
        int position;
        double scale;
        OcctColorRgb color;
    };

    struct OcctViewerViewCubeOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int visible;
        int position;
        int sizePixels;
        int offsetX;
        int offsetY;
    };

    struct OcctViewerFaceBoundaryOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int visible;
        OcctColorRgb color;
        double width;
        int setDefault;
        int applyExisting;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_objects_z_layer_set(
        OcctEngineHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int layer);

    OCCTBRIDGE_API OcctStatus occt_engine_object_z_layer_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        int* layer);

    OCCTBRIDGE_API OcctStatus occt_engine_triedron_update(
        OcctEngineHandle handle,
        const OcctViewerTriedronOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_view_cube_update(
        OcctEngineHandle handle,
        const OcctViewerViewCubeOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_face_boundary_update(
        OcctEngineHandle handle,
        const OcctObjectId* shapeIds,
        int count,
        const OcctViewerFaceBoundaryOptions* options);
}

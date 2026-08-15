#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewerDepthUpdateMask : std::uint32_t
    {
        OcctViewerDepthUpdate_AutoZFitSettings = 1u << 0,
        OcctViewerDepthUpdate_AutoZFitNow = 1u << 1,
        OcctViewerDepthUpdate_DefaultPolygonOffsets = 1u << 2
    };

    struct OcctViewerDepthUpdateOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        int autoZFitEnabled;
        double autoZFitScaleFactor;
        int polygonOffsetMode;
        double polygonOffsetFactor;
        double polygonOffsetUnits;
        int applyPolygonOffsetsToExisting;
    };

    struct OcctViewerDepthState
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int autoZFitEnabled;
        double autoZFitScaleFactor;
        int polygonOffsetMode;
        double polygonOffsetFactor;
        double polygonOffsetUnits;
    };

    struct OcctViewerObjectPolygonOffsetOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int resetToDefault;
        int mode;
        double factor;
        double units;
    };

    struct OcctViewerObjectPolygonOffsetState
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int mode;
        double factor;
        double units;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_depth_update(
        OcctEngineHandle handle,
        const OcctViewerDepthUpdateOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_depth_state_get(
        OcctEngineHandle handle,
        OcctViewerDepthState* state);

    OCCTBRIDGE_API OcctStatus occt_engine_object_polygon_offset_update(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctViewerObjectPolygonOffsetOptions* options);

    OCCTBRIDGE_API OcctStatus occt_engine_object_polygon_offset_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        OcctViewerObjectPolygonOffsetState* state);
}

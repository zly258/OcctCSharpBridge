#pragma once

#include "OcctNative.h"

extern "C"
{
    enum OcctViewportResetMask : std::uint32_t
    {
        OcctViewportReset_All = 1u << 0,
        OcctViewportReset_Orientation = 1u << 1,
        OcctViewportReset_Mapping = 1u << 2
    };

    enum OcctViewportRenderingUpdateMask : std::uint32_t
    {
        OcctViewportRenderingUpdate_MsaaSamples = 1u << 0,
        OcctViewportRenderingUpdate_ResolutionScale = 1u << 1,
        OcctViewportRenderingUpdate_ResolutionDpi = 1u << 2,
        OcctViewportRenderingUpdate_Method = 1u << 3,
        OcctViewportRenderingUpdate_Shadows = 1u << 4,
        OcctViewportRenderingUpdate_ImmediateUpdate = 1u << 5,
        OcctViewportRenderingUpdate_FrustumCulling = 1u << 6,
        OcctViewportRenderingUpdate_FaceBoundaries = 1u << 7
    };

    struct OcctViewportRenderingOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        int msaaSamples;
        double resolutionScale;
        double resolutionDpi;
        int renderingMethod;
        int shadowsEnabled;
        int immediateUpdate;
        int frustumCullingEnabled;
        int faceBoundariesVisible;
        int applyFaceBoundariesToExisting;
    };

    struct OcctViewportStateResult
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        OcctViewportState state;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_viewport_fit_objects(
        OcctEngineHandle handle,
        const OcctObjectId* objectIds,
        int count,
        double margin);

    OCCTBRIDGE_API OcctStatus occt_engine_viewport_fit_selected(
        OcctEngineHandle handle,
        double margin);

    OCCTBRIDGE_API OcctStatus occt_engine_viewport_zup_set(
        OcctEngineHandle handle,
        int orientation,
        int fitAll);

    OCCTBRIDGE_API OcctStatus occt_engine_viewport_reset(
        OcctEngineHandle handle,
        std::uint32_t resetMask);

    OCCTBRIDGE_API OcctStatus occt_engine_viewport_state_get(
        OcctEngineHandle handle,
        OcctViewportStateResult* result);

    OCCTBRIDGE_API OcctStatus occt_engine_viewport_screen_to_ray(
        OcctEngineHandle handle,
        int x,
        int y,
        OcctProjectionRay* result);

    OCCTBRIDGE_API OcctStatus occt_engine_viewport_gravity_point_get(
        OcctEngineHandle handle,
        OcctPoint3d* result);

    OCCTBRIDGE_API OcctStatus occt_engine_viewport_zoom_at_point(
        OcctEngineHandle handle,
        int x,
        int y,
        double delta);

    OCCTBRIDGE_API OcctStatus occt_engine_viewport_rendering_update(
        OcctEngineHandle handle,
        const OcctViewportRenderingOptions* options);
}

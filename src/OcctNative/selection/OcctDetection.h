#pragma once

#include "selection/OcctSelectionState.h"

#include <cstdint>

extern "C"
{
    struct OcctViewerDetectionOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        int x;
        int y;
        int maxHits;
        const OcctObjectId* ownerIds;
        int ownerCount;
        std::uint64_t objectKindMask;
        std::uint64_t shapeTypeMask;
        int includeWholeObjects;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_selection_detect_filtered(
        OcctEngineHandle handle,
        const OcctViewerDetectionOptions* options,
        OcctSelectionHitDetail* items,
        int capacity,
        int* count);
}

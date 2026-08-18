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

    // Query-only region picking. Unlike rectangle selection, this API never mutates the
    // AIS selection set or highlight state. Pass objectIds=null/capacity=0 to query count.
    OCCTBRIDGE_API OcctStatus occt_engine_selection_rectangle_query(
        OcctEngineHandle handle,
        int x1,
        int y1,
        int x2,
        int y2,
        int allowOverlap,
        OcctObjectId* objectIds,
        int capacity,
        int* count);
}

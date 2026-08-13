#pragma once

#include "OcctSelectionState.h"

#include <cstdint>

extern "C"
{
    OCCTBRIDGE_API int occt_detect_at_filtered(
        OcctHandle handle,
        int x,
        int y,
        int maxHits,
        const OcctObjectId* ownerIds,
        int ownerCount,
        std::uint64_t objectKindMask,
        std::uint64_t shapeTypeMask,
        int includeWholeObjects,
        OcctSelectionHitDetail* items,
        int capacity,
        int* count);
}

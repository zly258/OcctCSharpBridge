#pragma once

#include "OcctNative.h"

extern "C"
{
    struct OcctSelectionHit
    {
        OcctObjectId ownerObjectId;
        int subshapeType;
        int subshapeIndex;
    };

    struct OcctSelectionHitDetail
    {
        OcctObjectId ownerObjectId;
        int subshapeType;
        int subshapeIndex;
        OcctPoint3d point;
        double depth;
        double distanceToEye;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_selection_hits_get(
        OcctEngineHandle handle,
        OcctSelectionHit* items,
        int capacity,
        int* count);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_detected_hit_get(
        OcctEngineHandle handle,
        OcctSelectionHit* result,
        int* hasHit);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_detected_hit_detail_get(
        OcctEngineHandle handle,
        OcctSelectionHitDetail* result,
        int* hasHit);

    OCCTBRIDGE_API OcctStatus occt_engine_selection_detect_at(
        OcctEngineHandle handle,
        int x,
        int y,
        int maxHits,
        OcctSelectionHitDetail* items,
        int capacity,
        int* count);
}

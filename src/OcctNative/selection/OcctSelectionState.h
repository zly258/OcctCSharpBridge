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

    // Returns the current registered AIS selection as structured object/subshape identities.
    // Call with items=nullptr/capacity=0 to query count, then call again with a large enough buffer.
    // subshapeIndex follows the same TopExp_Explorer ordering as occt_get_subshape;
    // whole-object selection uses OcctShape_Shape and index -1.
    OCCTBRIDGE_API int occt_selected_hits(
        OcctHandle handle,
        OcctSelectionHit* items,
        int capacity,
        int* count);

    // Returns success/failure through the normal bridge error contract and reports whether
    // a registered object is currently detected through hasHit.
    OCCTBRIDGE_API int occt_detected_hit(
        OcctHandle handle,
        OcctSelectionHit* result,
        int* hasHit);

    OCCTBRIDGE_API int occt_detected_hit_detail(
        OcctHandle handle,
        OcctSelectionHitDetail* result,
        int* hasHit);

    // Performs native point detection and returns up to maxHits sorted AIS owners with
    // world pick point and depth data. This does not create registry/viewer subshape objects.
    OCCTBRIDGE_API int occt_detect_at(
        OcctHandle handle,
        int x,
        int y,
        int maxHits,
        OcctSelectionHitDetail* items,
        int capacity,
        int* count);
}

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
}

#pragma once

#include "OcctNative.h"

extern "C"
{
    // Returns a UTF-8 JSON snapshot of the most recently imported STEP/XDE document.
    // Call once with buffer=null/capacity=0 to query requiredBytes, then call again
    // with a buffer of at least requiredBytes bytes including the null terminator.
    OCCTBRIDGE_API OcctStatus occt_engine_step_document_json_get(
        OcctEngineHandle handle,
        char* utf8Buffer,
        int capacity,
        int* requiredBytes);
}

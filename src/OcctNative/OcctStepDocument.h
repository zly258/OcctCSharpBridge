#pragma once

#include "OcctNative.h"

extern "C"
{
    // Return a UTF-8 JSON snapshot of the most recently imported STEP/XDE document.
    // The pointer remains valid until the engine scratch buffer is reused.
    OCCTBRIDGE_API const char* occt_get_last_step_document_json(OcctHandle handle);
}

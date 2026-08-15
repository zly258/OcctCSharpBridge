#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_model_assembly_compound_create(
        OcctModelingSessionHandle handle,
        const OcctObjectId* shapeIds,
        int count,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_assembly_wire_create(
        OcctModelingSessionHandle handle,
        const OcctObjectId* edgeIds,
        int count,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_assembly_sew(
        OcctModelingSessionHandle handle,
        const OcctObjectId* shapeIds,
        int count,
        double tolerance,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_assembly_solid_from_shell_create(
        OcctModelingSessionHandle handle,
        OcctObjectId shellId,
        OcctObjectId* result);
}

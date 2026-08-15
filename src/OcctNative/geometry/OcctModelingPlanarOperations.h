#pragma once

#include "modeling/OcctModeling.h"

extern "C"
{
    enum OcctModelJoinType
    {
        OcctModelJoin_Arc = 0,
        OcctModelJoin_Tangent = 1,
        OcctModelJoin_Intersection = 2
    };

    OCCTBRIDGE_API OcctStatus occt_model_make_face_with_holes(
        OcctModelingSessionHandle handle,
        OcctObjectId outerWireId,
        const OcctObjectId* innerWireIds,
        int innerWireCount,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_trim_edge(
        OcctModelingSessionHandle handle,
        OcctObjectId edgeId,
        double firstParameter,
        double lastParameter,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_model_offset_wire(
        OcctModelingSessionHandle handle,
        OcctObjectId wireId,
        double offset,
        double altitude,
        int joinType,
        OcctBool openResult,
        OcctObjectId* result);
}

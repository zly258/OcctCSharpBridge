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

    OCCTBRIDGE_API OcctObjectId occt_model_make_face_with_holes(
        OcctModelHandle handle,
        OcctObjectId outerWireId,
        const OcctObjectId* innerWireIds,
        int innerWireCount);

    OCCTBRIDGE_API OcctObjectId occt_model_trim_edge(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        double firstParameter,
        double lastParameter);

    OCCTBRIDGE_API OcctObjectId occt_model_offset_wire(
        OcctModelHandle handle,
        OcctObjectId wireId,
        double offset,
        double altitude,
        int joinType,
        int openResult);
}

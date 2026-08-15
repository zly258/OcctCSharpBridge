#pragma once

#include "OcctNative.h"

extern "C"
{
    OCCTBRIDGE_API OcctStatus occt_engine_shape_boolean(
        OcctEngineHandle handle,
        int operation,
        OcctObjectId leftId,
        OcctObjectId rightId,
        OcctBool hideInputs,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_extrude(
        OcctEngineHandle handle,
        OcctObjectId profileId,
        OcctVector3d value,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_revolve(
        OcctEngineHandle handle,
        OcctObjectId profileId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_sweep(
        OcctEngineHandle handle,
        OcctObjectId spineWireId,
        OcctObjectId profileId,
        OcctBool hideInputs,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_loft(
        OcctEngineHandle handle,
        const OcctObjectId* wireIds,
        int count,
        OcctBool makeSolid,
        OcctBool ruled,
        double tolerance,
        OcctBool hideInputs,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_fillet_all_edges(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        double radius,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_fillet_edges(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        const int* edgeIndices,
        int count,
        double radius,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_chamfer_all_edges(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        double distance,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_chamfer_edges(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        const int* edgeIndices,
        int count,
        double distance,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_offset(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        double offset,
        double tolerance,
        OcctBool hideInput,
        OcctObjectId* result);

    OCCTBRIDGE_API OcctStatus occt_engine_shape_thick_solid(
        OcctEngineHandle handle,
        OcctObjectId solidId,
        int faceIndexToRemove,
        double thickness,
        double tolerance,
        OcctBool hideInput,
        OcctObjectId* result);
}

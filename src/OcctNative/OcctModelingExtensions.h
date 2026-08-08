#pragma once

#include "OcctModeling.h"

extern "C"
{
    enum OcctModelJoinType
    {
        OcctModelJoin_Arc = 0,
        OcctModelJoin_Tangent = 1,
        OcctModelJoin_Intersection = 2
    };

    struct OcctOrientedBounds
    {
        OcctPoint3d center;
        OcctVector3d xDirection;
        OcctVector3d yDirection;
        OcctVector3d zDirection;
        double halfSizeX;
        double halfSizeY;
        double halfSizeZ;
    };

    struct OcctModelBSplineCurveInfo
    {
        int degree;
        int poleCount;
        int knotCount;
        int rational;
        int periodic;
    };

    OCCTBRIDGE_API int occt_model_shape_is_same(
        OcctModelHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId);

    OCCTBRIDGE_API int occt_model_shape_is_partner(
        OcctModelHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId);

    OCCTBRIDGE_API int occt_model_shape_oriented_bounds(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        int optimal,
        OcctOrientedBounds* result);

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

    OCCTBRIDGE_API int occt_model_edge_bspline_info(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        OcctModelBSplineCurveInfo* result);

    OCCTBRIDGE_API int occt_model_edge_bspline_pole_at(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        int index,
        OcctPoint3d* pole,
        double* weight);

    OCCTBRIDGE_API int occt_model_edge_bspline_knot_at(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        int index,
        double* knot,
        int* multiplicity);
}

#pragma once

#include "OcctModeling.h"

extern "C"
{
    struct OcctModelFaceAnalysis
    {
        OcctObjectId faceId;
        int surfaceType;
        int orientation;
        int edgeCount;
        int wireCount;
        double area;
        double maximumTolerance;
        OcctUvBounds uvBounds;
        OcctBounds bounds;
    };

    OCCTBRIDGE_API int occt_model_shape_face_analysis(
        OcctModelHandle handle,
        OcctObjectId shapeId,
        OcctModelFaceAnalysis* items,
        int capacity,
        int* count);
}

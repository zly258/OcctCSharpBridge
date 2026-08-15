#pragma once

#include "modeling/OcctModeling.h"

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

    OCCTBRIDGE_API OcctStatus occt_model_shape_face_analysis_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctModelFaceAnalysis* items,
        int capacity,
        int* required);
}

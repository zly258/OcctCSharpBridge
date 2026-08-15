#pragma once

#include "OcctNative.h"

#include <cstdint>

extern "C"
{
    enum OcctTextHorizontalAlignment
    {
        OcctTextHorizontal_Left = 0,
        OcctTextHorizontal_Center = 1,
        OcctTextHorizontal_Right = 2
    };

    enum OcctTextVerticalAlignment
    {
        OcctTextVertical_Bottom = 0,
        OcctTextVertical_Center = 1,
        OcctTextVertical_Top = 2
    };

    struct OcctBRepTextOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        OcctPoint3d position;
        OcctVector3d normal;
        OcctVector3d xDirection;
        double height;
        double extrusionDepth;
        OcctBool bold;
        OcctBool italic;
        int horizontalAlignment;
        int verticalAlignment;
    };

    struct OcctBRepAnnotationOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        double offset;
        double textHeight;
        double arrowSize;
    };

    OCCTBRIDGE_API OcctStatus occt_model_brep_text_create(
        OcctModelingSessionHandle session,
        const char* utf8Text,
        const char* fontName,
        const OcctBRepTextOptions* options,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_length_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId edgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_angle_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_radius_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId circularEdgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId);

    OCCTBRIDGE_API OcctStatus occt_model_diameter_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId circularEdgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId);
}

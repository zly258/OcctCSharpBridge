#pragma once

#include "OcctNative.h"

#include <cstdint>

extern "C"
{
    enum OcctViewerTextUpdateMask : std::uint32_t
    {
        OcctViewerTextUpdate_None = 0,
        OcctViewerTextUpdate_Content = 1u << 0,
        OcctViewerTextUpdate_Position = 1u << 1,
        OcctViewerTextUpdate_Height = 1u << 2,
        OcctViewerTextUpdate_Font = 1u << 3,
        OcctViewerTextUpdate_Angle = 1u << 4,
        OcctViewerTextUpdate_Zoomable = 1u << 5,
        OcctViewerTextUpdate_Color = 1u << 6
    };

    struct OcctViewerTextOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        OcctPoint3d position;
        double height;
        double angleDegrees;
        double red;
        double green;
        double blue;
        OcctBool zoomable;
    };

    enum OcctViewerDimensionKind
    {
        OcctViewerDimension_Length = 0,
        OcctViewerDimension_Angle = 1,
        OcctViewerDimension_Radius = 2,
        OcctViewerDimension_Diameter = 3
    };

    enum OcctViewerDimensionUpdateMask : std::uint32_t
    {
        OcctViewerDimensionUpdate_None = 0,
        OcctViewerDimensionUpdate_Flyout = 1u << 0,
        OcctViewerDimensionUpdate_Color = 1u << 1
    };

    struct OcctViewerDimensionOptions
    {
        std::uint32_t structSize;
        std::uint32_t apiVersion;
        std::uint32_t updateMask;
        double flyout;
        double red;
        double green;
        double blue;
    };

    OCCTBRIDGE_API OcctStatus occt_engine_text_create(
        OcctEngineHandle engine,
        const char* utf8Text,
        const char* fontName,
        const OcctViewerTextOptions* options,
        OcctObjectId* resultTextId);

    OCCTBRIDGE_API OcctStatus occt_engine_text_update(
        OcctEngineHandle engine,
        OcctObjectId textId,
        const char* utf8Text,
        const char* fontName,
        const OcctViewerTextOptions* options);

    // Additive ABI5 text presentation controls. These stay outside OcctViewerTextOptions so
    // callers compiled against the original struct remain binary compatible.
    OCCTBRIDGE_API OcctStatus occt_engine_text_set_justification(
        OcctEngineHandle engine,
        OcctObjectId textId,
        int horizontalAlignment,
        int verticalAlignment);

    OCCTBRIDGE_API OcctStatus occt_engine_text_set_orientation(
        OcctEngineHandle engine,
        OcctObjectId textId,
        OcctVector3d planeNormal,
        OcctVector3d xDirection,
        OcctBool enabled);

    OCCTBRIDGE_API OcctStatus occt_engine_text_set_wrapping(
        OcctEngineHandle engine,
        OcctObjectId textId,
        double width,
        OcctBool wordWrapping);

    OCCTBRIDGE_API OcctStatus occt_engine_text_set_background(
        OcctEngineHandle engine,
        OcctObjectId textId,
        OcctBool enabled,
        double red,
        double green,
        double blue);

    OCCTBRIDGE_API OcctStatus occt_engine_dimension_create(
        OcctEngineHandle engine,
        int kind,
        OcctObjectId firstShapeId,
        OcctObjectId secondShapeId,
        const OcctViewerDimensionOptions* options,
        OcctObjectId* resultDimensionId);

    // Additive ABI5 APIs for hosts that own the drafting plane. The legacy generic dimension
    // entry point remains unchanged for compatibility and may continue to infer a plane.
    OCCTBRIDGE_API OcctStatus occt_engine_length_dimension_create_in_plane(
        OcctEngineHandle engine,
        OcctObjectId edgeShapeId,
        OcctVector3d planeNormal,
        const OcctViewerDimensionOptions* options,
        OcctObjectId* resultDimensionId);

    OCCTBRIDGE_API OcctStatus occt_engine_angle_dimension_create_in_plane(
        OcctEngineHandle engine,
        OcctObjectId firstEdgeShapeId,
        OcctObjectId secondEdgeShapeId,
        OcctVector3d planeNormal,
        const OcctViewerDimensionOptions* options,
        OcctObjectId* resultDimensionId);

    OCCTBRIDGE_API OcctStatus occt_engine_dimension_update(
        OcctEngineHandle engine,
        OcctObjectId dimensionId,
        const OcctViewerDimensionOptions* options);

    // Additive style setter kept outside OcctViewerDimensionOptions so ABI5 callers compiled
    // against the original options struct remain binary compatible.
    OCCTBRIDGE_API OcctStatus occt_engine_dimension_set_text_height(
        OcctEngineHandle engine,
        OcctObjectId dimensionId,
        double textHeight);
}

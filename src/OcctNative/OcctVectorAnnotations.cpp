#include "core/OcctInternal.hxx"
#include "geometry/OcctBRepAnnotationBuilder.hxx"
#include "geometry/OcctBRepTextBuilder.hxx"

#include <Graphic3d_HorizontalTextAlignment.hxx>
#include <Graphic3d_VerticalTextAlignment.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>

#include <stdexcept>
#include <string>

using namespace OcctBridge;
using namespace OcctModelingInternal;

namespace
{
    TopoDS_Edge requireEdge(Engine* engine, OcctObjectId id, const char* name)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr || entry->shape.ShapeType() != TopAbs_EDGE)
            throw std::invalid_argument(std::string(name) + " must be an edge.");
        return TopoDS::Edge(entry->shape);
    }
}

extern "C"
{
    OcctObjectId occt_make_text_shape(
        OcctHandle h,
        const char* text,
        OcctPoint3d position,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double height,
        double extrusionDepth,
        const char* fontName,
        int bold,
        int italic)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;
        return executeObject(engine, [&]
        {
            return engine->addShape(
                buildBRepText(
                    text,
                    fontName,
                    position,
                    normal,
                    xDirection,
                    height,
                    extrusionDepth,
                    bold != 0,
                    italic != 0,
                    Graphic3d_HTA_LEFT,
                    Graphic3d_VTA_BOTTOM),
                false,
                "BRepText");
        });
    }

    OcctObjectId occt_make_length_annotation_shape(
        OcctHandle h,
        OcctObjectId edgeId,
        double flyout,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;
        return executeObject(engine, [&]
        {
            return engine->addShape(
                buildLengthAnnotation(
                    requireEdge(engine, edgeId, "Length annotation input"),
                    flyout,
                    textHeight,
                    arrowSize,
                    fontName),
                false,
                "VectorLengthDimension");
        });
    }

    OcctObjectId occt_make_angle_annotation_shape(
        OcctHandle h,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        double radius,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;
        return executeObject(engine, [&]
        {
            return engine->addShape(
                buildAngleAnnotation(
                    requireEdge(engine, firstEdgeId, "First angular annotation input"),
                    requireEdge(engine, secondEdgeId, "Second angular annotation input"),
                    radius,
                    textHeight,
                    arrowSize,
                    fontName),
                false,
                "VectorAngleDimension");
        });
    }

    OcctObjectId occt_make_radius_annotation_shape(
        OcctHandle h,
        OcctObjectId circularEdgeId,
        double flyout,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;
        return executeObject(engine, [&]
        {
            return engine->addShape(
                buildRadiusAnnotation(
                    requireEdge(engine, circularEdgeId, "Radius annotation input"),
                    flyout,
                    textHeight,
                    arrowSize,
                    fontName),
                false,
                "VectorRadiusDimension");
        });
    }

    OcctObjectId occt_make_diameter_annotation_shape(
        OcctHandle h,
        OcctObjectId circularEdgeId,
        double flyout,
        double textHeight,
        double arrowSize,
        const char* fontName)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;
        return executeObject(engine, [&]
        {
            return engine->addShape(
                buildDiameterAnnotation(
                    requireEdge(engine, circularEdgeId, "Diameter annotation input"),
                    flyout,
                    textHeight,
                    arrowSize,
                    fontName),
                false,
                "VectorDiameterDimension");
        });
    }
}

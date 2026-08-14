#include "modeling/OcctModelingAnnotations.h"
#include "geometry/OcctBRepAnnotationBuilder.hxx"
#include "geometry/OcctBRepTextBuilder.hxx"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <Graphic3d_HorizontalTextAlignment.hxx>
#include <Graphic3d_VerticalTextAlignment.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>

#include <cmath>
#include <stdexcept>
#include <string>

using namespace OcctModelingInternal;

namespace
{
    constexpr std::uint32_t TextOptionsApiVersion = 1;
    constexpr std::uint32_t AnnotationOptionsApiVersion = 1;

    Graphic3d_HorizontalTextAlignment horizontalAlignment(int value)
    {
        switch (value)
        {
        case OcctTextHorizontal_Left: return Graphic3d_HTA_LEFT;
        case OcctTextHorizontal_Center: return Graphic3d_HTA_CENTER;
        case OcctTextHorizontal_Right: return Graphic3d_HTA_RIGHT;
        default: throw std::invalid_argument("Unsupported horizontal text alignment.");
        }
    }

    Graphic3d_VerticalTextAlignment verticalAlignment(int value)
    {
        switch (value)
        {
        case OcctTextVertical_Bottom: return Graphic3d_VTA_BOTTOM;
        case OcctTextVertical_Center: return Graphic3d_VTA_CENTER;
        case OcctTextVertical_Top: return Graphic3d_VTA_TOP;
        default: throw std::invalid_argument("Unsupported vertical text alignment.");
        }
    }

    void validateTextOptions(const OcctBRepTextOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("BRep text options are null.");
        if (options->structSize < sizeof(OcctBRepTextOptions) || options->apiVersion != TextOptionsApiVersion)
            throw std::invalid_argument("Unsupported BRep text options size or version.");
    }

    void validateAnnotationOptions(const OcctBRepAnnotationOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("BRep annotation options are null.");
        if (options->structSize < sizeof(OcctBRepAnnotationOptions) || options->apiVersion != AnnotationOptionsApiVersion)
            throw std::invalid_argument("Unsupported BRep annotation options size or version.");
        OcctBridge::requirePositive(options->textHeight, "Text height");
        OcctBridge::requirePositive(options->arrowSize, "Arrow size");
        if (!std::isfinite(options->offset)) throw std::invalid_argument("Annotation offset must be finite.");
    }

    TopoDS_Edge requireEdge(ModelSession* model, OcctObjectId id, const char* name)
    {
        const TopoDS_Shape& shape = model->requireShape(id);
        if (shape.ShapeType() != TopAbs_EDGE)
            throw std::invalid_argument(std::string(name) + " must be an edge.");
        return TopoDS::Edge(shape);
    }

    template<typename Factory>
    OcctStatus executeShapeStatus(ModelSession* model, OcctObjectId* output, Factory&& factory)
    {
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        model->errors.clear();
        if (output == nullptr)
        {
            model->errors.set(OcctStatus_ErrorInvalidArgument, "Result shape ID output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }

        *output = 0;
        if (!execute(model, [&] { *output = model->addShape(factory()); }))
            return model->errors.code;
        return OcctStatus_Ok;
    }
}

extern "C"
{
    OcctStatus occt_model_brep_text_create(
        OcctModelingSessionHandle session,
        const char* utf8Text,
        const char* fontName,
        const OcctBRepTextOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeShapeStatus(model, resultShapeId, [&]
        {
            validateTextOptions(options);
            return buildBRepText(
                utf8Text,
                fontName,
                options->position,
                options->normal,
                options->xDirection,
                options->height,
                options->extrusionDepth,
                options->bold != 0,
                options->italic != 0,
                horizontalAlignment(options->horizontalAlignment),
                verticalAlignment(options->verticalAlignment));
        });
    }

    OcctStatus occt_model_length_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId edgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeShapeStatus(model, resultShapeId, [&]
        {
            validateAnnotationOptions(options);
            return buildLengthAnnotation(
                requireEdge(model, edgeId, "Length annotation input"),
                options->offset,
                options->textHeight,
                options->arrowSize,
                fontName);
        });
    }

    OcctStatus occt_model_angle_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeShapeStatus(model, resultShapeId, [&]
        {
            validateAnnotationOptions(options);
            OcctBridge::requirePositive(options->offset, "Angular annotation radius");
            return buildAngleAnnotation(
                requireEdge(model, firstEdgeId, "First angular annotation input"),
                requireEdge(model, secondEdgeId, "Second angular annotation input"),
                options->offset,
                options->textHeight,
                options->arrowSize,
                fontName);
        });
    }

    OcctStatus occt_model_radius_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId circularEdgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeShapeStatus(model, resultShapeId, [&]
        {
            validateAnnotationOptions(options);
            return buildRadiusAnnotation(
                requireEdge(model, circularEdgeId, "Radius annotation input"),
                options->offset,
                options->textHeight,
                options->arrowSize,
                fontName);
        });
    }

    OcctStatus occt_model_diameter_annotation_create(
        OcctModelingSessionHandle session,
        OcctObjectId circularEdgeId,
        const char* fontName,
        const OcctBRepAnnotationOptions* options,
        OcctObjectId* resultShapeId)
    {
        ModelSession* model = reinterpret_cast<ModelSession*>(session);
        return executeShapeStatus(model, resultShapeId, [&]
        {
            validateAnnotationOptions(options);
            return buildDiameterAnnotation(
                requireEdge(model, circularEdgeId, "Diameter annotation input"),
                options->offset,
                options->textHeight,
                options->arrowSize,
                fontName);
        });
    }
}

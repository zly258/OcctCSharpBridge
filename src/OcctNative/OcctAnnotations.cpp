#include "presentation/OcctAnnotations.h"
#include "core/OcctInternal.hxx"

#include <AIS_TextLabel.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <Prs3d_DimensionAspect.hxx>
#include <Precision.hxx>
#include <PrsDim_AngleDimension.hxx>
#include <PrsDim_DiameterDimension.hxx>
#include <PrsDim_Dimension.hxx>
#include <PrsDim_LengthDimension.hxx>
#include <PrsDim_RadiusDimension.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>
#include <gp_Pln.hxx>

#include <cmath>
#include <stdexcept>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t TextOptionsApiVersion = 1;
    constexpr std::uint32_t DimensionOptionsApiVersion = 1;
    constexpr std::uint32_t AllTextUpdateBits =
        OcctViewerTextUpdate_Content |
        OcctViewerTextUpdate_Position |
        OcctViewerTextUpdate_Height |
        OcctViewerTextUpdate_Font |
        OcctViewerTextUpdate_Angle |
        OcctViewerTextUpdate_Zoomable |
        OcctViewerTextUpdate_Color;
    constexpr std::uint32_t AllDimensionUpdateBits =
        OcctViewerDimensionUpdate_Flyout |
        OcctViewerDimensionUpdate_Color;

    TCollection_ExtendedString extended(const char* text)
    {
        return TCollection_ExtendedString(text == nullptr ? "" : text, Standard_True);
    }

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    void validateTextOptions(const OcctViewerTextOptions* options, bool isUpdate)
    {
        if (options == nullptr) throw std::invalid_argument("Viewer text options are null.");
        if (options->structSize < sizeof(OcctViewerTextOptions) || options->apiVersion != TextOptionsApiVersion)
            throw std::invalid_argument("Unsupported viewer text options size or version.");
        if ((options->updateMask & ~AllTextUpdateBits) != 0)
            throw std::invalid_argument("Viewer text update mask contains unsupported bits.");
        if (!isUpdate || (options->updateMask & OcctViewerTextUpdate_Height) != 0)
            requirePositive(options->height, "Text height");
        if (!isUpdate || (options->updateMask & OcctViewerTextUpdate_Angle) != 0)
        {
            if (!std::isfinite(options->angleDegrees))
                throw std::invalid_argument("Text angle must be finite.");
        }
    }

    void validateDimensionOptions(const OcctViewerDimensionOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Viewer dimension options are null.");
        if (options->structSize < sizeof(OcctViewerDimensionOptions) || options->apiVersion != DimensionOptionsApiVersion)
            throw std::invalid_argument("Unsupported viewer dimension options size or version.");
        if ((options->updateMask & ~AllDimensionUpdateBits) != 0)
            throw std::invalid_argument("Viewer dimension update mask contains unsupported bits.");
        if (!std::isfinite(options->flyout))
            throw std::invalid_argument("Dimension flyout must be finite.");
    }

    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
        return *entry;
    }

    Handle(AIS_TextLabel) requiredText(Engine* engine, OcctObjectId textId)
    {
        ObjectEntry* entry = engine->findObject(textId);
        if (entry == nullptr || entry->kind != OcctObject_Text)
            throw std::invalid_argument("Text ID does not exist.");
        Handle(AIS_TextLabel) label = Handle(AIS_TextLabel)::DownCast(entry->presentation);
        if (label.IsNull()) throw std::runtime_error("Text presentation type is invalid.");
        return label;
    }

    Handle(PrsDim_Dimension) requiredDimension(Engine* engine, OcctObjectId dimensionId)
    {
        ObjectEntry* entry = engine->findObject(dimensionId);
        if (entry == nullptr || entry->kind != OcctObject_Dimension)
            throw std::invalid_argument("Dimension ID does not exist.");
        Handle(PrsDim_Dimension) dimension = Handle(PrsDim_Dimension)::DownCast(entry->presentation);
        if (dimension.IsNull()) throw std::runtime_error("Dimension presentation type is invalid.");
        return dimension;
    }

    void setDimensionColor(const Handle(PrsDim_Dimension)& dimension, double red, double green, double blue)
    {
        Handle(Prs3d_DimensionAspect) aspect = new Prs3d_DimensionAspect();
        aspect->SetCommonColor(color(red, green, blue));
        dimension->SetDimensionAspect(aspect);
    }

    void configureDimension(
        const Handle(PrsDim_Dimension)& dimension,
        const OcctViewerDimensionOptions& options)
    {
        setDimensionColor(dimension, options.red, options.green, options.blue);
        dimension->SetFlyout(options.flyout);
        if (!dimension->IsValid())
            throw std::runtime_error("Dimension geometry is not valid for this annotation type.");
    }

    gp_Pln dimensionPlaneForEdge(const TopoDS_Edge& edge)
    {
        BRepAdaptor_Curve curve(edge);
        const gp_Pnt first = curve.Value(curve.FirstParameter());
        const gp_Pnt last = curve.Value(curve.LastParameter());
        const gp_Vec edgeVector(first, last);
        if (edgeVector.SquareMagnitude() <= Precision::SquareConfusion())
            throw std::runtime_error("Edge has zero length.");
        const gp_Dir edgeDirection(edgeVector);
        gp_Dir reference(0.0, 0.0, 1.0);
        if (std::abs(edgeDirection.Dot(reference)) > 0.95) reference = gp_Dir(1.0, 0.0, 0.0);
        return gp_Pln(first, edgeDirection.Crossed(reference));
    }

    template<typename Function>
    OcctStatus executeStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    template<typename Function>
    OcctStatus executeObjectStatus(Engine* engine, OcctObjectId* output, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        if (output == nullptr)
        {
            engine->setError(OcctStatus_ErrorInvalidArgument, "Result object ID output is null.");
            return OcctStatus_ErrorInvalidArgument;
        }
        *output = 0;
        const OcctObjectId value = executeObject(engine, std::forward<Function>(function));
        if (value == 0) return engine->errors.code;
        *output = value;
        return OcctStatus_Ok;
    }

    OcctViewerTextOptions textOptions(
        std::uint32_t updateMask,
        OcctPoint3d position,
        double height,
        double angleDegrees,
        double red,
        double green,
        double blue,
        int zoomable)
    {
        return {
            static_cast<std::uint32_t>(sizeof(OcctViewerTextOptions)),
            TextOptionsApiVersion,
            updateMask,
            position,
            height,
            angleDegrees,
            red,
            green,
            blue,
            zoomable != 0 ? 1 : 0 };
    }

    OcctViewerDimensionOptions dimensionOptions(
        std::uint32_t updateMask,
        double flyout,
        double red,
        double green,
        double blue)
    {
        return {
            static_cast<std::uint32_t>(sizeof(OcctViewerDimensionOptions)),
            DimensionOptionsApiVersion,
            updateMask,
            flyout,
            red,
            green,
            blue };
    }
}

extern "C"
{
    OcctStatus occt_engine_text_create(
        OcctEngineHandle handle,
        const char* utf8Text,
        const char* fontName,
        const OcctViewerTextOptions* options,
        OcctObjectId* resultTextId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, resultTextId, [&]
        {
            validateTextOptions(options, false);
            Handle(AIS_TextLabel) label = new AIS_TextLabel();
            label->SetText(extended(utf8Text));
            label->SetPosition(point(options->position));
            label->SetHeight(options->height);
            label->SetAngle(options->angleDegrees * 3.14159265358979323846 / 180.0);
            label->SetZoomable(options->zoomable != 0);
            label->SetColor(color(options->red, options->green, options->blue));
            if (fontName != nullptr && fontName[0] != '\0') label->SetFont(fontName);
            return engine->addPresentation(label, OcctObject_Text, "Text");
        });
    }

    OcctStatus occt_engine_text_update(
        OcctEngineHandle handle,
        OcctObjectId textId,
        const char* utf8Text,
        const char* fontName,
        const OcctViewerTextOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeStatus(engine, [&]
        {
            validateTextOptions(options, true);
            Handle(AIS_TextLabel) label = requiredText(engine, textId);
            const std::uint32_t mask = options->updateMask;
            if ((mask & OcctViewerTextUpdate_Content) != 0) label->SetText(extended(utf8Text));
            if ((mask & OcctViewerTextUpdate_Position) != 0) label->SetPosition(point(options->position));
            if ((mask & OcctViewerTextUpdate_Height) != 0) label->SetHeight(options->height);
            if ((mask & OcctViewerTextUpdate_Font) != 0) label->SetFont(fontName == nullptr ? "" : fontName);
            if ((mask & OcctViewerTextUpdate_Angle) != 0)
                label->SetAngle(options->angleDegrees * 3.14159265358979323846 / 180.0);
            if ((mask & OcctViewerTextUpdate_Zoomable) != 0) label->SetZoomable(options->zoomable != 0);
            if ((mask & OcctViewerTextUpdate_Color) != 0)
                label->SetColor(color(options->red, options->green, options->blue));
            engine->viewerContext.context->Redisplay(label, Standard_True);
        });
    }

    OcctStatus occt_engine_dimension_create(
        OcctEngineHandle handle,
        int kind,
        OcctObjectId firstShapeId,
        OcctObjectId secondShapeId,
        const OcctViewerDimensionOptions* options,
        OcctObjectId* resultDimensionId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeObjectStatus(engine, resultDimensionId, [&]
        {
            validateDimensionOptions(options);
            Handle(PrsDim_Dimension) dimension;
            const char* name = nullptr;
            switch (kind)
            {
            case OcctViewerDimension_Length:
            {
                ObjectEntry& edge = requiredShape(engine, firstShapeId);
                if (edge.shape.ShapeType() != TopAbs_EDGE)
                    throw std::invalid_argument("Length dimension input must be an edge.");
                const TopoDS_Edge topologicalEdge = TopoDS::Edge(edge.shape);
                dimension = new PrsDim_LengthDimension(topologicalEdge, dimensionPlaneForEdge(topologicalEdge));
                name = "LengthDimension";
                break;
            }
            case OcctViewerDimension_Angle:
            {
                ObjectEntry& first = requiredShape(engine, firstShapeId);
                ObjectEntry& second = requiredShape(engine, secondShapeId);
                if (first.shape.ShapeType() != TopAbs_EDGE || second.shape.ShapeType() != TopAbs_EDGE)
                    throw std::invalid_argument("Angle dimension inputs must be edges.");
                dimension = new PrsDim_AngleDimension(TopoDS::Edge(first.shape), TopoDS::Edge(second.shape));
                name = "AngleDimension";
                break;
            }
            case OcctViewerDimension_Radius:
                dimension = new PrsDim_RadiusDimension(requiredShape(engine, firstShapeId).shape);
                name = "RadiusDimension";
                break;
            case OcctViewerDimension_Diameter:
                dimension = new PrsDim_DiameterDimension(requiredShape(engine, firstShapeId).shape);
                name = "DiameterDimension";
                break;
            default:
                throw std::invalid_argument("Unsupported viewer dimension kind.");
            }

            configureDimension(dimension, *options);
            return engine->addPresentation(dimension, OcctObject_Dimension, name);
        });
    }

    OcctStatus occt_engine_dimension_update(
        OcctEngineHandle handle,
        OcctObjectId dimensionId,
        const OcctViewerDimensionOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeStatus(engine, [&]
        {
            validateDimensionOptions(options);
            Handle(PrsDim_Dimension) dimension = requiredDimension(engine, dimensionId);
            const std::uint32_t mask = options->updateMask;
            if ((mask & OcctViewerDimensionUpdate_Flyout) != 0) dimension->SetFlyout(options->flyout);
            if ((mask & OcctViewerDimensionUpdate_Color) != 0)
                setDimensionColor(dimension, options->red, options->green, options->blue);
            if (!dimension->IsValid())
                throw std::runtime_error("Dimension geometry is not valid after update.");
            engine->viewerContext.context->Redisplay(dimension, Standard_True);
        });
    }

    // ABI 4 compatibility shell. Keep the frozen symbols, but route every operation
    // through the current typed/status-based API so there is only one implementation path.
    OcctObjectId occt_add_text(
        OcctHandle handle,
        const char* text,
        OcctPoint3d position,
        double height,
        double red,
        double green,
        double blue,
        int zoomable)
    {
        const OcctViewerTextOptions options = textOptions(
            AllTextUpdateBits,
            position,
            height,
            0.0,
            red,
            green,
            blue,
            zoomable);
        OcctObjectId result = 0;
        return occt_engine_text_create(
            reinterpret_cast<OcctEngineHandle>(handle),
            text,
            "",
            &options,
            &result) == OcctStatus_Ok ? result : 0;
    }

    int occt_set_text(OcctHandle handle, OcctObjectId textId, const char* text)
    {
        const OcctViewerTextOptions options = textOptions(OcctViewerTextUpdate_Content, {}, 1.0, 0.0, 0.0, 0.0, 0.0, 0);
        return occt_engine_text_update(reinterpret_cast<OcctEngineHandle>(handle), textId, text, "", &options) == OcctStatus_Ok ? 1 : 0;
    }

    int occt_set_text_position(OcctHandle handle, OcctObjectId textId, OcctPoint3d position)
    {
        const OcctViewerTextOptions options = textOptions(OcctViewerTextUpdate_Position, position, 1.0, 0.0, 0.0, 0.0, 0.0, 0);
        return occt_engine_text_update(reinterpret_cast<OcctEngineHandle>(handle), textId, "", "", &options) == OcctStatus_Ok ? 1 : 0;
    }

    int occt_set_text_height(OcctHandle handle, OcctObjectId textId, double height)
    {
        const OcctViewerTextOptions options = textOptions(OcctViewerTextUpdate_Height, {}, height, 0.0, 0.0, 0.0, 0.0, 0);
        return occt_engine_text_update(reinterpret_cast<OcctEngineHandle>(handle), textId, "", "", &options) == OcctStatus_Ok ? 1 : 0;
    }

    int occt_set_text_font(OcctHandle handle, OcctObjectId textId, const char* fontName)
    {
        const OcctViewerTextOptions options = textOptions(OcctViewerTextUpdate_Font, {}, 1.0, 0.0, 0.0, 0.0, 0.0, 0);
        return occt_engine_text_update(reinterpret_cast<OcctEngineHandle>(handle), textId, "", fontName, &options) == OcctStatus_Ok ? 1 : 0;
    }

    int occt_set_text_angle(OcctHandle handle, OcctObjectId textId, double angleDegrees)
    {
        const OcctViewerTextOptions options = textOptions(OcctViewerTextUpdate_Angle, {}, 1.0, angleDegrees, 0.0, 0.0, 0.0, 0);
        return occt_engine_text_update(reinterpret_cast<OcctEngineHandle>(handle), textId, "", "", &options) == OcctStatus_Ok ? 1 : 0;
    }

    int occt_set_text_zoomable(OcctHandle handle, OcctObjectId textId, int zoomable)
    {
        const OcctViewerTextOptions options = textOptions(OcctViewerTextUpdate_Zoomable, {}, 1.0, 0.0, 0.0, 0.0, 0.0, zoomable);
        return occt_engine_text_update(reinterpret_cast<OcctEngineHandle>(handle), textId, "", "", &options) == OcctStatus_Ok ? 1 : 0;
    }

    int occt_set_dimension_flyout(OcctHandle handle, OcctObjectId dimensionId, double flyout)
    {
        const OcctViewerDimensionOptions options = dimensionOptions(OcctViewerDimensionUpdate_Flyout, flyout, 0.0, 0.0, 0.0);
        return occt_engine_dimension_update(reinterpret_cast<OcctEngineHandle>(handle), dimensionId, &options) == OcctStatus_Ok ? 1 : 0;
    }

    OcctObjectId occt_add_length_dimension(
        OcctHandle handle,
        OcctObjectId edgeId,
        double flyout,
        double red,
        double green,
        double blue)
    {
        const OcctViewerDimensionOptions options = dimensionOptions(AllDimensionUpdateBits, flyout, red, green, blue);
        OcctObjectId result = 0;
        return occt_engine_dimension_create(
            reinterpret_cast<OcctEngineHandle>(handle),
            OcctViewerDimension_Length,
            edgeId,
            0,
            &options,
            &result) == OcctStatus_Ok ? result : 0;
    }

    OcctObjectId occt_add_angle_dimension(
        OcctHandle handle,
        OcctObjectId firstEdgeId,
        OcctObjectId secondEdgeId,
        double flyout,
        double red,
        double green,
        double blue)
    {
        const OcctViewerDimensionOptions options = dimensionOptions(AllDimensionUpdateBits, flyout, red, green, blue);
        OcctObjectId result = 0;
        return occt_engine_dimension_create(
            reinterpret_cast<OcctEngineHandle>(handle),
            OcctViewerDimension_Angle,
            firstEdgeId,
            secondEdgeId,
            &options,
            &result) == OcctStatus_Ok ? result : 0;
    }

    OcctObjectId occt_add_radius_dimension(
        OcctHandle handle,
        OcctObjectId shapeId,
        double flyout,
        double red,
        double green,
        double blue)
    {
        const OcctViewerDimensionOptions options = dimensionOptions(AllDimensionUpdateBits, flyout, red, green, blue);
        OcctObjectId result = 0;
        return occt_engine_dimension_create(
            reinterpret_cast<OcctEngineHandle>(handle),
            OcctViewerDimension_Radius,
            shapeId,
            0,
            &options,
            &result) == OcctStatus_Ok ? result : 0;
    }

    OcctObjectId occt_add_diameter_dimension(
        OcctHandle handle,
        OcctObjectId shapeId,
        double flyout,
        double red,
        double green,
        double blue)
    {
        const OcctViewerDimensionOptions options = dimensionOptions(AllDimensionUpdateBits, flyout, red, green, blue);
        OcctObjectId result = 0;
        return occt_engine_dimension_create(
            reinterpret_cast<OcctEngineHandle>(handle),
            OcctViewerDimension_Diameter,
            shapeId,
            0,
            &options,
            &result) == OcctStatus_Ok ? result : 0;
    }
}

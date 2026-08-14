#include "selection/OcctSelectionOverlay.h"
#include "core/OcctInternal.hxx"

#include <AIS_DisplayStatus.hxx>
#include <Aspect_TypeOfLine.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_TransModeFlags.hxx>
#include <Graphic3d_ZLayerId.hxx>

#include <algorithm>
#include <cmath>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    constexpr std::uint32_t SelectionRectangleOptionsApiVersion = 1;

    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->errors.code;
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeOverlayStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->errors.code;
    }

    void validateOptions(const OcctViewerSelectionRectangleOptions* options)
    {
        if (options == nullptr) throw std::invalid_argument("Selection rectangle options are null.");
        if (options->structSize < sizeof(OcctViewerSelectionRectangleOptions) ||
            options->apiVersion != SelectionRectangleOptionsApiVersion)
        {
            throw std::invalid_argument("Unsupported selection rectangle options size or version.");
        }
        (void)color(options->lineColor.r, options->lineColor.g, options->lineColor.b);
        (void)color(options->fillColor.r, options->fillColor.g, options->fillColor.b);
        if (!std::isfinite(options->fillTransparency) ||
            options->fillTransparency < 0.0 || options->fillTransparency > 1.0)
        {
            throw std::invalid_argument("Selection rectangle fill transparency must be between 0 and 1.");
        }
        requirePositive(options->lineWidth, "Selection rectangle line width");
    }
}

extern "C"
{
    OcctStatus occt_engine_selection_rectangle_overlay_show(
        OcctEngineHandle handle,
        const OcctViewerSelectionRectangleOptions* options)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeOverlayStatus(engine, [&]
        {
            validateOptions(options);
            const Quantity_Color lineColor = color(
                options->lineColor.r,
                options->lineColor.g,
                options->lineColor.b);
            const Quantity_Color fillColor = color(
                options->fillColor.r,
                options->fillColor.g,
                options->fillColor.b);

            if (engine->viewerContext.selectionRubberBand.IsNull())
            {
                engine->viewerContext.selectionRubberBand = new AIS_RubberBand(
                    lineColor,
                    Aspect_TOL_SOLID,
                    fillColor,
                    options->fillTransparency,
                    options->lineWidth);
                engine->viewerContext.selectionRubberBand->SetZLayer(Graphic3d_ZLayerId_TopOSD);
                engine->viewerContext.selectionRubberBand->SetTransformPersistence(
                    new Graphic3d_TransformPers(Graphic3d_TMF_2d, Aspect_TOTP_LEFT_LOWER));
                engine->viewerContext.selectionRubberBand->SetDisplayMode(0);
                engine->viewerContext.selectionRubberBand->SetMutable(Standard_True);
            }
            else
            {
                engine->viewerContext.selectionRubberBand->SetLineColor(lineColor);
                engine->viewerContext.selectionRubberBand->SetLineType(Aspect_TOL_SOLID);
                engine->viewerContext.selectionRubberBand->SetLineWidth(options->lineWidth);
                engine->viewerContext.selectionRubberBand->SetFilling(
                    fillColor,
                    options->fillTransparency);
            }

            const int minX = std::min(options->x1, options->x2);
            const int maxX = std::max(options->x1, options->x2);
            const int minClientY = std::min(options->y1, options->y2);
            const int maxClientY = std::max(options->y1, options->y2);
            Standard_Integer windowWidth = 0;
            Standard_Integer windowHeight = 0;
            engine->viewerContext.window->Size(windowWidth, windowHeight);
            if (windowHeight <= 0) throw std::runtime_error("The OCCT window height is invalid.");

            const int minY = windowHeight - maxClientY;
            const int maxY = windowHeight - minClientY;
            engine->viewerContext.selectionRubberBand->SetRectangle(minX, minY, maxX, maxY);

            if (engine->viewerContext.context->IsDisplayed(engine->viewerContext.selectionRubberBand))
            {
                engine->viewerContext.context->Redisplay(
                    engine->viewerContext.selectionRubberBand,
                    Standard_False);
            }
            else
            {
                engine->viewerContext.context->Display(
                    engine->viewerContext.selectionRubberBand,
                    0,
                    -1,
                    Standard_False,
                    AIS_DS_Displayed);
            }
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_selection_rectangle_overlay_hide(OcctEngineHandle handle)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeOverlayStatus(engine, [&]
        {
            if (engine->viewerContext.selectionRubberBand.IsNull()) return;
            if (engine->viewerContext.context->IsDisplayed(engine->viewerContext.selectionRubberBand))
            {
                engine->viewerContext.context->Remove(
                    engine->viewerContext.selectionRubberBand,
                    Standard_False);
            }
            engine->viewerContext.selectionRubberBand->ClearPoints();
            engine->requestRedraw();
        });
    }
}

#include "OcctInternal.hxx"
#include "OcctSelectionOverlay.h"

#include <AIS_DisplayStatus.hxx>
#include <Aspect_TypeOfLine.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_TransModeFlags.hxx>
#include <Graphic3d_ZLayerId.hxx>

using namespace OcctBridge;

extern "C"
{
    int occt_show_selection_rectangle(
        OcctHandle h,
        int x1,
        int y1,
        int x2,
        int y2,
        double lineR,
        double lineG,
        double lineB,
        double fillR,
        double fillG,
        double fillB,
        double fillTransparency,
        double lineWidth)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;

        return execute(engine, [&]
        {
            const Quantity_Color lineColor = color(lineR, lineG, lineB);
            const Quantity_Color fillColor = color(fillR, fillG, fillB);
            const Standard_Real transparency = std::clamp(fillTransparency, 0.0, 1.0);
            const Standard_Real width = std::max(lineWidth, 0.5);

            if (engine->viewerContext.selectionRubberBand.IsNull())
            {
                engine->viewerContext.selectionRubberBand = new AIS_RubberBand(
                    lineColor,
                    Aspect_TOL_SOLID,
                    fillColor,
                    transparency,
                    width);
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
                engine->viewerContext.selectionRubberBand->SetLineWidth(width);
                engine->viewerContext.selectionRubberBand->SetFilling(fillColor, transparency);
            }

            const int minX = std::min(x1, x2);
            const int maxX = std::max(x1, x2);
            const int minClientY = std::min(y1, y2);
            const int maxClientY = std::max(y1, y2);
            Standard_Integer windowWidth = 0;
            Standard_Integer windowHeight = 0;
            engine->viewerContext.window->Size(windowWidth, windowHeight);
            if (windowHeight <= 0) throw std::runtime_error("The OCCT window height is invalid.");

            // AIS_RubberBand with LEFT_LOWER persistence uses a bottom-left Y origin,
            // while WinForms/WPF mouse coordinates use a top-left Y origin.
            const int minY = windowHeight - maxClientY;
            const int maxY = windowHeight - minClientY;
            engine->viewerContext.selectionRubberBand->SetRectangle(minX, minY, maxX, maxY);

            if (engine->viewerContext.context->IsDisplayed(engine->viewerContext.selectionRubberBand))
            {
                engine->viewerContext.context->Redisplay(engine->viewerContext.selectionRubberBand, Standard_False);
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

    int occt_hide_selection_rectangle(OcctHandle h)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;

        return execute(engine, [&]
        {
            if (engine->viewerContext.selectionRubberBand.IsNull()) return;

            if (engine->viewerContext.context->IsDisplayed(engine->viewerContext.selectionRubberBand))
            {
                engine->viewerContext.context->Remove(engine->viewerContext.selectionRubberBand, Standard_False);
            }
            engine->viewerContext.selectionRubberBand->ClearPoints();
            engine->requestRedraw();
        });
    }
}

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

            if (engine->selectionRubberBand.IsNull())
            {
                engine->selectionRubberBand = new AIS_RubberBand(
                    lineColor,
                    Aspect_TOL_SOLID,
                    fillColor,
                    transparency,
                    width);
                engine->selectionRubberBand->SetZLayer(Graphic3d_ZLayerId_TopOSD);
                engine->selectionRubberBand->SetTransformPersistence(
                    new Graphic3d_TransformPers(Graphic3d_TMF_2d, Aspect_TOTP_LEFT_UPPER));
                engine->selectionRubberBand->SetDisplayMode(0);
                engine->selectionRubberBand->SetMutable(Standard_True);
            }
            else
            {
                engine->selectionRubberBand->SetLineColor(lineColor);
                engine->selectionRubberBand->SetLineType(Aspect_TOL_SOLID);
                engine->selectionRubberBand->SetLineWidth(width);
                engine->selectionRubberBand->SetFilling(fillColor, transparency);
            }

            const int minX = std::min(x1, x2);
            const int maxX = std::max(x1, x2);
            const int minY = std::min(y1, y2);
            const int maxY = std::max(y1, y2);
            engine->selectionRubberBand->SetRectangle(minX, -maxY, maxX, -minY);

            if (engine->context->IsDisplayed(engine->selectionRubberBand))
            {
                engine->context->Redisplay(engine->selectionRubberBand, Standard_False);
            }
            else
            {
                engine->context->Display(
                    engine->selectionRubberBand,
                    0,
                    -1,
                    Standard_False,
                    AIS_DS_Displayed);
            }

            // The rubber band is an immediate top-layer presentation. Updating only that
            // layer avoids a full scene redraw on every mouse move and prevents flicker.
            engine->view->InvalidateImmediate();
            engine->view->RedrawImmediate();
        });
    }

    int occt_hide_selection_rectangle(OcctHandle h)
    {
        Engine* engine = engineOf(h);
        if (!validateInitialized(engine)) return 0;

        return execute(engine, [&]
        {
            if (engine->selectionRubberBand.IsNull()) return;

            if (engine->context->IsDisplayed(engine->selectionRubberBand))
            {
                engine->context->Remove(engine->selectionRubberBand, Standard_False);
            }
            engine->selectionRubberBand->ClearPoints();
            engine->view->InvalidateImmediate();
            engine->view->RedrawImmediate();
        });
    }
}

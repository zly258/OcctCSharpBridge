#include "OcctInternal.hxx"

#include <AIS_SelectionScheme.hxx>
#include <Graphic3d_Vec2.hxx>
#include <StdSelect_ViewerSelector3d.hxx>

using namespace OcctBridge;

extern "C"
{
    int occt_set_selection_tolerance(OcctHandle h, int pixelTolerance)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            if (pixelTolerance < 0 || pixelTolerance > 100)
                throw std::invalid_argument("Selection tolerance must be between 0 and 100 pixels.");
            e->context->SetPixelTolerance(pixelTolerance);
        });
    }

    int occt_move_to(OcctHandle h, int x, int y)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->context->MoveTo(x, y, e->view, Standard_True); });
    }

    int occt_select(OcctHandle h, int x, int y, int append)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->context->MoveTo(x, y, e->view, Standard_False);
            if (e->context->HasDetected())
            {
                e->context->SelectDetected(append ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace);
            }
            else if (!append)
            {
                e->context->ClearSelected(Standard_False);
            }
            e->context->UpdateCurrentViewer();
        });
    }

    int occt_select_rectangle_ex(OcctHandle h, int x1, int y1, int x2, int y2, int append, int allowOverlap)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            const Handle(StdSelect_ViewerSelector3d)& selector = e->context->MainSelector();
            selector->AllowOverlapDetection(allowOverlap != 0);
            const Graphic3d_Vec2i minPoint(std::min(x1, x2), std::min(y1, y2));
            const Graphic3d_Vec2i maxPoint(std::max(x1, x2), std::max(y1, y2));
            e->context->SelectRectangle(
                minPoint,
                maxPoint,
                e->view,
                append ? AIS_SelectionScheme_Add : AIS_SelectionScheme_Replace);
            selector->AllowOverlapDetection(Standard_False);
            e->context->UpdateCurrentViewer();
        });
    }

    int occt_select_object(OcctHandle h, OcctObjectId objectId, int append)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findObject(objectId);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            if (!entry->selectable)
                throw std::invalid_argument("Object is not selectable.");
            if (!append) e->context->ClearSelected(Standard_False);
            e->context->SetSelected(entry->presentation, Standard_False);
            e->view->Redraw();
        });
    }

    int occt_set_selection_mode(OcctHandle h, int mode)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            e->selectionMode = mode;
            for (auto& pair : e->objects) e->applySelectionMode(pair.second.presentation);
            e->view->Redraw();
        });
    }

    int occt_clear_selection(OcctHandle h)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return execute(e, [&] { e->context->ClearSelected(Standard_True); });
    }
}

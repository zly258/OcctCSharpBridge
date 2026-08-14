#include "OcctViewerContext.hxx"

#include <stdexcept>

namespace OcctBridge
{
    bool ViewerContext::isInitialized() const
    {
        return !view.IsNull() && !context.IsNull();
    }

    bool ViewerContext::isUpdating() const
    {
        return updateDepth > 0;
    }

    void ViewerContext::beginUpdate()
    {
        ++updateDepth;
    }

    void ViewerContext::requestRedraw()
    {
        if (isUpdating())
        {
            redrawPending = true;
            return;
        }
        view->Redraw();
    }

    void ViewerContext::requestFitAll()
    {
        if (isUpdating())
        {
            fitAllPending = true;
            redrawPending = true;
            return;
        }
        view->FitAll(0.01, Standard_False);
        view->ZFitAll();
        view->Redraw();
    }

    void ViewerContext::endUpdate(bool fitAll)
    {
        if (updateDepth <= 0) throw std::logic_error("No OCCT display batch is active.");
        if (fitAll)
        {
            fitAllPending = true;
            redrawPending = true;
        }
        --updateDepth;
        if (updateDepth > 0) return;

        if (fitAllPending)
        {
            view->FitAll(0.01, Standard_False);
            view->ZFitAll();
        }
        if (fitAllPending || redrawPending) view->Redraw();
        fitAllPending = false;
        redrawPending = false;
    }
}

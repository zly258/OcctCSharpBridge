#include "core/OcctInternal.hxx"

using namespace OcctBridge;

namespace
{
    ObjectEntry& requiredObject(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findObject(id);
        if (entry == nullptr) throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }
}

extern "C"
{
    int occt_show_all(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            for (auto& pair : e->scene.objects)
                if (!pair.second.presentation.IsNull())
                    e->viewerContext.context->Display(pair.second.presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_hide_all(OcctHandle h)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            for (auto& pair : e->scene.objects)
                if (!pair.second.presentation.IsNull())
                    e->viewerContext.context->Erase(pair.second.presentation, Standard_False);
            e->requestRedraw();
        });
    }

    int occt_redisplay_object(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, id);
            e->viewerContext.context->Redisplay(entry.presentation, Standard_False, Standard_True);
            e->requestRedraw();
        });
    }

    int occt_highlight_object(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, id);
            e->viewerContext.context->HilightWithColor(
                entry.presentation,
                e->viewerContext.context->HighlightStyle(),
                Standard_False);
            e->requestRedraw();
        });
    }

    int occt_unhighlight_object(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e)) return 0;
        return execute(e, [&]
        {
            ObjectEntry& entry = requiredObject(e, id);
            e->viewerContext.context->Unhilight(entry.presentation, Standard_False);
            e->requestRedraw();
        });
    }

    OcctObjectId occt_copy_selected_subshape_at(OcctHandle h, int index)
    {
        Engine* e = engineOf(h); if (!validateInitialized(e) || index < 0) return 0;
        return executeObject(e, [&]() -> OcctObjectId
        {
            int current = 0;
            for (e->viewerContext.context->InitSelected();
                 e->viewerContext.context->MoreSelected();
                 e->viewerContext.context->NextSelected(), ++current)
            {
                if (current != index) continue;
                if (!e->viewerContext.context->HasSelectedShape())
                    throw std::runtime_error("The selected item has no topological shape.");
                const TopoDS_Shape selected = e->viewerContext.context->SelectedShape();
                if (selected.IsNull())
                    throw std::runtime_error("The selected topological subshape is null.");
                return e->addShape(selected, false, "SelectedSubshape");
            }
            throw std::out_of_range("Selected subshape index is out of range.");
        });
    }
}

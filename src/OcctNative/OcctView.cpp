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
}

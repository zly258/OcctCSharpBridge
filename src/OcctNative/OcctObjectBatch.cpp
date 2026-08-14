#include "core/OcctInternal.hxx"

#include <Graphic3d_MaterialAspect.hxx>

#include <unordered_set>

using namespace OcctBridge;

namespace
{
    std::vector<ObjectEntry*> requireObjects(
        Engine* engine,
        const OcctObjectId* objectIds,
        int count)
    {
        if (count < 0) throw std::invalid_argument("Object count must not be negative.");
        if (count > 0 && objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");

        std::vector<ObjectEntry*> entries;
        std::unordered_set<OcctObjectId> uniqueIds;
        entries.reserve(static_cast<std::size_t>(count));
        uniqueIds.reserve(static_cast<std::size_t>(count));
        for (int index = 0; index < count; ++index)
        {
            const OcctObjectId id = objectIds[index];
            if (!uniqueIds.insert(id).second) continue;
            ObjectEntry* entry = engine->findObject(id);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            entries.push_back(entry);
        }
        return entries;
    }
}

extern "C"
{
    int occt_set_objects_color(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        double r,
        double g,
        double b)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            const auto entries = requireObjects(engine, objectIds, count);
            const Quantity_Color value = color(r, g, b);
            for (ObjectEntry* entry : entries)
                engine->viewerContext.context->SetColor(entry->presentation, value, Standard_False);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    int occt_set_objects_transparency(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        double transparency)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            const auto entries = requireObjects(engine, objectIds, count);
            const double value = std::clamp(transparency, 0.0, 1.0);
            for (ObjectEntry* entry : entries)
                engine->viewerContext.context->SetTransparency(entry->presentation, value, Standard_False);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    int occt_set_objects_visible(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int visible)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            const auto entries = requireObjects(engine, objectIds, count);
            for (ObjectEntry* entry : entries)
            {
                if (visible != 0) engine->viewerContext.context->Display(entry->presentation, Standard_False);
                else engine->viewerContext.context->Erase(entry->presentation, Standard_False);
            }
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    int occt_set_objects_display_mode(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int displayMode)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            const auto entries = requireObjects(engine, objectIds, count);
            const int mode = displayMode == OcctDisplay_Wireframe ? AIS_WireFrame : AIS_Shaded;
            for (ObjectEntry* entry : entries)
                engine->viewerContext.context->SetDisplayMode(entry->presentation, mode, Standard_False);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    int occt_set_objects_line_width(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        double width)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            requirePositive(width, "Line width");
            const auto entries = requireObjects(engine, objectIds, count);
            for (ObjectEntry* entry : entries)
                engine->viewerContext.context->SetWidth(entry->presentation, width, Standard_False);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    int occt_set_objects_material(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int material)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            const auto entries = requireObjects(engine, objectIds, count);
            const Graphic3d_MaterialAspect value(materialName(material));
            for (ObjectEntry* entry : entries)
                engine->viewerContext.context->SetMaterial(entry->presentation, value, Standard_False);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    int occt_redisplay_objects(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            const auto entries = requireObjects(engine, objectIds, count);
            for (ObjectEntry* entry : entries)
                engine->viewerContext.context->Redisplay(entry->presentation, Standard_False, Standard_True);
            if (!entries.empty()) engine->requestRedraw();
        });
    }

    int occt_select_objects(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int appendSelection)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            const auto entries = requireObjects(engine, objectIds, count);
            if (appendSelection == 0) engine->viewerContext.context->ClearSelected(Standard_False);
            for (ObjectEntry* entry : entries)
                engine->viewerContext.context->SetSelected(entry->presentation, Standard_False);
            engine->viewerContext.context->HilightSelected(Standard_False);
            engine->requestRedraw();
        });
    }

    int occt_object_is_visible(OcctHandle handle, OcctObjectId objectId)
    {
        Engine* engine = engineOf(handle);
        const ObjectEntry* entry = engine == nullptr ? nullptr : engine->findObject(objectId);
        return engine != nullptr
            && engine->isInitialized()
            && entry != nullptr
            && !entry->presentation.IsNull()
            && engine->viewerContext.context->IsDisplayed(entry->presentation)
            ? 1
            : 0;
    }

    int occt_object_is_selected(OcctHandle handle, OcctObjectId objectId)
    {
        Engine* engine = engineOf(handle);
        const ObjectEntry* entry = engine == nullptr ? nullptr : engine->findObject(objectId);
        return engine != nullptr
            && engine->isInitialized()
            && entry != nullptr
            && !entry->presentation.IsNull()
            && engine->viewerContext.context->IsSelected(entry->presentation)
            ? 1
            : 0;
    }
}

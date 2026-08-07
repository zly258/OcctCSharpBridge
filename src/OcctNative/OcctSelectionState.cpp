#include "OcctInternal.hxx"

using namespace OcctBridge;

namespace
{
    std::vector<ObjectEntry*> requireSelectableObjects(
        Engine* engine,
        const OcctObjectId* objectIds,
        int count)
    {
        if (count < 0) throw std::invalid_argument("Object count must not be negative.");
        if (count > 0 && objectIds == nullptr) throw std::invalid_argument("Object ID array is null.");
        std::vector<ObjectEntry*> result;
        std::vector<OcctObjectId> uniqueIds;
        for (int index = 0; index < count; ++index)
        {
            const OcctObjectId id = objectIds[index];
            if (std::find(uniqueIds.begin(), uniqueIds.end(), id) != uniqueIds.end()) continue;
            ObjectEntry* entry = engine->findObject(id);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            if (!entry->selectable)
                throw std::invalid_argument("A non-selectable object cannot be added to the selection.");
            uniqueIds.push_back(id);
            result.push_back(entry);
        }
        return result;
    }
}

extern "C"
{
    int occt_set_selected_objects_ex(
        OcctHandle handle,
        const OcctObjectId* objectIds,
        int count,
        int operation)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            if (operation < OcctSelection_Replace || operation > OcctSelection_Clear)
                throw std::invalid_argument("Selection operation is out of range.");

            if (operation == OcctSelection_Clear)
            {
                engine->context->ClearSelected(Standard_False);
                engine->context->UpdateCurrentViewer();
                return;
            }

            const auto entries = requireSelectableObjects(engine, objectIds, count);
            if (operation == OcctSelection_Replace)
                engine->context->ClearSelected(Standard_False);

            for (ObjectEntry* entry : entries)
            {
                const bool isSelected = engine->context->IsSelected(entry->presentation);
                switch (operation)
                {
                    case OcctSelection_Replace:
                    case OcctSelection_Add:
                        if (!isSelected) engine->context->SetSelected(entry->presentation, Standard_False);
                        break;
                    case OcctSelection_Remove:
                        if (isSelected) engine->context->AddOrRemoveSelected(entry->presentation, Standard_False);
                        break;
                    case OcctSelection_Toggle:
                        engine->context->AddOrRemoveSelected(entry->presentation, Standard_False);
                        break;
                    default:
                        break;
                }
            }
            engine->context->HilightSelected(Standard_False);
            engine->context->UpdateCurrentViewer();
        });
    }
}

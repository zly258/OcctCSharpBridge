#include "OcctSceneRegistry.hxx"

namespace OcctBridge
{
    ObjectEntry* SceneRegistry::findObject(OcctObjectId id)
    {
        const auto iterator = objects.find(id);
        return iterator == objects.end() ? nullptr : &iterator->second;
    }

    const ObjectEntry* SceneRegistry::findObject(OcctObjectId id) const
    {
        const auto iterator = objects.find(id);
        return iterator == objects.end() ? nullptr : &iterator->second;
    }

    ObjectEntry* SceneRegistry::findShape(OcctObjectId id)
    {
        ObjectEntry* entry = findObject(id);
        return entry != nullptr && entry->kind == OcctObject_Shape && !entry->shape.IsNull() ? entry : nullptr;
    }

    const ObjectEntry* SceneRegistry::findShape(OcctObjectId id) const
    {
        const ObjectEntry* entry = findObject(id);
        return entry != nullptr && entry->kind == OcctObject_Shape && !entry->shape.IsNull() ? entry : nullptr;
    }

    OcctObjectId SceneRegistry::findPresentation(const Handle(AIS_InteractiveObject)& presentation) const
    {
        if (presentation.IsNull()) return 0;
        for (const auto& pair : objects)
        {
            if (pair.second.presentation == presentation) return pair.first;
        }
        return 0;
    }

    OcctObjectId SceneRegistry::allocateId()
    {
        return nextId++;
    }

    void SceneRegistry::clear()
    {
        objects.clear();
        objectIdByApplicationTag.clear();
        nextId = 1;
    }
}

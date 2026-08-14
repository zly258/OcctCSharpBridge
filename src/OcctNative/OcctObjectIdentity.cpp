#include "core/OcctInternal.hxx"

using namespace OcctBridge;

extern "C"
{
    int occt_set_object_application_tag(
        OcctHandle handle,
        OcctObjectId objectId,
        const char* utf8Tag)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            ObjectEntry* entry = engine->findObject(objectId);
            if (entry == nullptr) throw std::invalid_argument("Object ID does not exist.");

            const std::string tag = utf8Tag == nullptr ? std::string() : std::string(utf8Tag);
            if (tag == entry->applicationTag) return;

            if (!tag.empty())
            {
                const auto existing = engine->scene.objectIdByApplicationTag.find(tag);
                if (existing != engine->scene.objectIdByApplicationTag.end()
                    && existing->second != objectId
                    && engine->findObject(existing->second) != nullptr)
                {
                    throw std::invalid_argument("ApplicationTag must be unique within an engine.");
                }
            }

            if (!entry->applicationTag.empty())
                engine->scene.objectIdByApplicationTag.erase(entry->applicationTag);
            entry->applicationTag = tag;
            if (!tag.empty()) engine->scene.objectIdByApplicationTag[tag] = objectId;
        });
    }

    const char* occt_get_object_application_tag(OcctHandle handle, OcctObjectId objectId)
    {
        Engine* engine = engineOf(handle);
        if (engine == nullptr) return "";
        const ObjectEntry* entry = engine->findObject(objectId);
        engine->errors.scratch = entry == nullptr ? std::string() : entry->applicationTag;
        return engine->errors.scratch.c_str();
    }

    OcctObjectId occt_find_object_by_application_tag(OcctHandle handle, const char* utf8Tag)
    {
        Engine* engine = engineOf(handle);
        if (engine == nullptr || utf8Tag == nullptr || *utf8Tag == '\0') return 0;
        const auto iterator = engine->scene.objectIdByApplicationTag.find(utf8Tag);
        if (iterator == engine->scene.objectIdByApplicationTag.end()) return 0;
        if (engine->findObject(iterator->second) == nullptr)
        {
            engine->scene.objectIdByApplicationTag.erase(iterator);
            return 0;
        }
        return iterator->second;
    }
}

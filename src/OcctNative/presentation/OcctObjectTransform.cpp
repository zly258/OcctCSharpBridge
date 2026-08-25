#include "presentation/OcctObjectTransform.h"
#include "core/OcctInternal.hxx"

#include <cmath>
#include <stdexcept>
#include <unordered_set>
#include <utility>
#include <vector>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeTransformStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    ObjectEntry& requiredObject(Engine* engine, OcctObjectId objectId)
    {
        ObjectEntry* entry = engine->findObject(objectId);
        if (entry == nullptr || entry->presentation.IsNull())
            throw std::invalid_argument("Object ID does not exist.");
        return *entry;
    }

    void validateTransformation(const OcctTransform3d& value)
    {
        const double values[] = {
            value.m00, value.m01, value.m02, value.m03,
            value.m10, value.m11, value.m12, value.m13,
            value.m20, value.m21, value.m22, value.m23 };
        for (double item : values)
        {
            if (!std::isfinite(item))
                throw std::invalid_argument("Transformation matrix must contain only finite values.");
        }
    }

    gp_Trsf transformFromValue(const OcctTransform3d& value)
    {
        validateTransformation(value);
        gp_Trsf transform;
        transform.SetValues(
            value.m00, value.m01, value.m02, value.m03,
            value.m10, value.m11, value.m12, value.m13,
            value.m20, value.m21, value.m22, value.m23);
        return transform;
    }

    OcctTransform3d transformToValue(const gp_Trsf& transform)
    {
        return {
            transform.Value(1, 1), transform.Value(1, 2), transform.Value(1, 3), transform.Value(1, 4),
            transform.Value(2, 1), transform.Value(2, 2), transform.Value(2, 3), transform.Value(2, 4),
            transform.Value(3, 1), transform.Value(3, 2), transform.Value(3, 3), transform.Value(3, 4) };
    }

    void setTransformation(Engine* engine, ObjectEntry& entry, const OcctTransform3d& value)
    {
        const gp_Trsf transformation = transformFromValue(value);
        entry.presentation->SetLocalTransformation(transformation);
        engine->viewerContext.context->Redisplay(entry.presentation, Standard_False, Standard_True);
    }
}

namespace OcctBridge
{
    TopoDS_Shape shapeWithPresentationTransformation(const ObjectEntry& entry)
    {
        if (entry.shape.IsNull() || entry.presentation.IsNull()) return entry.shape;
        const gp_Trsf transformation = entry.presentation->LocalTransformation();
        if (transformation.Form() == gp_Identity) return entry.shape;
        return transformed(entry.shape, transformation);
    }
}

extern "C"
{
    OcctStatus occt_engine_object_transform_set(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        const OcctTransform3d* transformation)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeTransformStatus(engine, [&]
        {
            if (transformation == nullptr)
                throw std::invalid_argument("Transformation is null.");
            ObjectEntry& entry = requiredObject(engine, objectId);
            setTransformation(engine, entry, *transformation);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_transform_get(
        OcctEngineHandle handle,
        OcctObjectId objectId,
        OcctTransform3d* transformation,
        int* hasTransformation)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeTransformStatus(engine, [&]
        {
            if (transformation == nullptr || hasTransformation == nullptr)
                throw std::invalid_argument("Transformation output is null.");
            const ObjectEntry& entry = requiredObject(engine, objectId);
            const gp_Trsf value = entry.presentation->LocalTransformation();
            *transformation = transformToValue(value);
            *hasTransformation = value.Form() == gp_Identity ? 0 : 1;
        });
    }

    OcctStatus occt_engine_object_transform_reset(
        OcctEngineHandle handle,
        OcctObjectId objectId)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeTransformStatus(engine, [&]
        {
            ObjectEntry& entry = requiredObject(engine, objectId);
            entry.presentation->ResetTransformation();
            engine->viewerContext.context->Redisplay(entry.presentation, Standard_False, Standard_True);
            engine->requestRedraw();
        });
    }

    OcctStatus occt_engine_object_transforms_set(
        OcctEngineHandle handle,
        const OcctViewerObjectTransformUpdate* updates,
        int count)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeTransformStatus(engine, [&]
        {
            if (count < 0) throw std::invalid_argument("Transformation update count must not be negative.");
            if (count > 0 && updates == nullptr)
                throw std::invalid_argument("Transformation update array is null.");

            std::vector<ObjectEntry*> entries;
            entries.reserve(static_cast<std::size_t>(count));
            std::unordered_set<OcctObjectId> seen;
            seen.reserve(static_cast<std::size_t>(count));
            for (int index = 0; index < count; ++index)
            {
                const auto& update = updates[index];
                if (!seen.insert(update.objectId).second)
                    throw std::invalid_argument("Transformation update contains duplicate object IDs.");
                validateTransformation(update.transformation);
                entries.push_back(&requiredObject(engine, update.objectId));
            }

            for (int index = 0; index < count; ++index)
                setTransformation(engine, *entries[static_cast<std::size_t>(index)], updates[index].transformation);
            if (count > 0) engine->requestRedraw();
        });
    }
}

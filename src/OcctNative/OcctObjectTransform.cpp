#include "core/OcctInternal.hxx"
#include "OcctViewerInteraction.h"

using namespace OcctBridge;

namespace
{
    gp_Trsf transformFromMatrix(const double* matrix)
    {
        if (matrix == nullptr) throw std::invalid_argument("Transformation matrix is null.");
        gp_Trsf transform;
        transform.SetValues(
            matrix[0], matrix[1], matrix[2], matrix[3],
            matrix[4], matrix[5], matrix[6], matrix[7],
            matrix[8], matrix[9], matrix[10], matrix[11]);
        return transform;
    }

    gp_Trsf transformFromValue(const OcctTransform3d& value)
    {
        const double matrix[12] =
        {
            value.m00, value.m01, value.m02, value.m03,
            value.m10, value.m11, value.m12, value.m13,
            value.m20, value.m21, value.m22, value.m23
        };
        return transformFromMatrix(matrix);
    }

    void writeMatrix(const gp_Trsf& transform, double* matrix)
    {
        if (matrix == nullptr) throw std::invalid_argument("Transformation result matrix is null.");
        int index = 0;
        for (int row = 1; row <= 3; ++row)
            for (int column = 1; column <= 4; ++column)
                matrix[index++] = transform.Value(row, column);
    }
}

namespace OcctBridge
{
    TopoDS_Shape shapeWithPresentationTransformation(const ObjectEntry& entry)
    {
        if (entry.shape.IsNull()) return {};
        if (entry.presentation.IsNull() || !entry.presentation->HasTransformation())
            return entry.shape;
        return transformed(entry.shape, entry.presentation->LocalTransformation());
    }
}

extern "C"
{
    int occt_set_object_transform(
        OcctHandle handle,
        OcctObjectId objectId,
        const double* matrix3x4)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            ObjectEntry* entry = engine->findObject(objectId);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            if (entry->kind == OcctObject_Shape) engine->invalidatePristineStepDocument();
            entry->presentation->SetLocalTransformation(transformFromMatrix(matrix3x4));
            entry->presentation->UpdateTransformation();
            engine->viewerContext.context->RecomputeSelectionOnly(entry->presentation);
            engine->requestRedraw();
        });
    }

    int occt_get_object_transform(
        OcctHandle handle,
        OcctObjectId objectId,
        double* matrix3x4,
        int* hasTransform)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            if (hasTransform == nullptr) throw std::invalid_argument("hasTransform result is null.");
            const ObjectEntry* entry = engine->findObject(objectId);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            *hasTransform = entry->presentation->HasTransformation() ? 1 : 0;
            writeMatrix(entry->presentation->LocalTransformation(), matrix3x4);
        });
    }

    int occt_reset_object_transform(OcctHandle handle, OcctObjectId objectId)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            ObjectEntry* entry = engine->findObject(objectId);
            if (entry == nullptr || entry->presentation.IsNull())
                throw std::invalid_argument("Object ID does not exist.");
            if (entry->kind == OcctObject_Shape) engine->invalidatePristineStepDocument();
            entry->presentation->ResetTransformation();
            entry->presentation->UpdateTransformation();
            engine->viewerContext.context->RecomputeSelectionOnly(entry->presentation);
            engine->requestRedraw();
        });
    }

    int occt_set_object_transforms(
        OcctHandle handle,
        const OcctObjectTransformUpdate* updates,
        int count)
    {
        Engine* engine = engineOf(handle); if (!validateInitialized(engine)) return 0;
        return execute(engine, [&]
        {
            if (count < 0) throw std::invalid_argument("Transformation update count must not be negative.");
            if (count > 0 && updates == nullptr)
                throw std::invalid_argument("Transformation update array is null.");

            struct PreparedUpdate
            {
                ObjectEntry* entry;
                gp_Trsf transformation;
            };

            std::vector<PreparedUpdate> prepared;
            prepared.reserve(static_cast<std::size_t>(count));
            bool containsShape = false;
            for (int index = 0; index < count; ++index)
            {
                ObjectEntry* entry = engine->findObject(updates[index].objectId);
                if (entry == nullptr || entry->presentation.IsNull())
                    throw std::invalid_argument("Object ID does not exist.");
                prepared.push_back({entry, transformFromValue(updates[index].transformation)});
                containsShape = containsShape || entry->kind == OcctObject_Shape;
            }

            if (containsShape) engine->invalidatePristineStepDocument();
            for (const PreparedUpdate& update : prepared)
            {
                update.entry->presentation->SetLocalTransformation(update.transformation);
                update.entry->presentation->UpdateTransformation();
                engine->viewerContext.context->RecomputeSelectionOnly(update.entry->presentation);
            }
            if (!prepared.empty()) engine->requestRedraw();
        });
    }
}

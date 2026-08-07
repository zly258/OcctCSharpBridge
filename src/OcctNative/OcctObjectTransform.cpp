#include "OcctInternal.hxx"

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
            entry->presentation->SetLocalTransformation(transformFromMatrix(matrix3x4));
            entry->presentation->UpdateTransformation();
            engine->context->RecomputeSelectionOnly(entry->presentation);
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
            entry->presentation->ResetTransformation();
            entry->presentation->UpdateTransformation();
            engine->context->RecomputeSelectionOnly(entry->presentation);
            engine->requestRedraw();
        });
    }
}

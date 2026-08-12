#include "OcctModelingSessionInternal.hxx"

#include <BRepBuilderAPI_Copy.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctModelHandle occt_model_create()
    {
        try { return new ModelSession(); }
        catch (...) { return nullptr; }
    }

    void occt_model_destroy(OcctModelHandle handle)
    {
        delete modelOf(handle);
    }

    const char* occt_model_last_error(OcctModelHandle handle)
    {
        ModelSession* model = modelOf(handle);
        return model == nullptr ? "Invalid OCCT modeling handle." : model->lastError.c_str();
    }

    const char* occt_model_capabilities()
    {
        return "headless;geometry-query;analytic-geometry;differential-geometry;topology;topology-reference;history;inertia;intersection;healing;mesh;projection;ray-intersection;classification;advanced-boolean;splitter;sweep;loft;step;iges;brep;stl;viewer-interop";
    }

    int occt_model_shape_ids_copy(OcctModelHandle handle, OcctObjectId* results, int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;
        int copied = 0;
        if (execute(model, [&]
        {
            if (capacity < 0) throw std::invalid_argument("Shape-ID buffer capacity must not be negative.");
            const int count = static_cast<int>(model->shapes.size());
            if (results == nullptr)
            {
                if (capacity != 0) throw std::invalid_argument("Null shape-ID buffer requires zero capacity.");
                copied = count;
                return;
            }
            if (capacity < count) throw std::invalid_argument("Shape-ID buffer capacity is smaller than the result count.");

            int index = 0;
            for (const auto& pair : model->shapes)
                results[index++] = pair.first;
            copied = count;
        }) == 0)
            return -1;
        return copied;
    }

    int occt_model_shape_exists(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        return model != nullptr && model->shapes.find(shapeId) != model->shapes.end() ? 1 : 0;
    }

    int occt_model_delete_shape(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        return execute(model, [&]
        {
            if (model->shapes.erase(shapeId) == 0) throw std::invalid_argument("Shape ID does not exist.");
        });
    }

    int occt_model_clear(OcctModelHandle handle)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        return execute(model, [&]
        {
            model->shapes.clear();
            model->operations.clear();
            model->rayHits.clear();
        });
    }

    const char* occt_model_operation_report(OcctModelHandle handle, OcctOperationId operationId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return "Invalid OCCT modeling handle.";
        model->scratchString.clear();
        execute(model, [&] { model->scratchString = requireOperation(model, operationId).report; });
        return model->scratchString.c_str();
    }

    OcctObjectId occt_model_copy_shape(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            BRepBuilderAPI_Copy copy(model->requireShape(shapeId), Standard_True, Standard_True);
            if (!copy.IsDone()) throw std::runtime_error("Shape copy failed.");
            return copy.Shape();
        });
    }
}

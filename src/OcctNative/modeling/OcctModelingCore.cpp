#include "modeling/OcctModelingSession.h"
#include "modeling/OcctModelingSessionInternal.hxx"

#include <BRepBuilderAPI_Copy.hxx>

#include <cstring>
#include <limits>
#include <string>

using namespace OcctModelingInternal;

namespace
{
    constexpr char Capabilities[] =
        "headless;geometry-query;analytic-geometry;differential-geometry;topology;"
        "topology-reference;history;inertia;intersection;healing;mesh;projection;"
        "ray-intersection;classification;advanced-boolean;splitter;sweep;loft;"
        "step;iges;brep;stl;xde-document;viewer-interop";

    OcctStatus copyUtf8(
        const std::string& value,
        char* buffer,
        int capacity,
        int* required)
    {
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;
        if (value.size() >= static_cast<std::size_t>(std::numeric_limits<int>::max()))
            return OcctStatus_ErrorOutOfMemory;

        const int size = static_cast<int>(value.size()) + 1;
        *required = size;
        if (buffer == nullptr)
            return capacity == 0 ? OcctStatus_Ok : OcctStatus_ErrorInvalidArgument;
        if (capacity < size) return OcctStatus_ErrorBufferTooSmall;

        std::memcpy(buffer, value.c_str(), static_cast<std::size_t>(size));
        return OcctStatus_Ok;
    }
}

extern "C"
{
    OcctModelingSessionHandle occt_model_session_create()
    {
        try
        {
            return reinterpret_cast<OcctModelingSessionHandle>(new ModelSession());
        }
        catch (...)
        {
            return nullptr;
        }
    }

    void occt_model_session_destroy(OcctModelingSessionHandle handle)
    {
        delete sessionOf(handle);
    }

    OcctStatus occt_model_session_last_error_code(OcctModelingSessionHandle handle)
    {
        const ModelSession* model = reinterpret_cast<const ModelSession*>(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        const std::lock_guard<std::recursive_mutex> guard(model->mutex);
        return model->errorContext().code;
    }

    OcctStatus occt_model_session_last_error_message(
        OcctModelingSessionHandle handle,
        char* buffer,
        int capacity,
        int* required)
    {
        const ModelSession* model = reinterpret_cast<const ModelSession*>(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        const std::lock_guard<std::recursive_mutex> guard(model->mutex);
        return copyUtf8(model->errorContext().message, buffer, capacity, required);
    }

    OcctStatus occt_model_capabilities_get(
        char* buffer,
        int capacity,
        int* required)
    {
        return copyUtf8(Capabilities, buffer, capacity, required);
    }

    OcctStatus occt_model_shapes_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            if (model->shapes.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
                throw std::length_error("Shape registry exceeds the ABI buffer size limit.");

            const int count = static_cast<int>(model->shapes.size());
            *required = count;
            if (results == nullptr)
            {
                if (capacity != 0) throw std::invalid_argument("Null shape-ID buffer requires zero capacity.");
                return;
            }
            if (capacity < count)
                throw std::invalid_argument("Shape-ID buffer capacity is smaller than the result count.");

            int index = 0;
            for (const auto& pair : model->shapes)
                results[index++] = pair.first;
        });
    }

    OcctStatus occt_model_shape_exists_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBool* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        const std::lock_guard<std::recursive_mutex> guard(model->mutex);
        model->errorContext().clear();
        *result = model->shapes.find(shapeId) != model->shapes.end() ? 1 : 0;
        return OcctStatus_Ok;
    }

    OcctStatus occt_model_shape_delete(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId)
    {
        ModelSession* model = sessionOf(handle);
        return executeStatus(model, [&]
        {
            if (model->shapes.erase(shapeId) == 0)
                throw std::invalid_argument("Shape ID does not exist.");
        });
    }

    OcctStatus occt_model_session_clear(OcctModelingSessionHandle handle)
    {
        ModelSession* model = sessionOf(handle);
        return executeStatus(model, [&]
        {
            model->shapes.clear();
            model->operations.clear();
            model->rayHits.clear();
            model->edgeIntersections.clear();
            model->lastXdeDocument.Nullify();
            model->lastXdeLeafShapeIds.clear();
            model->lastXdeSourceFormat.clear();
        });
    }

    OcctStatus occt_model_operation_report_get(
        OcctModelingSessionHandle handle,
        std::int64_t operationId,
        char* buffer,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        std::string report;
        const OcctStatus status = executeStatus(model, [&]
        {
            report = requireOperation(model, static_cast<OcctOperationId>(operationId)).report;
        });
        if (status != OcctStatus_Ok) return status;
        return copyUtf8(report, buffer, capacity, required);
    }

    OcctStatus occt_model_shape_copy(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            BRepBuilderAPI_Copy copy(model->requireShape(shapeId), Standard_True, Standard_True);
            if (!copy.IsDone()) throw std::runtime_error("Shape copy failed.");
            return copy.Shape();
        });
    }
}

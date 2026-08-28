#pragma once

#include "core/OcctErrorContext.hxx"
#include "modeling/OcctModeling.h"
#include "modeling/OcctModelingIntersection.h"

#include <BRepTools_History.hxx>
#include <Standard_Failure.hxx>
#include <TopoDS_Shape.hxx>

#include <mutex>
#include <new>
#include <stdexcept>
#include <string>
#include <thread>
#include <unordered_map>
#include <utility>
#include <vector>

namespace OcctModelingInternal
{
    struct HistoryLineage
    {
        std::vector<OcctObjectId> generated;
        std::vector<OcctObjectId> modified;
        bool removed = false;
        bool materialized = false;
    };

    struct OperationRecord
    {
        Handle(BRepTools_History) history;
        std::string report;
        bool hasWarnings = false;
        bool hasErrors = false;
        std::unordered_map<OcctObjectId, HistoryLineage> lineageBySource;

        OperationRecord() = default;

        OperationRecord(
            const Handle(BRepTools_History)& operationHistory,
            std::string operationReport,
            bool operationHasWarnings,
            bool operationHasErrors,
            std::unordered_map<OcctObjectId, HistoryLineage> operationLineageBySource = {})
            : history(operationHistory),
              report(std::move(operationReport)),
              hasWarnings(operationHasWarnings),
              hasErrors(operationHasErrors),
              lineageBySource(std::move(operationLineageBySource))
        {
        }
    };

    struct ModelSession
    {
        mutable std::recursive_mutex mutex;
        mutable std::unordered_map<std::thread::id, OcctBridge::ErrorContext> errorsByThread;
        std::unordered_map<OcctObjectId, TopoDS_Shape> shapes;
        std::unordered_map<OcctOperationId, OperationRecord> operations;
        std::vector<OcctModelRayHit> rayHits;
        std::vector<OcctModelEdgeIntersection> edgeIntersections;
        OcctObjectId nextShapeId = 1;
        OcctOperationId nextOperationId = 1;

        OcctBridge::ErrorContext& errorContext() const
        {
            return errorsByThread[std::this_thread::get_id()];
        }

        TopoDS_Shape& requireShape(OcctObjectId id)
        {
            const auto iterator = shapes.find(id);
            if (iterator == shapes.end() || iterator->second.IsNull())
                throw std::invalid_argument("Shape ID does not exist.");
            return iterator->second;
        }

        const TopoDS_Shape& requireShape(OcctObjectId id) const
        {
            const auto iterator = shapes.find(id);
            if (iterator == shapes.end() || iterator->second.IsNull())
                throw std::invalid_argument("Shape ID does not exist.");
            return iterator->second;
        }

        OcctObjectId addShape(const TopoDS_Shape& shape)
        {
            if (shape.IsNull()) throw std::runtime_error("OCCT returned a null shape.");
            const OcctObjectId id = nextShapeId++;
            shapes.emplace(id, shape);
            return id;
        }

        OcctOperationId addOperation(
            const Handle(BRepTools_History)& history,
            std::string report,
            bool hasWarnings,
            bool hasErrors)
        {
            const OcctOperationId id = nextOperationId++;
            operations.emplace(id, OperationRecord{history, std::move(report), hasWarnings, hasErrors});
            return id;
        }
    };

    inline ModelSession* sessionOf(OcctModelingSessionHandle handle)
    {
        return reinterpret_cast<ModelSession*>(handle);
    }

    template<typename Function>
    inline int execute(ModelSession* model, Function&& function)
    {
        if (model == nullptr) return 0;
        const std::lock_guard<std::recursive_mutex> guard(model->mutex);
        model->errorContext().clear();
        try
        {
            function();
            return 1;
        }
        catch (const Standard_Failure& failure)
        {
            const char* message = failure.GetMessageString();
            model->errorContext().set(OcctStatus_ErrorOcct, message == nullptr ? "Open CASCADE operation failed." : message);
        }
        catch (const std::invalid_argument& exception)
        {
            model->errorContext().set(OcctStatus_ErrorInvalidArgument, exception.what());
        }
        catch (const std::logic_error& exception)
        {
            model->errorContext().set(OcctStatus_ErrorInvalidState, exception.what());
        }
        catch (const std::bad_alloc&)
        {
            model->errorContext().set(OcctStatus_ErrorOutOfMemory, "Native memory allocation failed.");
        }
        catch (const std::exception& exception)
        {
            model->errorContext().set(OcctStatus_ErrorUnknown, exception.what());
        }
        catch (...)
        {
            model->errorContext().set(OcctStatus_ErrorUnknown, "Unknown native modeling error.");
        }
        return 0;
    }

    template<typename Function>
    inline OcctStatus executeStatus(ModelSession* model, Function&& function)
    {
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        const std::lock_guard<std::recursive_mutex> guard(model->mutex);
        return execute(model, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : model->errorContext().code;
    }

    template<typename Result, typename Function>
    inline Result executeValue(ModelSession* model, Result fallback, Function&& function)
    {
        Result result = fallback;
        execute(model, [&] { result = function(); });
        return result;
    }

    template<typename Function>
    inline OcctObjectId executeShape(ModelSession* model, Function&& function)
    {
        OcctObjectId result = 0;
        execute(model, [&] { result = model->addShape(function()); });
        return result;
    }

    template<typename Function>
    inline OcctStatus executeShapeStatus(
        ModelSession* model,
        OcctObjectId* result,
        Function&& function)
    {
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = 0;
        return executeStatus(model, [&]
        {
            *result = model->addShape(function());
        });
    }

    inline OperationRecord& requireOperation(ModelSession* model, OcctOperationId operationId)
    {
        const auto iterator = model->operations.find(operationId);
        if (iterator == model->operations.end())
            throw std::invalid_argument("Operation ID does not exist.");
        return iterator->second;
    }

    inline const OperationRecord& requireOperation(const ModelSession* model, OcctOperationId operationId)
    {
        const auto iterator = model->operations.find(operationId);
        if (iterator == model->operations.end())
            throw std::invalid_argument("Operation ID does not exist.");
        return iterator->second;
    }
}

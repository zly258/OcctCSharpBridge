#pragma once

#include "OcctModeling.h"
#include "OcctModelingIntersection.h"

#include <BRepTools_History.hxx>
#include <Standard_Failure.hxx>
#include <TopoDS_Shape.hxx>

#include <stdexcept>
#include <string>
#include <unordered_map>
#include <utility>
#include <vector>

namespace OcctModelingInternal
{
    struct OperationRecord
    {
        Handle(BRepTools_History) history;
        std::string report;
        bool hasWarnings = false;
        bool hasErrors = false;
    };

    struct ModelSession
    {
        std::string lastError;
        std::string scratchString;
        std::unordered_map<OcctObjectId, TopoDS_Shape> shapes;
        std::unordered_map<OcctOperationId, OperationRecord> operations;
        std::vector<OcctModelRayHit> rayHits;
        std::vector<OcctModelEdgeIntersection> edgeIntersections;
        OcctObjectId nextShapeId = 1;
        OcctOperationId nextOperationId = 1;

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

    inline ModelSession* modelOf(OcctModelHandle handle)
    {
        return static_cast<ModelSession*>(handle);
    }

    template<typename Function>
    inline int execute(ModelSession* model, Function&& function)
    {
        if (model == nullptr) return 0;
        model->lastError.clear();
        try
        {
            function();
            return 1;
        }
        catch (const Standard_Failure& failure)
        {
            const char* message = failure.GetMessageString();
            model->lastError = message == nullptr ? "Open CASCADE operation failed." : message;
        }
        catch (const std::exception& exception)
        {
            model->lastError = exception.what();
        }
        catch (...)
        {
            model->lastError = "Unknown native modeling error.";
        }
        return 0;
    }

    template<typename Function>
    inline OcctObjectId executeShape(ModelSession* model, Function&& function)
    {
        OcctObjectId result = 0;
        execute(model, [&] { result = model->addShape(function()); });
        return result;
    }

    inline const OperationRecord& requireOperation(ModelSession* model, OcctOperationId operationId)
    {
        const auto iterator = model->operations.find(operationId);
        if (iterator == model->operations.end())
            throw std::invalid_argument("Operation ID does not exist.");
        return iterator->second;
    }
}

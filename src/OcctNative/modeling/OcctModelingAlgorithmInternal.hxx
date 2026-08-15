#pragma once

#include "modeling/OcctModelingShapeInternal.hxx"

#include <BOPAlgo_GlueEnum.hxx>
#include <TopTools_ListIteratorOfListOfShape.hxx>

#include <sstream>
#include <stdexcept>
#include <string>

namespace OcctModelingInternal
{
    inline OcctModelAlgorithmResult failedAlgorithmResult()
    {
        return {0, 0, 0, 0, 1};
    }

    template<typename Function>
    inline OcctStatus executeAlgorithmStatus(
        ModelSession* model,
        OcctModelAlgorithmResult* result,
        Function&& function)
    {
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = failedAlgorithmResult();
        return executeStatus(model, [&]
        {
            *result = function();
        });
    }

    inline BOPAlgo_GlueEnum glueValue(int value)
    {
        switch (value)
        {
            case OcctModelGlue_Shift: return BOPAlgo_GlueShift;
            case OcctModelGlue_Full: return BOPAlgo_GlueFull;
            default: return BOPAlgo_GlueOff;
        }
    }

    template<typename Algorithm>
    inline void applyBooleanOptions(Algorithm& algorithm, const OcctModelBooleanOptions* options)
    {
        if (options == nullptr) return;
        if (options->fuzzyValue < 0.0) throw std::invalid_argument("Fuzzy value must not be negative.");
        algorithm.SetFuzzyValue(options->fuzzyValue);
        algorithm.SetRunParallel(options->runParallel != 0);
        algorithm.SetNonDestructive(options->nonDestructive != 0);
        algorithm.SetGlue(glueValue(options->glue));
        algorithm.SetCheckInverted(options->checkInverted != 0);
    }

    template<typename Algorithm>
    inline std::string algorithmReport(Algorithm& algorithm)
    {
        std::ostringstream stream;
        algorithm.DumpErrors(stream);
        algorithm.DumpWarnings(stream);
        return stream.str();
    }

    template<typename Algorithm>
    inline OcctModelAlgorithmResult finishBuilderAlgorithm(
        ModelSession* model,
        Algorithm& algorithm,
        const OcctModelBooleanOptions* options)
    {
        algorithm.Build();
        const bool hasErrors = algorithm.HasErrors();
        const bool hasWarnings = algorithm.HasWarnings();
        const std::string report = algorithmReport(algorithm);
        if (!algorithm.IsDone() || hasErrors || algorithm.Shape().IsNull())
            throw std::runtime_error(report.empty() ? "OCCT modeling algorithm failed." : report);

        if (options != nullptr && (options->simplifyEdges != 0 || options->simplifyFaces != 0))
        {
            const double angularTolerance = options->angularTolerance > 0.0
                ? options->angularTolerance
                : Precision::Angular();
            algorithm.SimplifyResult(
                options->simplifyEdges != 0,
                options->simplifyFaces != 0,
                angularTolerance);
        }

        const OcctObjectId shapeId = model->addShape(algorithm.Shape());
        const OcctOperationId operationId = model->addOperation(
            algorithm.History(),
            report,
            hasWarnings,
            hasErrors);
        return {shapeId, operationId, 1, hasWarnings ? 1 : 0, hasErrors ? 1 : 0};
    }

    template<typename Algorithm>
    inline OcctModelAlgorithmResult finishMakeShapeAlgorithm(
        ModelSession* model,
        Algorithm& algorithm,
        const TopTools_ListOfShape& arguments,
        const char* failureMessage)
    {
        algorithm.Build();
        if (!algorithm.IsDone() || algorithm.Shape().IsNull())
            throw std::runtime_error(failureMessage);

        Handle(BRepTools_History) history = new BRepTools_History(arguments, algorithm);
        const OcctObjectId shapeId = model->addShape(algorithm.Shape());
        const OcctOperationId operationId = model->addOperation(history, {}, false, false);
        return {shapeId, operationId, 1, 0, 0};
    }

    inline HistoryLineage& materializeHistoryLineage(
        ModelSession* model,
        OcctOperationId operationId,
        OcctObjectId sourceId)
    {
        OperationRecord& operation = requireOperation(model, operationId);
        HistoryLineage& lineage = operation.lineageBySource[sourceId];
        const auto idsAreAlive = [&](const std::vector<OcctObjectId>& ids)
        {
            for (const OcctObjectId id : ids)
            {
                const auto iterator = model->shapes.find(id);
                if (iterator == model->shapes.end() || iterator->second.IsNull()) return false;
            }
            return true;
        };
        if (lineage.materialized && idsAreAlive(lineage.generated) && idsAreAlive(lineage.modified))
            return lineage;

        lineage.generated.clear();
        lineage.modified.clear();
        lineage.removed = false;
        const TopoDS_Shape& source = model->requireShape(sourceId);
        if (!operation.history.IsNull())
        {
            const auto append = [&](const TopTools_ListOfShape& shapes, std::vector<OcctObjectId>& ids)
            {
                ids.reserve(static_cast<std::size_t>(shapes.Size()));
                for (TopTools_ListIteratorOfListOfShape iterator(shapes); iterator.More(); iterator.Next())
                    ids.push_back(model->addShape(iterator.Value()));
            };
            append(operation.history->Generated(source), lineage.generated);
            append(operation.history->Modified(source), lineage.modified);
            lineage.removed = operation.history->IsRemoved(source);
        }
        lineage.materialized = true;
        return lineage;
    }

    inline int historyCopy(
        ModelSession* model,
        OcctOperationId operationId,
        OcctObjectId sourceId,
        bool generated,
        OcctObjectId* results,
        int capacity)
    {
        if (capacity < 0) throw std::invalid_argument("History buffer capacity must not be negative.");
        HistoryLineage& lineage = materializeHistoryLineage(model, operationId, sourceId);
        const std::vector<OcctObjectId>& ids = generated ? lineage.generated : lineage.modified;
        const int count = static_cast<int>(ids.size());
        if (results == nullptr)
        {
            if (capacity != 0) throw std::invalid_argument("Null history buffer requires zero capacity.");
            return count;
        }
        if (capacity < count) throw std::invalid_argument("History buffer capacity is smaller than the result count.");
        std::copy(ids.begin(), ids.end(), results);
        return count;
    }
}

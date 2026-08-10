#pragma once

#include "OcctModelingShapeInternal.hxx"

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

    inline OcctObjectId historyShapeAt(
        ModelSession* model,
        OcctOperationId operationId,
        OcctObjectId sourceId,
        int index,
        bool generated)
    {
        if (index < 0) throw std::out_of_range("History index must not be negative.");
        const OperationRecord& operation = requireOperation(model, operationId);
        if (operation.history.IsNull())
            throw std::runtime_error("The operation has no topology history.");

        const TopoDS_Shape& source = model->requireShape(sourceId);
        const auto& list = generated
            ? operation.history->Generated(source)
            : operation.history->Modified(source);
        int current = 0;
        for (TopTools_ListIteratorOfListOfShape iterator(list); iterator.More(); iterator.Next(), ++current)
        {
            if (current == index) return model->addShape(iterator.Value());
        }
        throw std::out_of_range("History index is out of range.");
    }

    inline int historyCount(
        ModelSession* model,
        OcctOperationId operationId,
        OcctObjectId sourceId,
        bool generated)
    {
        const OperationRecord& operation = requireOperation(model, operationId);
        if (operation.history.IsNull()) return 0;
        const TopoDS_Shape& source = model->requireShape(sourceId);
        return generated
            ? operation.history->Generated(source).Size()
            : operation.history->Modified(source).Size();
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
        const OperationRecord& operation = requireOperation(model, operationId);
        if (operation.history.IsNull()) return 0;

        const TopoDS_Shape& source = model->requireShape(sourceId);
        const auto& list = generated
            ? operation.history->Generated(source)
            : operation.history->Modified(source);
        const int count = list.Size();
        if (capacity < count) throw std::invalid_argument("History buffer capacity is smaller than the result count.");
        if (count > 0 && results == nullptr) throw std::invalid_argument("History result buffer is null.");

        int index = 0;
        for (TopTools_ListIteratorOfListOfShape iterator(list); iterator.More(); iterator.Next(), ++index)
            results[index] = model->addShape(iterator.Value());
        return count;
    }
}

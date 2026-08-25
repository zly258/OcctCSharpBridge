#include "modeling/OcctModelingBoolean.h"
#include "modeling/OcctModelingAlgorithmInternal.hxx"

#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepAlgoAPI_Splitter.hxx>
#include <BOPAlgo_Builder.hxx>
#include <BOPAlgo_CellsBuilder.hxx>

using namespace OcctModelingInternal;

namespace
{
    TopTools_ListOfShape optionalShapeList(
        ModelSession* model,
        const OcctObjectId* ids,
        int count,
        const char* name)
    {
        if (count < 0) throw std::invalid_argument(std::string(name) + " count must not be negative.");
        if (count > 0 && ids == nullptr) throw std::invalid_argument(std::string(name) + " array is null.");
        TopTools_ListOfShape result;
        for (int index = 0; index < count; ++index) result.Append(model->requireShape(ids[index]));
        return result;
    }

    template<typename Algorithm>
    OcctModelAlgorithmResult finishPerformedBop(
        ModelSession* model,
        Algorithm& algorithm)
    {
        const bool hasErrors = algorithm.HasErrors();
        const bool hasWarnings = algorithm.HasWarnings();
        const std::string report = algorithmReport(algorithm);
        if (hasErrors || algorithm.Shape().IsNull())
            throw std::runtime_error(report.empty() ? "OCCT BOPAlgo operation failed." : report);

        const OcctObjectId shapeId = model->addShape(algorithm.Shape());
        const OcctOperationId operationId = model->addOperation(
            algorithm.History(),
            report,
            hasWarnings,
            hasErrors);
        return {shapeId, operationId, 1, hasWarnings ? 1 : 0, hasErrors ? 1 : 0};
    }
}

extern "C"
{
    OcctStatus occt_model_boolean_execute(
        OcctModelingSessionHandle handle,
        int operation,
        OcctObjectId leftId,
        OcctObjectId rightId,
        const OcctModelBooleanOptions* options,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            if (operation < OcctModelBoolean_Fuse || operation > OcctModelBoolean_Section)
                throw std::invalid_argument("Boolean operation is invalid.");

            const TopoDS_Shape& left = model->requireShape(leftId);
            const TopoDS_Shape& right = model->requireShape(rightId);
            TopTools_ListOfShape arguments;
            arguments.Append(left);
            TopTools_ListOfShape tools;
            tools.Append(right);

            switch (operation)
            {
                case OcctModelBoolean_Cut:
                {
                    BRepAlgoAPI_Cut algorithm;
                    algorithm.SetArguments(arguments);
                    algorithm.SetTools(tools);
                    applyBooleanOptions(algorithm, options);
                    return finishBuilderAlgorithm(model, algorithm, options);
                }
                case OcctModelBoolean_Common:
                {
                    BRepAlgoAPI_Common algorithm;
                    algorithm.SetArguments(arguments);
                    algorithm.SetTools(tools);
                    applyBooleanOptions(algorithm, options);
                    return finishBuilderAlgorithm(model, algorithm, options);
                }
                case OcctModelBoolean_Section:
                {
                    BRepAlgoAPI_Section algorithm(left, right, Standard_False);
                    applyBooleanOptions(algorithm, options);
                    return finishBuilderAlgorithm(model, algorithm, options);
                }
                case OcctModelBoolean_Fuse:
                default:
                {
                    BRepAlgoAPI_Fuse algorithm;
                    algorithm.SetArguments(arguments);
                    algorithm.SetTools(tools);
                    applyBooleanOptions(algorithm, options);
                    return finishBuilderAlgorithm(model, algorithm, options);
                }
            }
        });
    }

    OcctStatus occt_model_boolean_general_fuse_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* shapeIds,
        int shapeCount,
        const OcctModelBooleanOptions* options,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            BOPAlgo_Builder algorithm;
            algorithm.SetArguments(shapeList(model, shapeIds, shapeCount, "General Fuse arguments"));
            applyBooleanOptions(algorithm, options);
            algorithm.Perform();
            return finishPerformedBop(model, algorithm);
        });
    }

    OcctStatus occt_model_boolean_cells_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* argumentIds,
        int argumentCount,
        const OcctObjectId* takeIds,
        int takeCount,
        const OcctObjectId* avoidIds,
        int avoidCount,
        int material,
        OcctBool removeInternalBoundaries,
        const OcctModelBooleanOptions* options,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            if (material < 0) throw std::invalid_argument("Cells material must not be negative.");
            if (removeInternalBoundaries != 0 && material == 0)
                throw std::invalid_argument("Removing CellsBuilder internal boundaries requires a non-zero material.");
            BOPAlgo_CellsBuilder algorithm;
            algorithm.SetArguments(shapeList(model, argumentIds, argumentCount, "Cells arguments"));
            applyBooleanOptions(algorithm, options);
            algorithm.Perform();
            if (algorithm.HasErrors())
            {
                const std::string report = algorithmReport(algorithm);
                throw std::runtime_error(report.empty() ? "OCCT CellsBuilder preparation failed." : report);
            }

            const TopTools_ListOfShape take =
                shapeList(model, takeIds, takeCount, "Cells take selection");
            const TopTools_ListOfShape avoid =
                optionalShapeList(model, avoidIds, avoidCount, "Cells avoid selection");
            algorithm.AddToResult(take, avoid, material, removeInternalBoundaries != 0);
            return finishPerformedBop(model, algorithm);
        });
    }

    OcctStatus occt_model_boolean_split_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* objectIds,
        int objectCount,
        const OcctObjectId* toolIds,
        int toolCount,
        const OcctModelBooleanOptions* options,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            BRepAlgoAPI_Splitter algorithm;
            algorithm.SetArguments(shapeList(model, objectIds, objectCount, "Splitter objects"));
            algorithm.SetTools(shapeList(model, toolIds, toolCount, "Splitter tools"));
            applyBooleanOptions(algorithm, options);
            return finishBuilderAlgorithm(model, algorithm, options);
        });
    }
}

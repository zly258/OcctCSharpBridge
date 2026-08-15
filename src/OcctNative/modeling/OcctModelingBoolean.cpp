#include "modeling/OcctModelingBoolean.h"
#include "modeling/OcctModelingAlgorithmInternal.hxx"

#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepAlgoAPI_Splitter.hxx>

using namespace OcctModelingInternal;

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

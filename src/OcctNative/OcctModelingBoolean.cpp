#include "OcctModelingAlgorithmInternal.hxx"

#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepAlgoAPI_Splitter.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctModelAlgorithmResult occt_model_boolean(OcctModelHandle handle, int operation, OcctObjectId leftId, OcctObjectId rightId, const OcctModelBooleanOptions* options)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            const TopoDS_Shape& left = model->requireShape(leftId);
            const TopoDS_Shape& right = model->requireShape(rightId);
            TopTools_ListOfShape arguments;
            arguments.Append(left);
            TopTools_ListOfShape tools;
            tools.Append(right);

            if (operation == OcctModelBoolean_Cut)
            {
                BRepAlgoAPI_Cut algorithm;
                algorithm.SetArguments(arguments);
                algorithm.SetTools(tools);
                applyBooleanOptions(algorithm, options);
                result = finishBuilderAlgorithm(model, algorithm, options);
            }
            else if (operation == OcctModelBoolean_Common)
            {
                BRepAlgoAPI_Common algorithm;
                algorithm.SetArguments(arguments);
                algorithm.SetTools(tools);
                applyBooleanOptions(algorithm, options);
                result = finishBuilderAlgorithm(model, algorithm, options);
            }
            else if (operation == OcctModelBoolean_Section)
            {
                BRepAlgoAPI_Section algorithm(left, right, Standard_False);
                applyBooleanOptions(algorithm, options);
                result = finishBuilderAlgorithm(model, algorithm, options);
            }
            else
            {
                BRepAlgoAPI_Fuse algorithm;
                algorithm.SetArguments(arguments);
                algorithm.SetTools(tools);
                applyBooleanOptions(algorithm, options);
                result = finishBuilderAlgorithm(model, algorithm, options);
            }
        });
        return result;
    }

    OcctModelAlgorithmResult occt_model_split(OcctModelHandle handle, const OcctObjectId* objectIds, int objectCount, const OcctObjectId* toolIds, int toolCount, const OcctModelBooleanOptions* options)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            BRepAlgoAPI_Splitter algorithm;
            algorithm.SetArguments(shapeList(model, objectIds, objectCount, "Splitter objects"));
            algorithm.SetTools(shapeList(model, toolIds, toolCount, "Splitter tools"));
            applyBooleanOptions(algorithm, options);
            result = finishBuilderAlgorithm(model, algorithm, options);
        });
        return result;
    }
}

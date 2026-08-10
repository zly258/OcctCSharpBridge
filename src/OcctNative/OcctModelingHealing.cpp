#include "OcctModelingAlgorithmInternal.hxx"

#include <ShapeFix_Shape.hxx>
#include <ShapeUpgrade_UnifySameDomain.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctModelAlgorithmResult occt_model_unify_same_domain(OcctModelHandle handle, OcctObjectId shapeId, int unifyEdges, int unifyFaces, int concatBsplines)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            const TopoDS_Shape& source = model->requireShape(shapeId);
            ShapeUpgrade_UnifySameDomain algorithm(source, unifyEdges != 0, unifyFaces != 0, concatBsplines != 0);
            algorithm.Build();
            if (algorithm.Shape().IsNull()) throw std::runtime_error("Unify same domain failed.");
            const OcctObjectId outputId = model->addShape(algorithm.Shape());
            const OcctOperationId operationId = model->addOperation(algorithm.History(), {}, false, false);
            result = {outputId, operationId, 1, 0, 0};
        });
        return result;
    }

    OcctModelAlgorithmResult occt_model_fix_shape(OcctModelHandle handle, OcctObjectId shapeId, double precision, double minTolerance, double maxTolerance)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            requirePositive(precision, "Precision");
            if (minTolerance < 0.0 || maxTolerance < minTolerance) throw std::invalid_argument("Tolerance range is invalid.");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            ShapeFix_Shape algorithm(source);
            algorithm.SetPrecision(precision);
            algorithm.SetMinTolerance(minTolerance);
            algorithm.SetMaxTolerance(maxTolerance);
            algorithm.Perform();
            const TopoDS_Shape fixed = algorithm.Shape();
            if (fixed.IsNull()) throw std::runtime_error("Shape healing failed.");
            Handle(BRepTools_History) history = new BRepTools_History();
            history->AddModified(source, fixed);
            const OcctObjectId outputId = model->addShape(fixed);
            const OcctOperationId operationId = model->addOperation(history, {}, false, false);
            result = {outputId, operationId, 1, 0, 0};
        });
        return result;
    }
}

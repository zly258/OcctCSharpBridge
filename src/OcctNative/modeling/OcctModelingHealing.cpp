#include "modeling/OcctModelingHealing.h"
#include "modeling/OcctModelingAlgorithmInternal.hxx"

#include <ShapeFix_Shape.hxx>
#include <ShapeUpgrade_UnifySameDomain.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_healing_unify_same_domain_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBool unifyEdges,
        OcctBool unifyFaces,
        OcctBool concatBsplines,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            const TopoDS_Shape& source = model->requireShape(shapeId);
            ShapeUpgrade_UnifySameDomain algorithm(
                source,
                unifyEdges != 0,
                unifyFaces != 0,
                concatBsplines != 0);
            algorithm.Build();
            if (algorithm.Shape().IsNull())
                throw std::runtime_error("Unify same domain failed.");
            const OcctObjectId outputId = model->addShape(algorithm.Shape());
            const OcctOperationId operationId = model->addOperation(algorithm.History(), {}, false, false);
            return {outputId, operationId, 1, 0, 0};
        });
    }

    OcctStatus occt_model_healing_fix_shape_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double precision,
        double minTolerance,
        double maxTolerance,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            requirePositive(precision, "Precision");
            if (minTolerance < 0.0 || maxTolerance < minTolerance)
                throw std::invalid_argument("Tolerance range is invalid.");
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
            return {outputId, operationId, 1, 0, 0};
        });
    }
}

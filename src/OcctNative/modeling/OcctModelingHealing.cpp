#include "modeling/OcctModelingHealing.h"
#include "modeling/OcctModelingAlgorithmInternal.hxx"

#include <ShapeFix_Shape.hxx>
#include <ShapeUpgrade_UnifySameDomain.hxx>
#include <ShapeCustom.hxx>
#include <ShapeFix_ShapeTolerance.hxx>
#include <ShapeFix_Wireframe.hxx>
#include <BRep_Builder.hxx>
#include <BRepTools_ReShape.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>

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

    OcctStatus occt_model_healing_fix_tolerance_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double tolerance,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            requirePositive(tolerance, "Tolerance");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            ShapeFix_ShapeTolerance fixer;
            fixer.LimitTolerance(source, 0.0, tolerance, TopAbs_SHAPE);
            const TopoDS_Shape fixed = source; // in-place limits tolerance
            // Re-register as new shape for history tracking
            const OcctObjectId outputId = model->addShape(fixed);
            const OcctOperationId opId = model->addOperation(nullptr, {}, false, false);
            return {outputId, opId, 1, 0, 0};
        });
    }

    OcctStatus occt_model_healing_fix_gaps_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double gapTolerance,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            requirePositive(gapTolerance, "Gap tolerance");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            Handle(ShapeFix_Wireframe) fixer = new ShapeFix_Wireframe(source);
            fixer->SetPrecision(gapTolerance);
            fixer->FixSmallEdges();
            fixer->FixWireGaps();
            const TopoDS_Shape fixed = fixer->Shape();
            if (fixed.IsNull()) throw std::runtime_error("Gap fixing returned a null shape.");
            const OcctObjectId outputId = model->addShape(fixed);
            const OcctOperationId opId = model->addOperation(nullptr, {}, false, false);
            return {outputId, opId, 1, 0, 0};
        });
    }

    OcctStatus occt_model_healing_reshape_remove_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const int* subShapeIndices,
        int count,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            requireCount(count, 1, "SubShape index list");
            if (subShapeIndices == nullptr) throw std::invalid_argument("SubShape index array is null.");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            Handle(BRepTools_ReShape) reshape = new BRepTools_ReShape();
            // Iterate all sub-shapes and remove those at the given indices
            int idx = 0;
            for (TopExp_Explorer explorer(source, TopAbs_FACE); explorer.More(); explorer.Next(), ++idx) {
                for (int j = 0; j < count; ++j) {
                    if (subShapeIndices[j] == idx) {
                        reshape->Remove(explorer.Current());
                        break;
                    }
                }
            }
            const TopoDS_Shape fixed = reshape->Apply(source);
            if (fixed.IsNull()) throw std::runtime_error("Reshape remove returned a null shape.");
            const OcctObjectId outputId = model->addShape(fixed);
            const OcctOperationId opId = model->addOperation(nullptr, {}, false, false);
            return {outputId, opId, 1, 0, 0};
        });
    }
}

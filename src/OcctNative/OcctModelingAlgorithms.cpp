#include "OcctModelingInternal.hxx"

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

    OcctModelAlgorithmResult occt_model_extrude(OcctModelHandle handle, OcctObjectId profileId, OcctVector3d vectorValue)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            const TopoDS_Shape& profile = model->requireShape(profileId);
            const gp_Vec extrusion = toVector(vectorValue);
            if (extrusion.SquareMagnitude() <= Precision::SquareConfusion()) throw std::invalid_argument("Extrusion vector must not be zero.");
            BRepPrimAPI_MakePrism algorithm(profile, extrusion, Standard_False, Standard_True);
            TopTools_ListOfShape arguments;
            arguments.Append(profile);
            result = finishMakeShapeAlgorithm(model, algorithm, arguments, "Extrusion failed.");
        });
        return result;
    }

    OcctModelAlgorithmResult occt_model_revolve(OcctModelHandle handle, OcctObjectId profileId, OcctPoint3d axisPoint, OcctVector3d axisDirection, double angleDegrees)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            if (std::abs(angleDegrees) <= Precision::Angular()) throw std::invalid_argument("Revolution angle must not be zero.");
            const TopoDS_Shape& profile = model->requireShape(profileId);
            BRepPrimAPI_MakeRevol algorithm(profile, gp_Ax1(toPoint(axisPoint), toDirection(axisDirection)), angleDegrees * 3.14159265358979323846 / 180.0, Standard_True);
            TopTools_ListOfShape arguments;
            arguments.Append(profile);
            result = finishMakeShapeAlgorithm(model, algorithm, arguments, "Revolution failed.");
        });
        return result;
    }

    OcctModelAlgorithmResult occt_model_sweep(OcctModelHandle handle, OcctObjectId spineWireId, OcctObjectId profileId)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            const TopoDS_Shape& spineShape = model->requireShape(spineWireId);
            const TopoDS_Shape& profile = model->requireShape(profileId);
            if (spineShape.ShapeType() != TopAbs_WIRE) throw std::invalid_argument("Sweep spine must be a wire.");
            BRepOffsetAPI_MakePipe algorithm(TopoDS::Wire(spineShape), profile);
            TopTools_ListOfShape arguments;
            arguments.Append(spineShape);
            arguments.Append(profile);
            result = finishMakeShapeAlgorithm(model, algorithm, arguments, "Sweep failed.");
        });
        return result;
    }

    OcctModelAlgorithmResult occt_model_loft(OcctModelHandle handle, const OcctObjectId* wireIds, int count, int makeSolid, int ruled, double tolerance)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            requireCount(count, 2, "Loft section list");
            requirePositive(tolerance, "Tolerance");
            if (wireIds == nullptr) throw std::invalid_argument("Loft wire ID array is null.");
            BRepOffsetAPI_ThruSections algorithm(makeSolid != 0, ruled != 0, tolerance);
            TopTools_ListOfShape arguments;
            for (int index = 0; index < count; ++index)
            {
                const TopoDS_Shape& wireShape = model->requireShape(wireIds[index]);
                if (wireShape.ShapeType() != TopAbs_WIRE) throw std::invalid_argument("Loft inputs must be wires.");
                algorithm.AddWire(TopoDS::Wire(wireShape));
                arguments.Append(wireShape);
            }
            result = finishMakeShapeAlgorithm(model, algorithm, arguments, "Loft failed.");
        });
        return result;
    }

    OcctModelAlgorithmResult occt_model_fillet_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double radius)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            requirePositive(radius, "Fillet radius");
            requireCount(count, 1, "Fillet edge list");
            if (edgeIndices == nullptr) throw std::invalid_argument("Edge index array is null.");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            BRepFilletAPI_MakeFillet algorithm(source);
            for (int index = 0; index < count; ++index) algorithm.Add(radius, indexedEdge(source, edgeIndices[index]));
            TopTools_ListOfShape arguments;
            arguments.Append(source);
            result = finishMakeShapeAlgorithm(model, algorithm, arguments, "Selected-edge fillet failed.");
        });
        return result;
    }

    OcctModelAlgorithmResult occt_model_chamfer_edges(OcctModelHandle handle, OcctObjectId shapeId, const int* edgeIndices, int count, double distance)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            requirePositive(distance, "Chamfer distance");
            requireCount(count, 1, "Chamfer edge list");
            if (edgeIndices == nullptr) throw std::invalid_argument("Edge index array is null.");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            BRepFilletAPI_MakeChamfer algorithm(source);
            for (int index = 0; index < count; ++index) algorithm.Add(distance, indexedEdge(source, edgeIndices[index]));
            TopTools_ListOfShape arguments;
            arguments.Append(source);
            result = finishMakeShapeAlgorithm(model, algorithm, arguments, "Selected-edge chamfer failed.");
        });
        return result;
    }

    OcctModelAlgorithmResult occt_model_offset(OcctModelHandle handle, OcctObjectId shapeId, double offset, double tolerance)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            if (std::abs(offset) <= Precision::Confusion()) throw std::invalid_argument("Offset must not be zero.");
            requirePositive(tolerance, "Tolerance");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            BRepOffsetAPI_MakeOffsetShape algorithm;
            algorithm.PerformByJoin(source, offset, tolerance, BRepOffset_Skin, Standard_False, Standard_False, GeomAbs_Arc, Standard_True);
            if (!algorithm.IsDone() || algorithm.Shape().IsNull()) throw std::runtime_error("Offset failed.");
            TopTools_ListOfShape arguments;
            arguments.Append(source);
            Handle(BRepTools_History) history = new BRepTools_History(arguments, algorithm);
            const OcctObjectId outputId = model->addShape(algorithm.Shape());
            const OcctOperationId operationId = model->addOperation(history, {}, false, false);
            result = {outputId, operationId, 1, 0, 0};
        });
        return result;
    }

    OcctModelAlgorithmResult occt_model_thick_solid(OcctModelHandle handle, OcctObjectId solidId, const int* faceIndicesToRemove, int count, double thickness, double tolerance)
    {
        ModelSession* model = modelOf(handle);
        OcctModelAlgorithmResult result = failedAlgorithmResult();
        execute(model, [&]
        {
            if (std::abs(thickness) <= Precision::Confusion()) throw std::invalid_argument("Thickness must not be zero.");
            requirePositive(tolerance, "Tolerance");
            requireCount(count, 1, "Removed face list");
            if (faceIndicesToRemove == nullptr) throw std::invalid_argument("Face index array is null.");
            const TopoDS_Shape& source = model->requireShape(solidId);
            TopTools_ListOfShape faces;
            for (int index = 0; index < count; ++index) faces.Append(indexedFace(source, faceIndicesToRemove[index]));
            BRepOffsetAPI_MakeThickSolid algorithm;
            algorithm.MakeThickSolidByJoin(source, faces, thickness, tolerance, BRepOffset_Skin, Standard_False, Standard_False, GeomAbs_Arc, Standard_True);
            if (!algorithm.IsDone() || algorithm.Shape().IsNull()) throw std::runtime_error("Thick solid operation failed.");
            TopTools_ListOfShape arguments;
            arguments.Append(source);
            Handle(BRepTools_History) history = new BRepTools_History(arguments, algorithm);
            const OcctObjectId outputId = model->addShape(algorithm.Shape());
            const OcctOperationId operationId = model->addOperation(history, {}, false, false);
            result = {outputId, operationId, 1, 0, 0};
        });
        return result;
    }

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

    int occt_model_history_generated_count(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        try { return historyCount(model, operationId, sourceShapeId, true); }
        catch (...) { return 0; }
    }

    OcctObjectId occt_model_history_generated_at(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, int index)
    {
        ModelSession* model = modelOf(handle);
        OcctObjectId result = 0;
        execute(model, [&] { result = historyShapeAt(model, operationId, sourceShapeId, index, true); });
        return result;
    }

    int occt_model_history_modified_count(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        try { return historyCount(model, operationId, sourceShapeId, false); }
        catch (...) { return 0; }
    }

    OcctObjectId occt_model_history_modified_at(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId, int index)
    {
        ModelSession* model = modelOf(handle);
        OcctObjectId result = 0;
        execute(model, [&] { result = historyShapeAt(model, operationId, sourceShapeId, index, false); });
        return result;
    }

    int occt_model_history_is_removed(OcctModelHandle handle, OcctOperationId operationId, OcctObjectId sourceShapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        try
        {
            const OperationRecord& operation = requireOperation(model, operationId);
            if (operation.history.IsNull()) return 0;
            return operation.history->IsRemoved(model->requireShape(sourceShapeId)) ? 1 : 0;
        }
        catch (...) { return 0; }
    }
}

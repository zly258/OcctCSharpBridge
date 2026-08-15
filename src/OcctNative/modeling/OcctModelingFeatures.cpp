#include "modeling/OcctModelingFeatures.h"
#include "modeling/OcctModelingAlgorithmInternal.hxx"

#include <BRepFilletAPI_MakeChamfer.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepOffsetAPI_MakeOffsetShape.hxx>
#include <BRepOffsetAPI_MakePipe.hxx>
#include <BRepOffsetAPI_MakeThickSolid.hxx>
#include <BRepOffsetAPI_ThruSections.hxx>
#include <BRepOffset_Mode.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <GeomAbs_JoinType.hxx>
#include <gp_Ax1.hxx>

#include <cmath>

using namespace OcctModelingInternal;

extern "C"
{
    OcctStatus occt_model_feature_extrude_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId profileId,
        OcctVector3d vectorValue,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            const TopoDS_Shape& profile = model->requireShape(profileId);
            const gp_Vec extrusion = toVector(vectorValue);
            if (extrusion.SquareMagnitude() <= Precision::SquareConfusion())
                throw std::invalid_argument("Extrusion vector must not be zero.");
            BRepPrimAPI_MakePrism algorithm(profile, extrusion, Standard_False, Standard_True);
            TopTools_ListOfShape arguments;
            arguments.Append(profile);
            return finishMakeShapeAlgorithm(model, algorithm, arguments, "Extrusion failed.");
        });
    }

    OcctStatus occt_model_feature_revolve_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId profileId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            if (std::abs(angleDegrees) <= Precision::Angular())
                throw std::invalid_argument("Revolution angle must not be zero.");
            const TopoDS_Shape& profile = model->requireShape(profileId);
            BRepPrimAPI_MakeRevol algorithm(
                profile,
                gp_Ax1(toPoint(axisPoint), toDirection(axisDirection)),
                angleDegrees * 3.14159265358979323846 / 180.0,
                Standard_True);
            TopTools_ListOfShape arguments;
            arguments.Append(profile);
            return finishMakeShapeAlgorithm(model, algorithm, arguments, "Revolution failed.");
        });
    }

    OcctStatus occt_model_feature_sweep_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId spineWireId,
        OcctObjectId profileId,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            const TopoDS_Shape& spineShape = model->requireShape(spineWireId);
            const TopoDS_Shape& profile = model->requireShape(profileId);
            if (spineShape.ShapeType() != TopAbs_WIRE)
                throw std::invalid_argument("Sweep spine must be a wire.");
            BRepOffsetAPI_MakePipe algorithm(TopoDS::Wire(spineShape), profile);
            TopTools_ListOfShape arguments;
            arguments.Append(spineShape);
            arguments.Append(profile);
            return finishMakeShapeAlgorithm(model, algorithm, arguments, "Sweep failed.");
        });
    }

    OcctStatus occt_model_feature_loft_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* wireIds,
        int count,
        OcctBool makeSolid,
        OcctBool ruled,
        double tolerance,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            requireCount(count, 2, "Loft section list");
            requirePositive(tolerance, "Tolerance");
            if (wireIds == nullptr) throw std::invalid_argument("Loft wire ID array is null.");
            BRepOffsetAPI_ThruSections algorithm(makeSolid != 0, ruled != 0, tolerance);
            TopTools_ListOfShape arguments;
            for (int index = 0; index < count; ++index)
            {
                const TopoDS_Shape& wireShape = model->requireShape(wireIds[index]);
                if (wireShape.ShapeType() != TopAbs_WIRE)
                    throw std::invalid_argument("Loft inputs must be wires.");
                algorithm.AddWire(TopoDS::Wire(wireShape));
                arguments.Append(wireShape);
            }
            return finishMakeShapeAlgorithm(model, algorithm, arguments, "Loft failed.");
        });
    }

    OcctStatus occt_model_feature_fillet_edges_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const int* edgeIndices,
        int count,
        double radius,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            requirePositive(radius, "Fillet radius");
            requireCount(count, 1, "Fillet edge list");
            if (edgeIndices == nullptr) throw std::invalid_argument("Edge index array is null.");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            BRepFilletAPI_MakeFillet algorithm(source);
            for (int index = 0; index < count; ++index)
                algorithm.Add(radius, indexedEdge(source, edgeIndices[index]));
            TopTools_ListOfShape arguments;
            arguments.Append(source);
            return finishMakeShapeAlgorithm(model, algorithm, arguments, "Selected-edge fillet failed.");
        });
    }

    OcctStatus occt_model_feature_chamfer_edges_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const int* edgeIndices,
        int count,
        double distance,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            requirePositive(distance, "Chamfer distance");
            requireCount(count, 1, "Chamfer edge list");
            if (edgeIndices == nullptr) throw std::invalid_argument("Edge index array is null.");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            BRepFilletAPI_MakeChamfer algorithm(source);
            for (int index = 0; index < count; ++index)
                algorithm.Add(distance, indexedEdge(source, edgeIndices[index]));
            TopTools_ListOfShape arguments;
            arguments.Append(source);
            return finishMakeShapeAlgorithm(model, algorithm, arguments, "Selected-edge chamfer failed.");
        });
    }

    OcctStatus occt_model_feature_offset_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double offset,
        double tolerance,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            if (std::abs(offset) <= Precision::Confusion())
                throw std::invalid_argument("Offset must not be zero.");
            requirePositive(tolerance, "Tolerance");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            BRepOffsetAPI_MakeOffsetShape algorithm;
            algorithm.PerformByJoin(
                source,
                offset,
                tolerance,
                BRepOffset_Skin,
                Standard_False,
                Standard_False,
                GeomAbs_Arc,
                Standard_True);
            if (!algorithm.IsDone() || algorithm.Shape().IsNull())
                throw std::runtime_error("Offset failed.");
            TopTools_ListOfShape arguments;
            arguments.Append(source);
            Handle(BRepTools_History) history = new BRepTools_History(arguments, algorithm);
            const OcctObjectId outputId = model->addShape(algorithm.Shape());
            const OcctOperationId operationId = model->addOperation(history, {}, false, false);
            return {outputId, operationId, 1, 0, 0};
        });
    }

    OcctStatus occt_model_feature_thick_solid_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId solidId,
        const int* faceIndicesToRemove,
        int count,
        double thickness,
        double tolerance,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            if (std::abs(thickness) <= Precision::Confusion())
                throw std::invalid_argument("Thickness must not be zero.");
            requirePositive(tolerance, "Tolerance");
            requireCount(count, 1, "Removed face list");
            if (faceIndicesToRemove == nullptr)
                throw std::invalid_argument("Face index array is null.");
            const TopoDS_Shape& source = model->requireShape(solidId);
            TopTools_ListOfShape faces;
            for (int index = 0; index < count; ++index)
                faces.Append(indexedFace(source, faceIndicesToRemove[index]));
            BRepOffsetAPI_MakeThickSolid algorithm;
            algorithm.MakeThickSolidByJoin(
                source,
                faces,
                thickness,
                tolerance,
                BRepOffset_Skin,
                Standard_False,
                Standard_False,
                GeomAbs_Arc,
                Standard_True);
            if (!algorithm.IsDone() || algorithm.Shape().IsNull())
                throw std::runtime_error("Thick solid operation failed.");
            TopTools_ListOfShape arguments;
            arguments.Append(source);
            Handle(BRepTools_History) history = new BRepTools_History(arguments, algorithm);
            const OcctObjectId outputId = model->addShape(algorithm.Shape());
            const OcctOperationId operationId = model->addOperation(history, {}, false, false);
            return {outputId, operationId, 1, 0, 0};
        });
    }
}

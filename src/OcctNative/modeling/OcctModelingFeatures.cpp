#include "modeling/OcctModelingFeatures.h"
#include "modeling/OcctModelingAlgorithmInternal.hxx"

#include <BRepFilletAPI_MakeChamfer.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepOffsetAPI_MakeOffsetShape.hxx>
#include <BRepOffsetAPI_MakePipe.hxx>
#include <BRepOffsetAPI_MakeThickSolid.hxx>
#include <BRepOffsetAPI_ThruSections.hxx>
#include <BRepOffsetAPI_DraftAngle.hxx>
#include <BRepOffset_Mode.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <GeomAbs_JoinType.hxx>
#include <gp_Ax1.hxx>
#include <gp_Pln.hxx>

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

    OcctStatus occt_model_feature_draft_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId solidId,
        const int* faceIndices, int faceCount,
        const OcctDraftOptions* options,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            constexpr uint32_t kApiVersion = 1;
            if (options == nullptr) throw std::invalid_argument("Draft options are null.");
            if (options->structSize < sizeof(OcctDraftOptions))
                throw std::invalid_argument("Unsupported draft options size.");
            if (options->apiVersion != kApiVersion)
                throw std::invalid_argument("Unsupported draft options API version.");
            requireCount(faceCount, 1, "Draft face list");
            if (faceIndices == nullptr) throw std::invalid_argument("Face index array is null.");

            const double angleRad = options->angleDegrees * M_PI / 180.0;
            const gp_Dir pullDir = toDirection(options->pullDirection);
            const gp_Pln neutralPlane(
                toPoint(options->neutralPlanePoint),
                toDirection(options->neutralPlaneNormal));

            const TopoDS_Shape& source = model->requireShape(solidId);
            BRepOffsetAPI_DraftAngle algorithm(source);

            for (int i = 0; i < faceCount; ++i) {
                const TopoDS_Face face = indexedFace(source, faceIndices[i]);
                algorithm.Add(face, pullDir, angleRad, neutralPlane);
            }

            TopTools_ListOfShape arguments;
            arguments.Append(source);
            return finishMakeShapeAlgorithm(model, algorithm, arguments, "Draft angle operation failed.");
        });
    }

    OcctStatus occt_model_feature_fillet_variable_execute(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const OcctEdgeFilletSpec* specs,
        int count,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            requireCount(count, 1, "Variable fillet spec list");
            if (specs == nullptr) throw std::invalid_argument("Fillet spec array is null.");
            const TopoDS_Shape& source = model->requireShape(shapeId);
            BRepFilletAPI_MakeFillet algorithm(source);
            for (int i = 0; i < count; ++i) {
                if (specs[i].r1 <= 0.0 || specs[i].r2 <= 0.0)
                    throw std::invalid_argument("Fillet radii must be positive.");
                algorithm.Add(specs[i].r1, specs[i].r2, indexedEdge(source, specs[i].edgeIndex));
            }
            TopTools_ListOfShape arguments;
            arguments.Append(source);
            return finishMakeShapeAlgorithm(model, algorithm, arguments, "Variable fillet failed.");
        });
    }

    OcctStatus occt_model_feature_loft_guided_execute(
        OcctModelingSessionHandle handle,
        const OcctObjectId* sectionWireIds, int sectionCount,
        const OcctObjectId* guideWireIds,   int guideCount,
        OcctBool makeSolid,
        double tolerance,
        OcctModelAlgorithmResult* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeAlgorithmStatus(model, result, [&]() -> OcctModelAlgorithmResult
        {
            requireCount(sectionCount, 2, "Guided loft section list");
            requirePositive(tolerance, "Tolerance");
            if (sectionWireIds == nullptr) throw std::invalid_argument("Section wire ID array is null.");

            BRepOffsetAPI_ThruSections algorithm(makeSolid != 0, Standard_False, tolerance);
            TopTools_ListOfShape arguments;

            for (int i = 0; i < sectionCount; ++i) {
                const TopoDS_Shape& s = model->requireShape(sectionWireIds[i]);
                if (s.ShapeType() != TopAbs_WIRE)
                    throw std::invalid_argument("Guided loft sections must be wires.");
                algorithm.AddWire(TopoDS::Wire(s));
                arguments.Append(s);
            }

            if (guideWireIds != nullptr && guideCount > 0) {
                for (int i = 0; i < guideCount; ++i) {
                    const TopoDS_Shape& g = model->requireShape(guideWireIds[i]);
                    if (g.ShapeType() != TopAbs_WIRE)
                        throw std::invalid_argument("Guided loft guide curves must be wires.");
                    algorithm.AddGuideWire(TopoDS::Wire(g));
                    arguments.Append(g);
                }
            }

            return finishMakeShapeAlgorithm(model, algorithm, arguments, "Guided loft failed.");
        });
    }
}

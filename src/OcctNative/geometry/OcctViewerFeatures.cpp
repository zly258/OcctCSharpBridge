#include "geometry/OcctViewerFeatures.h"
#include "core/OcctInternal.hxx"

#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepFilletAPI_MakeChamfer.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepOffsetAPI_MakeOffsetShape.hxx>
#include <BRepOffsetAPI_MakePipe.hxx>
#include <BRepOffsetAPI_MakeThickSolid.hxx>
#include <BRepOffsetAPI_ThruSections.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <GeomAbs_JoinType.hxx>
#include <Precision.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopTools_IndexedMapOfShape.hxx>
#include <TopTools_ListOfShape.hxx>
#include <TopoDS.hxx>

#include <cmath>
#include <stdexcept>
#include <utility>

using namespace OcctBridge;

namespace
{
    OcctStatus requireInitializedEngine(Engine* engine)
    {
        if (engine == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (!validateInitialized(engine)) return engine->currentErrorCode();
        return OcctStatus_Ok;
    }

    template<typename Function>
    OcctStatus executeFeatureStatus(Engine* engine, Function&& function)
    {
        const OcctStatus initialized = requireInitializedEngine(engine);
        if (initialized != OcctStatus_Ok) return initialized;
        return execute(engine, std::forward<Function>(function)) != 0
            ? OcctStatus_Ok
            : engine->currentErrorCode();
    }

    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry = engine->findShape(id);
        if (entry == nullptr || entry->shape.IsNull())
            throw std::invalid_argument("Shape ID does not exist.");
        return *entry;
    }

    TopoDS_Shape committedShape(ObjectEntry& entry)
    {
        return shapeWithPresentationTransformation(entry);
    }

    TopoDS_Edge indexedEdge(const TopoDS_Shape& shape, int zeroBasedIndex)
    {
        if (zeroBasedIndex < 0) throw std::out_of_range("Edge index must not be negative.");
        TopTools_IndexedMapOfShape edges;
        TopExp::MapShapes(shape, TopAbs_EDGE, edges);
        const int oneBasedIndex = zeroBasedIndex + 1;
        if (oneBasedIndex > edges.Extent()) throw std::out_of_range("Edge index is out of range.");
        return TopoDS::Edge(edges(oneBasedIndex));
    }

    OcctObjectId addFeatureResult(
        Engine* engine,
        const TopoDS_Shape& shape,
        const char* name,
        const OcctObjectId* inputs,
        int inputCount,
        OcctBool hideInputs)
    {
        if (shape.IsNull()) throw std::runtime_error(std::string(name) + " returned a null shape.");
        const OcctObjectId result = engine->addShape(shape, false, name);
        if (hideInputs != 0)
        {
            for (int index = 0; index < inputCount; ++index) engine->hide(inputs[index]);
        }
        return result;
    }

    template<typename Function>
    OcctStatus createFeatureResult(Engine* engine, OcctObjectId* result, Function&& function)
    {
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = 0;
        return executeFeatureStatus(engine, [&]
        {
            *result = function();
            if (*result <= 0) throw std::runtime_error("Feature operation did not create a viewer object.");
        });
    }
}

extern "C"
{
    OcctStatus occt_engine_shape_boolean(
        OcctEngineHandle handle,
        int operation,
        OcctObjectId leftId,
        OcctObjectId rightId,
        OcctBool hideInputs,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            const TopoDS_Shape left = committedShape(requiredShape(engine, leftId));
            const TopoDS_Shape right = committedShape(requiredShape(engine, rightId));
            TopoDS_Shape resultShape;

            switch (operation)
            {
                case OcctBoolean_Fuse:
                {
                    BRepAlgoAPI_Fuse maker(left, right);
                    maker.Build();
                    if (!maker.IsDone()) throw std::runtime_error("Fuse failed.");
                    resultShape = maker.Shape();
                    break;
                }
                case OcctBoolean_Cut:
                {
                    BRepAlgoAPI_Cut maker(left, right);
                    maker.Build();
                    if (!maker.IsDone()) throw std::runtime_error("Cut failed.");
                    resultShape = maker.Shape();
                    break;
                }
                case OcctBoolean_Common:
                {
                    BRepAlgoAPI_Common maker(left, right);
                    maker.Build();
                    if (!maker.IsDone()) throw std::runtime_error("Common failed.");
                    resultShape = maker.Shape();
                    break;
                }
                case OcctBoolean_Section:
                {
                    BRepAlgoAPI_Section maker(left, right, Standard_False);
                    maker.Build();
                    if (!maker.IsDone()) throw std::runtime_error("Section failed.");
                    resultShape = maker.Shape();
                    break;
                }
                default:
                    throw std::invalid_argument("Boolean operation is out of range.");
            }

            const OcctObjectId inputs[2] = {leftId, rightId};
            return addFeatureResult(engine, resultShape, "Boolean", inputs, 2, hideInputs);
        });
    }

    OcctStatus occt_engine_shape_extrude(
        OcctEngineHandle handle,
        OcctObjectId profileId,
        OcctVector3d value,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            const TopoDS_Shape profile = committedShape(requiredShape(engine, profileId));
            const gp_Vec directionVector = vector(value);
            if (directionVector.SquareMagnitude() <= Precision::SquareConfusion())
                throw std::invalid_argument("Extrusion vector must not be zero.");
            BRepPrimAPI_MakePrism maker(profile, directionVector, Standard_False, Standard_True);
            if (!maker.IsDone()) throw std::runtime_error("Extrusion failed.");
            return addFeatureResult(engine, maker.Shape(), "Extrude", &profileId, 1, hideInput);
        });
    }

    OcctStatus occt_engine_shape_revolve(
        OcctEngineHandle handle,
        OcctObjectId profileId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            if (!std::isfinite(angleDegrees) || std::abs(angleDegrees) <= Precision::Angular())
                throw std::invalid_argument("Revolution angle must be finite and non-zero.");
            const TopoDS_Shape profile = committedShape(requiredShape(engine, profileId));
            BRepPrimAPI_MakeRevol maker(
                profile,
                gp_Ax1(point(axisPoint), direction(axisDirection)),
                angleDegrees * 3.14159265358979323846 / 180.0,
                Standard_True);
            if (!maker.IsDone()) throw std::runtime_error("Revolution failed.");
            return addFeatureResult(engine, maker.Shape(), "Revolve", &profileId, 1, hideInput);
        });
    }

    OcctStatus occt_engine_shape_sweep(
        OcctEngineHandle handle,
        OcctObjectId spineWireId,
        OcctObjectId profileId,
        OcctBool hideInputs,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            const TopoDS_Shape spine = committedShape(requiredShape(engine, spineWireId));
            const TopoDS_Shape profile = committedShape(requiredShape(engine, profileId));
            if (spine.ShapeType() != TopAbs_WIRE)
                throw std::invalid_argument("Sweep spine must be a wire.");
            BRepOffsetAPI_MakePipe maker(TopoDS::Wire(spine), profile);
            maker.Build();
            if (!maker.IsDone()) throw std::runtime_error("Sweep failed.");
            const OcctObjectId inputs[2] = {spineWireId, profileId};
            return addFeatureResult(engine, maker.Shape(), "Sweep", inputs, 2, hideInputs);
        });
    }

    OcctStatus occt_engine_shape_loft(
        OcctEngineHandle handle,
        const OcctObjectId* wireIds,
        int count,
        OcctBool makeSolid,
        OcctBool ruled,
        double tolerance,
        OcctBool hideInputs,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            requireCount(count, 2, "Loft");
            requirePositive(tolerance, "Tolerance");
            if (wireIds == nullptr) throw std::invalid_argument("Wire ID array is null.");

            BRepOffsetAPI_ThruSections maker(makeSolid != 0, ruled != 0, tolerance);
            for (int index = 0; index < count; ++index)
            {
                const TopoDS_Shape wire = committedShape(requiredShape(engine, wireIds[index]));
                if (wire.ShapeType() != TopAbs_WIRE)
                    throw std::invalid_argument("Loft inputs must be wires.");
                maker.AddWire(TopoDS::Wire(wire));
            }
            maker.Build();
            if (!maker.IsDone()) throw std::runtime_error("Loft failed.");
            return addFeatureResult(engine, maker.Shape(), "Loft", wireIds, count, hideInputs);
        });
    }

    OcctStatus occt_engine_shape_fillet_all_edges(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        double radius,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            requirePositive(radius, "Fillet radius");
            const TopoDS_Shape source = committedShape(requiredShape(engine, shapeId));
            BRepFilletAPI_MakeFillet maker(source);
            int count = 0;
            for (TopExp_Explorer explorer(source, TopAbs_EDGE); explorer.More(); explorer.Next())
            {
                maker.Add(radius, TopoDS::Edge(explorer.Current()));
                ++count;
            }
            if (count == 0) throw std::runtime_error("Shape has no edges to fillet.");
            maker.Build();
            if (!maker.IsDone()) throw std::runtime_error("Fillet failed. Try a smaller radius.");
            return addFeatureResult(engine, maker.Shape(), "Fillet", &shapeId, 1, hideInput);
        });
    }

    OcctStatus occt_engine_shape_fillet_edges(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        const int* edgeIndices,
        int count,
        double radius,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            requirePositive(radius, "Fillet radius");
            requireCount(count, 1, "Fillet edge list");
            if (edgeIndices == nullptr) throw std::invalid_argument("Edge index array is null.");
            const TopoDS_Shape source = committedShape(requiredShape(engine, shapeId));
            BRepFilletAPI_MakeFillet maker(source);
            for (int index = 0; index < count; ++index)
                maker.Add(radius, indexedEdge(source, edgeIndices[index]));
            maker.Build();
            if (!maker.IsDone()) throw std::runtime_error("Selected-edge fillet failed.");
            return addFeatureResult(engine, maker.Shape(), "FilletEdges", &shapeId, 1, hideInput);
        });
    }

    OcctStatus occt_engine_shape_chamfer_all_edges(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        double distance,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            requirePositive(distance, "Chamfer distance");
            const TopoDS_Shape source = committedShape(requiredShape(engine, shapeId));
            BRepFilletAPI_MakeChamfer maker(source);
            int count = 0;
            for (TopExp_Explorer explorer(source, TopAbs_EDGE); explorer.More(); explorer.Next())
            {
                maker.Add(distance, TopoDS::Edge(explorer.Current()));
                ++count;
            }
            if (count == 0) throw std::runtime_error("Shape has no edges to chamfer.");
            maker.Build();
            if (!maker.IsDone()) throw std::runtime_error("Chamfer failed. Try a smaller distance.");
            return addFeatureResult(engine, maker.Shape(), "Chamfer", &shapeId, 1, hideInput);
        });
    }

    OcctStatus occt_engine_shape_chamfer_edges(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        const int* edgeIndices,
        int count,
        double distance,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            requirePositive(distance, "Chamfer distance");
            requireCount(count, 1, "Chamfer edge list");
            if (edgeIndices == nullptr) throw std::invalid_argument("Edge index array is null.");
            const TopoDS_Shape source = committedShape(requiredShape(engine, shapeId));
            BRepFilletAPI_MakeChamfer maker(source);
            for (int index = 0; index < count; ++index)
                maker.Add(distance, indexedEdge(source, edgeIndices[index]));
            maker.Build();
            if (!maker.IsDone()) throw std::runtime_error("Selected-edge chamfer failed.");
            return addFeatureResult(engine, maker.Shape(), "ChamferEdges", &shapeId, 1, hideInput);
        });
    }

    OcctStatus occt_engine_shape_offset(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        double offset,
        double tolerance,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            if (!std::isfinite(offset) || std::abs(offset) <= Precision::Confusion())
                throw std::invalid_argument("Offset must be finite and non-zero.");
            requirePositive(tolerance, "Tolerance");
            const TopoDS_Shape source = committedShape(requiredShape(engine, shapeId));
            BRepOffsetAPI_MakeOffsetShape maker;
            maker.PerformByJoin(
                source,
                offset,
                tolerance,
                BRepOffset_Skin,
                Standard_False,
                Standard_False,
                GeomAbs_Arc,
                Standard_True);
            if (!maker.IsDone()) throw std::runtime_error("Offset shape failed.");
            return addFeatureResult(engine, maker.Shape(), "Offset", &shapeId, 1, hideInput);
        });
    }

    OcctStatus occt_engine_shape_thick_solid(
        OcctEngineHandle handle,
        OcctObjectId solidId,
        int faceIndexToRemove,
        double thickness,
        double tolerance,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createFeatureResult(engine, result, [&]
        {
            if (!std::isfinite(thickness) || std::abs(thickness) <= Precision::Confusion())
                throw std::invalid_argument("Thickness must be finite and non-zero.");
            requirePositive(tolerance, "Tolerance");
            if (faceIndexToRemove < 0) throw std::invalid_argument("Face index must not be negative.");

            const TopoDS_Shape source = committedShape(requiredShape(engine, solidId));
            TopTools_ListOfShape faces;
            int index = 0;
            bool found = false;
            for (TopExp_Explorer explorer(source, TopAbs_FACE); explorer.More(); explorer.Next(), ++index)
            {
                if (index == faceIndexToRemove)
                {
                    faces.Append(explorer.Current());
                    found = true;
                    break;
                }
            }
            if (!found) throw std::out_of_range("Face index is out of range.");

            BRepOffsetAPI_MakeThickSolid maker;
            maker.MakeThickSolidByJoin(
                source,
                faces,
                thickness,
                tolerance,
                BRepOffset_Skin,
                Standard_False,
                Standard_False,
                GeomAbs_Arc,
                Standard_True);
            if (!maker.IsDone()) throw std::runtime_error("Thick solid operation failed.");
            return addFeatureResult(engine, maker.Shape(), "ThickSolid", &solidId, 1, hideInput);
        });
    }
}

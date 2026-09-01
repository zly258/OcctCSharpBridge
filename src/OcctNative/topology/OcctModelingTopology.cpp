#include "topology/OcctModelingTopology.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepTools.hxx>
#include <BRepTools_WireExplorer.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <TopTools_IndexedDataMapOfShapeListOfShape.hxx>
#include <TopTools_ListIteratorOfListOfShape.hxx>

#include <stdexcept>

using namespace OcctModelingInternal;

namespace
{
    void copyMappedShapes(
        ModelSession* model,
        const TopTools_IndexedMapOfShape& map,
        OcctObjectId* results,
        int capacity,
        int* required)
    {
        const int count = map.Extent();
        *required = count;
        if (results == nullptr)
        {
            if (capacity != 0) throw std::invalid_argument("Null topology buffer requires zero capacity.");
            return;
        }
        if (capacity < count)
            throw std::invalid_argument("Topology buffer capacity is smaller than the result count.");
        for (int index = 1; index <= count; ++index)
            results[index - 1] = model->addShape(map(index));
    }

    void copyShapeList(
        ModelSession* model,
        const TopTools_ListOfShape& list,
        OcctObjectId* results,
        int capacity,
        int* required)
    {
        const int count = list.Size();
        *required = count;
        if (results == nullptr)
        {
            if (capacity != 0) throw std::invalid_argument("Null topology buffer requires zero capacity.");
            return;
        }
        if (capacity < count)
            throw std::invalid_argument("Topology buffer capacity is smaller than the result count.");

        int index = 0;
        for (TopTools_ListIteratorOfListOfShape iterator(list); iterator.More(); iterator.Next(), ++index)
            results[index] = model->addShape(iterator.Value());
    }
}

extern "C"
{
    OcctStatus occt_model_wire_create(
        OcctModelingSessionHandle handle,
        const OcctObjectId* edgeIds,
        int edgeCount,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            if (edgeIds == nullptr)
                throw std::invalid_argument("Edge ID array is null.");
            if (edgeCount < 1)
                throw std::invalid_argument("Wire requires at least one edge.");

            BRepBuilderAPI_MakeWire maker;
            for (int index = 0; index < edgeCount; ++index)
            {
                const TopoDS_Shape& shape = model->requireShape(edgeIds[index]);
                if (shape.ShapeType() != TopAbs_EDGE)
                    throw std::invalid_argument("Wire input must contain only edges.");
                maker.Add(TopoDS::Edge(shape));
                if (!maker.IsDone())
                    throw std::runtime_error("Wire construction failed.");
            }

            const TopoDS_Wire wire = maker.Wire();
            if (wire.IsNull())
                throw std::runtime_error("Wire construction produced a null wire.");
            return TopoDS_Shape(wire);
        });
    }

    OcctStatus occt_model_subshapes_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        int shapeType,
        OcctObjectId* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            TopTools_IndexedMapOfShape map;
            TopExp::MapShapes(model->requireShape(shapeId), toShapeEnum(shapeType), map);
            copyMappedShapes(model, map, results, capacity, required);
        });
    }

    OcctStatus occt_model_outer_wire_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        return executeShapeStatus(model, result, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Input must be a face.");
            const TopoDS_Wire wire = BRepTools::OuterWire(TopoDS::Face(shape));
            if (wire.IsNull()) throw std::runtime_error("Face has no outer wire.");
            return TopoDS_Shape(wire);
        });
    }

    OcctStatus occt_model_inner_wires_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId faceId,
        OcctObjectId* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Input must be a face.");

            const TopoDS_Face face = TopoDS::Face(shape);
            const TopoDS_Wire outer = BRepTools::OuterWire(face);
            TopTools_ListOfShape inner;
            for (TopExp_Explorer explorer(face, TopAbs_WIRE); explorer.More(); explorer.Next())
                if (!explorer.Current().IsSame(outer)) inner.Append(explorer.Current());

            copyShapeList(model, inner, results, capacity, required);
        });
    }

    OcctStatus occt_model_wire_edges_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId wireId,
        OcctObjectId* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(wireId);
            if (shape.ShapeType() != TopAbs_WIRE)
                throw std::invalid_argument("Input must be a wire.");

            TopTools_ListOfShape edges;
            for (BRepTools_WireExplorer explorer(TopoDS::Wire(shape)); explorer.More(); explorer.Next())
                edges.Append(explorer.Current());
            copyShapeList(model, edges, results, capacity, required);
        });
    }

    OcctStatus occt_model_ancestors_snapshot_get(
        OcctModelingSessionHandle handle,
        OcctObjectId rootId,
        OcctObjectId childId,
        int ancestorType,
        OcctObjectId* results,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        *required = 0;
        return executeStatus(model, [&]
        {
            const TopoDS_Shape& root = model->requireShape(rootId);
            const TopoDS_Shape& child = model->requireShape(childId);
            TopTools_IndexedDataMapOfShapeListOfShape map;
            TopExp::MapShapesAndAncestors(root, child.ShapeType(), toShapeEnum(ancestorType), map);
            if (!map.Contains(child)) return;
            copyShapeList(model, map.FindFromKey(child), results, capacity, required);
        });
    }
}

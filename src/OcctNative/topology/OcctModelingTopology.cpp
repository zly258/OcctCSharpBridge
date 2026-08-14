#include "OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_Sewing.hxx>
#include <BRepTools.hxx>
#include <TopTools_IndexedDataMapOfShapeListOfShape.hxx>
#include <TopTools_ListIteratorOfListOfShape.hxx>

using namespace OcctModelingInternal;

namespace
{
    int copyMappedShapes(ModelSession* model, const TopTools_IndexedMapOfShape& map, OcctObjectId* results, int capacity)
    {
        const int count = map.Extent();
        if (results == nullptr)
        {
            if (capacity != 0) throw std::invalid_argument("Null topology buffer requires zero capacity.");
            return count;
        }
        if (capacity < count) throw std::invalid_argument("Topology buffer capacity is smaller than the result count.");
        for (int index = 1; index <= count; ++index)
            results[index - 1] = model->addShape(map(index));
        return count;
    }

    int copyShapeList(ModelSession* model, const TopTools_ListOfShape& list, OcctObjectId* results, int capacity)
    {
        const int count = list.Size();
        if (results == nullptr)
        {
            if (capacity != 0) throw std::invalid_argument("Null topology buffer requires zero capacity.");
            return count;
        }
        if (capacity < count) throw std::invalid_argument("Topology buffer capacity is smaller than the result count.");
        int index = 0;
        for (TopTools_ListIteratorOfListOfShape iterator(list); iterator.More(); iterator.Next(), ++index)
            results[index] = model->addShape(iterator.Value());
        return count;
    }
}

extern "C"
{
    int occt_model_subshapes_copy(OcctModelHandle handle, OcctObjectId shapeId, int shapeType, OcctObjectId* results, int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;
        int copied = 0;
        if (execute(model, [&]
        {
            if (capacity < 0) throw std::invalid_argument("Subshape buffer capacity must not be negative.");
            TopTools_IndexedMapOfShape map;
            TopExp::MapShapes(model->requireShape(shapeId), toShapeEnum(shapeType), map);
            copied = copyMappedShapes(model, map, results, capacity);
        }) == 0)
            return -1;
        return copied;
    }

    OcctObjectId occt_model_outer_wire(OcctModelHandle handle, OcctObjectId faceId)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            const TopoDS_Wire wire = BRepTools::OuterWire(TopoDS::Face(shape));
            if (wire.IsNull()) throw std::runtime_error("Face has no outer wire.");
            return wire;
        });
    }

    int occt_model_inner_wires_copy(OcctModelHandle handle, OcctObjectId faceId, OcctObjectId* results, int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;
        int copied = 0;
        if (execute(model, [&]
        {
            if (capacity < 0) throw std::invalid_argument("Inner-wire buffer capacity must not be negative.");
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            const TopoDS_Face face = TopoDS::Face(shape);
            const TopoDS_Wire outer = BRepTools::OuterWire(face);
            TopTools_ListOfShape inner;
            for (TopExp_Explorer explorer(face, TopAbs_WIRE); explorer.More(); explorer.Next())
                if (!explorer.Current().IsSame(outer)) inner.Append(explorer.Current());
            copied = copyShapeList(model, inner, results, capacity);
        }) == 0)
            return -1;
        return copied;
    }

    int occt_model_ancestors_copy(
        OcctModelHandle handle,
        OcctObjectId rootId,
        OcctObjectId childId,
        int ancestorType,
        OcctObjectId* results,
        int capacity)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return -1;
        int copied = 0;
        if (execute(model, [&]
        {
            if (capacity < 0) throw std::invalid_argument("Ancestor buffer capacity must not be negative.");
            const TopoDS_Shape& root = model->requireShape(rootId);
            const TopoDS_Shape& child = model->requireShape(childId);
            TopTools_IndexedDataMapOfShapeListOfShape map;
            TopExp::MapShapesAndAncestors(root, child.ShapeType(), toShapeEnum(ancestorType), map);
            if (!map.Contains(child))
            {
                if (results != nullptr && capacity < 0) throw std::invalid_argument("Ancestor buffer capacity is invalid.");
                copied = 0;
                return;
            }
            copied = copyShapeList(model, map.FindFromKey(child), results, capacity);
        }) == 0)
            return -1;
        return copied;
    }

    OcctObjectId occt_model_sew(OcctModelHandle handle, const OcctObjectId* shapeIds, int count, double tolerance)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            requireCount(count, 1, "Sewing");
            requirePositive(tolerance, "Tolerance");
            if (shapeIds == nullptr) throw std::invalid_argument("Shape ID array is null.");
            BRepBuilderAPI_Sewing sewing(tolerance);
            for (int index = 0; index < count; ++index) sewing.Add(model->requireShape(shapeIds[index]));
            sewing.Perform();
            const TopoDS_Shape shape = sewing.SewedShape();
            if (shape.IsNull()) throw std::runtime_error("Sewing failed.");
            return shape;
        });
    }
}

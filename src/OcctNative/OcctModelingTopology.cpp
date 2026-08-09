#include "OcctModelingShapeInternal.hxx"

#include <BRepBuilderAPI_Sewing.hxx>
#include <BRepTools.hxx>
#include <TopTools_IndexedDataMapOfShapeListOfShape.hxx>
#include <TopTools_ListIteratorOfListOfShape.hxx>

using namespace OcctModelingInternal;

extern "C"
{
    int occt_model_topology_count(OcctModelHandle handle, OcctObjectId shapeId, int shapeType)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        try
        {
            TopTools_IndexedMapOfShape map;
            TopExp::MapShapes(model->requireShape(shapeId), toShapeEnum(shapeType), map);
            return map.Extent();
        }
        catch (...) { return 0; }
    }

    OcctObjectId occt_model_get_subshape(OcctModelHandle handle, OcctObjectId shapeId, int shapeType, int index)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            if (index < 0) throw std::out_of_range("Subshape index must not be negative.");
            TopTools_IndexedMapOfShape map;
            TopExp::MapShapes(model->requireShape(shapeId), toShapeEnum(shapeType), map);
            const int oneBased = index + 1;
            if (oneBased > map.Extent()) throw std::out_of_range("Subshape index is out of range.");
            return map(oneBased);
        });
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

    int occt_model_inner_wire_count(OcctModelHandle handle, OcctObjectId faceId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        try
        {
            const TopoDS_Face face = TopoDS::Face(model->requireShape(faceId));
            const TopoDS_Wire outer = BRepTools::OuterWire(face);
            int count = 0;
            for (TopExp_Explorer explorer(face, TopAbs_WIRE); explorer.More(); explorer.Next())
                if (!explorer.Current().IsSame(outer)) ++count;
            return count;
        }
        catch (...) { return 0; }
    }

    OcctObjectId occt_model_inner_wire_at(OcctModelHandle handle, OcctObjectId faceId, int index)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            if (index < 0) throw std::out_of_range("Inner wire index must not be negative.");
            const TopoDS_Face face = TopoDS::Face(model->requireShape(faceId));
            const TopoDS_Wire outer = BRepTools::OuterWire(face);
            int current = 0;
            for (TopExp_Explorer explorer(face, TopAbs_WIRE); explorer.More(); explorer.Next())
            {
                if (explorer.Current().IsSame(outer)) continue;
                if (current++ == index) return explorer.Current();
            }
            throw std::out_of_range("Inner wire index is out of range.");
        });
    }

    int occt_model_ancestor_count(OcctModelHandle handle, OcctObjectId rootId, OcctObjectId childId, int ancestorType)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        try
        {
            const TopoDS_Shape& root = model->requireShape(rootId);
            const TopoDS_Shape& child = model->requireShape(childId);
            TopTools_IndexedDataMapOfShapeListOfShape map;
            TopExp::MapShapesAndAncestors(root, child.ShapeType(), toShapeEnum(ancestorType), map);
            return map.Contains(child) ? map.FindFromKey(child).Size() : 0;
        }
        catch (...) { return 0; }
    }

    OcctObjectId occt_model_ancestor_at(OcctModelHandle handle, OcctObjectId rootId, OcctObjectId childId, int ancestorType, int index)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            if (index < 0) throw std::out_of_range("Ancestor index must not be negative.");
            const TopoDS_Shape& root = model->requireShape(rootId);
            const TopoDS_Shape& child = model->requireShape(childId);
            TopTools_IndexedDataMapOfShapeListOfShape map;
            TopExp::MapShapesAndAncestors(root, child.ShapeType(), toShapeEnum(ancestorType), map);
            if (!map.Contains(child)) throw std::out_of_range("The child has no requested ancestors.");
            int current = 0;
            for (TopTools_ListIteratorOfListOfShape iterator(map.FindFromKey(child)); iterator.More(); iterator.Next(), ++current)
                if (current == index) return iterator.Value();
            throw std::out_of_range("Ancestor index is out of range.");
        });
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

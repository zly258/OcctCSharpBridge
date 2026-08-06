#include "OcctModelingInternal.hxx"

using namespace OcctModelingInternal;

extern "C"
{
    OcctModelHandle occt_model_create()
    {
        try { return new ModelSession(); }
        catch (...) { return nullptr; }
    }

    void occt_model_destroy(OcctModelHandle handle)
    {
        delete modelOf(handle);
    }

    const char* occt_model_last_error(OcctModelHandle handle)
    {
        ModelSession* model = modelOf(handle);
        return model == nullptr ? "Invalid OCCT modeling handle." : model->lastError.c_str();
    }

    const char* occt_model_capabilities()
    {
        return "headless;geometry-query;analytic-geometry;topology;history;healing;mesh;projection;ray-intersection;classification;advanced-boolean;splitter;sweep;loft;step;iges;brep;stl;viewer-interop";
    }

    int occt_model_shape_count(OcctModelHandle handle)
    {
        ModelSession* model = modelOf(handle);
        return model == nullptr ? 0 : static_cast<int>(model->shapes.size());
    }

    OcctObjectId occt_model_shape_id_at(OcctModelHandle handle, int index)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr || index < 0 || index >= static_cast<int>(model->shapes.size())) return 0;
        auto iterator = model->shapes.begin();
        std::advance(iterator, index);
        return iterator->first;
    }

    int occt_model_shape_exists(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        return model != nullptr && model->shapes.find(shapeId) != model->shapes.end() ? 1 : 0;
    }

    int occt_model_delete_shape(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        return execute(model, [&]
        {
            if (model->shapes.erase(shapeId) == 0) throw std::invalid_argument("Shape ID does not exist.");
        });
    }

    int occt_model_clear(OcctModelHandle handle)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        return execute(model, [&]
        {
            model->shapes.clear();
            model->operations.clear();
            model->rayHits.clear();
        });
    }

    const char* occt_model_operation_report(OcctModelHandle handle, OcctOperationId operationId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return "Invalid OCCT modeling handle.";
        model->scratchString.clear();
        execute(model, [&] { model->scratchString = requireOperation(model, operationId).report; });
        return model->scratchString.c_str();
    }

    OcctObjectId occt_model_copy_shape(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        return executeShape(model, [&]
        {
            BRepBuilderAPI_Copy copy(model->requireShape(shapeId), Standard_True, Standard_True);
            if (!copy.IsDone()) throw std::runtime_error("Shape copy failed.");
            return copy.Shape();
        });
    }

    std::int64_t occt_model_shape_hash(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        try
        {
            return static_cast<std::int64_t>(TopTools_ShapeMapHasher{}(model->requireShape(shapeId)));
        }
        catch (const std::exception& exception)
        {
            model->lastError = exception.what();
            return 0;
        }
    }

    int occt_model_shape_type(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return OcctShape_Shape;
        try { return static_cast<int>(model->requireShape(shapeId).ShapeType()); }
        catch (...) { return OcctShape_Shape; }
    }

    int occt_model_shape_orientation(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return OcctModelOrientation_Forward;
        try { return static_cast<int>(model->requireShape(shapeId).Orientation()); }
        catch (...) { return OcctModelOrientation_Forward; }
    }

    int occt_model_shape_is_closed(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        try { return model->requireShape(shapeId).Closed() ? 1 : 0; }
        catch (...) { return 0; }
    }

    int occt_model_shape_is_valid(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0;
        try { return BRepCheck_Analyzer(model->requireShape(shapeId), Standard_True).IsValid() ? 1 : 0; }
        catch (...) { return 0; }
    }

    double occt_model_shape_tolerance(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return 0.0;
        try { return maximumTolerance(model->requireShape(shapeId)); }
        catch (...) { return 0.0; }
    }

    int occt_model_shape_bounds(OcctModelHandle handle, OcctObjectId shapeId, OcctBounds* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            Bnd_Box box;
            BRepBndLib::Add(model->requireShape(shapeId), box, Standard_True);
            if (box.IsVoid()) throw std::runtime_error("Shape bounding box is empty.");
            box.Get(result->minX, result->minY, result->minZ, result->maxX, result->maxY, result->maxZ);
        });
    }

    int occt_model_shape_linear_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            GProp_GProps properties;
            BRepGProp::LinearProperties(model->requireShape(shapeId), properties);
            fillProperties(properties, result);
        });
    }

    int occt_model_shape_surface_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            GProp_GProps properties;
            BRepGProp::SurfaceProperties(model->requireShape(shapeId), properties);
            fillProperties(properties, result);
        });
    }

    int occt_model_shape_volume_properties(OcctModelHandle handle, OcctObjectId shapeId, OcctMassProperties* result)
    {
        ModelSession* model = modelOf(handle);
        return execute(model, [&]
        {
            GProp_GProps properties;
            BRepGProp::VolumeProperties(model->requireShape(shapeId), properties);
            fillProperties(properties, result);
        });
    }

    int occt_model_shape_distance(OcctModelHandle handle, OcctObjectId firstId, OcctObjectId secondId, OcctDistanceResult* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            BRepExtrema_DistShapeShape distance(model->requireShape(firstId), model->requireShape(secondId));
            distance.Perform();
            if (!distance.IsDone() || distance.NbSolution() < 1) throw std::runtime_error("Distance calculation failed.");
            const gp_Pnt first = distance.PointOnShape1(1);
            const gp_Pnt second = distance.PointOnShape2(1);
            result->distance = distance.Value();
            result->pointOnFirst = {first.X(), first.Y(), first.Z()};
            result->pointOnSecond = {second.X(), second.Y(), second.Z()};
        });
    }

    const char* occt_model_check_report(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return "{\"valid\":false,\"error\":\"invalid handle\"}";
        model->scratchString.clear();
        execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(shapeId);
            BRepCheck_Analyzer analyzer(shape, Standard_True);
            std::ostringstream stream;
            stream << "{\"valid\":" << (analyzer.IsValid() ? "true" : "false")
                   << ",\"shapeType\":" << static_cast<int>(shape.ShapeType())
                   << ",\"maxTolerance\":" << maximumTolerance(shape)
                   << ",\"invalid\":[";
            bool firstItem = true;
            for (int type = static_cast<int>(TopAbs_VERTEX); type >= static_cast<int>(TopAbs_SOLID); --type)
            {
                int index = 0;
                for (TopExp_Explorer explorer(shape, static_cast<TopAbs_ShapeEnum>(type)); explorer.More(); explorer.Next(), ++index)
                {
                    if (BRepCheck_Analyzer(explorer.Current(), Standard_True).IsValid()) continue;
                    if (!firstItem) stream << ',';
                    firstItem = false;
                    stream << "{\"type\":" << type << ",\"index\":" << index << '}';
                }
            }
            stream << "]}";
            model->scratchString = stream.str();
        });
        return model->scratchString.c_str();
    }

    int occt_model_get_location(OcctModelHandle handle, OcctObjectId shapeId, OcctModelLocation* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const gp_Trsf transform = model->requireShape(shapeId).Location().Transformation();
            result->m11 = transform.Value(1, 1); result->m12 = transform.Value(1, 2); result->m13 = transform.Value(1, 3); result->m14 = transform.Value(1, 4);
            result->m21 = transform.Value(2, 1); result->m22 = transform.Value(2, 2); result->m23 = transform.Value(2, 3); result->m24 = transform.Value(2, 4);
            result->m31 = transform.Value(3, 1); result->m32 = transform.Value(3, 2); result->m33 = transform.Value(3, 3); result->m34 = transform.Value(3, 4);
            result->m41 = 0.0; result->m42 = 0.0; result->m43 = 0.0; result->m44 = 1.0;
        });
    }

    OcctObjectId occt_model_set_location(OcctModelHandle handle, OcctObjectId shapeId, const OcctModelLocation* location, int copyShape)
    {
        ModelSession* model = modelOf(handle);
        if (location == nullptr) return 0;
        OcctObjectId result = 0;
        execute(model, [&]
        {
            gp_Trsf transform;
            transform.SetValues(
                location->m11, location->m12, location->m13, location->m14,
                location->m21, location->m22, location->m23, location->m24,
                location->m31, location->m32, location->m33, location->m34);
            TopoDS_Shape located = model->requireShape(shapeId).Located(TopLoc_Location(transform), Standard_False);
            if (copyShape != 0) result = model->addShape(located);
            else { model->shapes[shapeId] = located; result = shapeId; }
        });
        return result;
    }

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

    int occt_model_vertex_point(OcctModelHandle handle, OcctObjectId vertexId, OcctPoint3d* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(vertexId);
            if (shape.ShapeType() != TopAbs_VERTEX) throw std::invalid_argument("Input must be a vertex.");
            const gp_Pnt point = BRep_Tool::Pnt(TopoDS::Vertex(shape));
            *result = {point.X(), point.Y(), point.Z()};
        });
    }

    int occt_model_edge_endpoints(OcctModelHandle handle, OcctObjectId edgeId, OcctPoint3d* start, OcctPoint3d* end)
    {
        ModelSession* model = modelOf(handle);
        if (start == nullptr || end == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            BRepAdaptor_Curve curve(TopoDS::Edge(shape));
            const gp_Pnt first = curve.Value(curve.FirstParameter());
            const gp_Pnt last = curve.Value(curve.LastParameter());
            *start = {first.X(), first.Y(), first.Z()};
            *end = {last.X(), last.Y(), last.Z()};
        });
    }

    int occt_model_edge_point_at(OcctModelHandle handle, OcctObjectId edgeId, double normalizedParameter, OcctPoint3d* resultPoint, OcctVector3d* resultTangent)
    {
        ModelSession* model = modelOf(handle);
        if (resultPoint == nullptr || resultTangent == nullptr) return 0;
        return execute(model, [&]
        {
            if (normalizedParameter < 0.0 || normalizedParameter > 1.0)
                throw std::invalid_argument("Normalized parameter must be between 0 and 1.");
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            BRepAdaptor_Curve curve(TopoDS::Edge(shape));
            const double parameter = curve.FirstParameter() + (curve.LastParameter() - curve.FirstParameter()) * normalizedParameter;
            gp_Pnt point;
            gp_Vec tangent;
            curve.D1(parameter, point, tangent);
            if (tangent.SquareMagnitude() <= Precision::SquareConfusion()) throw std::runtime_error("Edge tangent is undefined at this parameter.");
            tangent.Normalize();
            *resultPoint = {point.X(), point.Y(), point.Z()};
            *resultTangent = {tangent.X(), tangent.Y(), tangent.Z()};
        });
    }

    int occt_model_edge_curve_type(OcctModelHandle handle, OcctObjectId edgeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return OcctCurve_Other;
        try
        {
            const TopoDS_Shape& shape = model->requireShape(edgeId);
            if (shape.ShapeType() != TopAbs_EDGE) throw std::invalid_argument("Input must be an edge.");
            return static_cast<int>(BRepAdaptor_Curve(TopoDS::Edge(shape)).GetType());
        }
        catch (const std::exception& exception)
        {
            model->lastError = exception.what();
            return OcctCurve_Other;
        }
    }

    int occt_model_face_surface_type(OcctModelHandle handle, OcctObjectId faceId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return OcctSurface_Other;
        try
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            return static_cast<int>(BRepAdaptor_Surface(TopoDS::Face(shape), Standard_True).GetType());
        }
        catch (const std::exception& exception)
        {
            model->lastError = exception.what();
            return OcctSurface_Other;
        }
    }

    int occt_model_face_uv_bounds(OcctModelHandle handle, OcctObjectId faceId, OcctUvBounds* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            BRepTools::UVBounds(TopoDS::Face(shape), result->uMin, result->uMax, result->vMin, result->vMax);
        });
    }

    int occt_model_face_point_normal(OcctModelHandle handle, OcctObjectId faceId, double u, double v, OcctPoint3d* resultPoint, OcctVector3d* resultNormal)
    {
        ModelSession* model = modelOf(handle);
        if (resultPoint == nullptr || resultNormal == nullptr) return 0;
        return execute(model, [&]
        {
            const TopoDS_Shape& shape = model->requireShape(faceId);
            if (shape.ShapeType() != TopAbs_FACE) throw std::invalid_argument("Input must be a face.");
            const TopoDS_Face face = TopoDS::Face(shape);
            BRepAdaptor_Surface surface(face, Standard_True);
            gp_Pnt point;
            gp_Vec dU;
            gp_Vec dV;
            surface.D1(u, v, point, dU, dV);
            gp_Vec normal = dU.Crossed(dV);
            if (normal.SquareMagnitude() <= Precision::SquareConfusion()) throw std::runtime_error("Face normal is undefined at this UV position.");
            normal.Normalize();
            if (face.Orientation() == TopAbs_REVERSED) normal.Reverse();
            *resultPoint = {point.X(), point.Y(), point.Z()};
            *resultNormal = {normal.X(), normal.Y(), normal.Z()};
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

    OcctObjectId occt_model_display_in_engine(OcctHandle engineHandle, OcctModelHandle modelHandle, OcctObjectId shapeId, int fit)
    {
        OcctBridge::Engine* engine = OcctBridge::engineOf(engineHandle);
        ModelSession* model = modelOf(modelHandle);
        if (engine == nullptr || model == nullptr) return 0;
        try
        {
            return engine->addShape(model->requireShape(shapeId), fit != 0, "ModelShape");
        }
        catch (const Standard_Failure& failure)
        {
            const char* message = failure.GetMessageString();
            model->lastError = message == nullptr ? "Displaying model shape failed." : message;
        }
        catch (const std::exception& exception)
        {
            model->lastError = exception.what();
        }
        catch (...)
        {
            model->lastError = "Displaying model shape failed.";
        }
        return 0;
    }
}

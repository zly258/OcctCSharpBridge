#include "OcctModelingShapeInternal.hxx"

#include <BRepBndLib.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepExtrema_DistShapeShape.hxx>
#include <BRepGProp.hxx>
#include <Bnd_Box.hxx>
#include <TopLoc_Location.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <gp_Trsf.hxx>

#include <sstream>

using namespace OcctModelingInternal;

extern "C"
{
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
        try { return toOcctShapeType(model->requireShape(shapeId).ShapeType()); }
        catch (...) { return OcctShape_Shape; }
    }

    int occt_model_shape_orientation(OcctModelHandle handle, OcctObjectId shapeId)
    {
        ModelSession* model = modelOf(handle);
        if (model == nullptr) return OcctModelOrientation_Forward;
        try { return toModelOrientation(model->requireShape(shapeId).Orientation()); }
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
                   << ",\"shapeType\":" << toOcctShapeType(shape.ShapeType())
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
                    stream << "{\"type\":" << toOcctShapeType(static_cast<TopAbs_ShapeEnum>(type)) << ",\"index\":" << index << '}';
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
}
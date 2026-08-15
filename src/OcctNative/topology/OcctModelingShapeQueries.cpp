#include "topology/OcctModelingShapeQueries.h"
#include "modeling/OcctModelingShapeInternal.hxx"

#include <BRepBndLib.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepCheck_Shell.hxx>
#include <BRepCheck_Status.hxx>
#include <BRepCheck_Wire.hxx>
#include <BRepExtrema_DistShapeShape.hxx>
#include <BRepGProp.hxx>
#include <Bnd_Box.hxx>
#include <TopLoc_Location.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS_Shell.hxx>
#include <gp_Trsf.hxx>

#include <cstring>
#include <limits>
#include <sstream>
#include <stdexcept>
#include <string>

using namespace OcctModelingInternal;

namespace
{
    bool isTopologicallyClosed(const TopoDS_Shape& shape)
    {
        switch (shape.ShapeType())
        {
            case TopAbs_WIRE:
                return BRepCheck_Wire(TopoDS::Wire(shape)).Closed() == BRepCheck_NoError;

            case TopAbs_SHELL:
                return BRepCheck_Shell(TopoDS::Shell(shape)).Closed() == BRepCheck_NoError;

            case TopAbs_SOLID:
            case TopAbs_COMPSOLID:
            {
                bool hasShell = false;
                for (TopExp_Explorer explorer(shape, TopAbs_SHELL); explorer.More(); explorer.Next())
                {
                    hasShell = true;
                    if (BRepCheck_Shell(TopoDS::Shell(explorer.Current())).Closed() != BRepCheck_NoError)
                        return false;
                }
                return hasShell;
            }

            default:
                return false;
        }
    }

    OcctStatus copyUtf8(
        const std::string& value,
        char* buffer,
        int capacity,
        int* required)
    {
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;
        if (value.size() >= static_cast<std::size_t>(std::numeric_limits<int>::max()))
            return OcctStatus_ErrorOutOfMemory;

        const int size = static_cast<int>(value.size()) + 1;
        *required = size;
        if (buffer == nullptr)
            return capacity == 0 ? OcctStatus_Ok : OcctStatus_ErrorInvalidArgument;
        if (capacity < size) return OcctStatus_ErrorBufferTooSmall;

        std::memcpy(buffer, value.c_str(), static_cast<std::size_t>(size));
        return OcctStatus_Ok;
    }
}

extern "C"
{
    OcctStatus occt_model_shape_hash(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        std::int64_t* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = 0;
        return executeStatus(model, [&]
        {
            *result = static_cast<std::int64_t>(TopTools_ShapeMapHasher{}(model->requireShape(shapeId)));
        });
    }

    OcctStatus occt_model_shape_type(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctShapeType* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = OcctShape_Shape;
        return executeStatus(model, [&]
        {
            *result = static_cast<OcctShapeType>(toOcctShapeType(model->requireShape(shapeId).ShapeType()));
        });
    }

    OcctStatus occt_model_shape_orientation(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctModelOrientation* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = OcctModelOrientation_Forward;
        return executeStatus(model, [&]
        {
            *result = static_cast<OcctModelOrientation>(toModelOrientation(model->requireShape(shapeId).Orientation()));
        });
    }

    OcctStatus occt_model_shape_is_closed(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBool* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = 0;
        return executeStatus(model, [&]
        {
            *result = isTopologicallyClosed(model->requireShape(shapeId)) ? 1 : 0;
        });
    }

    OcctStatus occt_model_shape_is_valid(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBool* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = 0;
        return executeStatus(model, [&]
        {
            *result = BRepCheck_Analyzer(model->requireShape(shapeId), Standard_True).IsValid() ? 1 : 0;
        });
    }

    OcctStatus occt_model_shape_tolerance(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        double* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = 0.0;
        return executeStatus(model, [&]
        {
            *result = maximumTolerance(model->requireShape(shapeId));
        });
    }

    OcctStatus occt_model_shape_bounds(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctBounds* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            Bnd_Box box;
            BRepBndLib::Add(model->requireShape(shapeId), box, Standard_True);
            if (box.IsVoid()) throw std::runtime_error("Shape bounding box is empty.");
            box.Get(result->minX, result->minY, result->minZ, result->maxX, result->maxY, result->maxZ);
        });
    }

    OcctStatus occt_model_shape_linear_properties(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            GProp_GProps properties;
            BRepGProp::LinearProperties(model->requireShape(shapeId), properties);
            fillProperties(properties, result);
        });
    }

    OcctStatus occt_model_shape_surface_properties(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            GProp_GProps properties;
            BRepGProp::SurfaceProperties(model->requireShape(shapeId), properties);
            fillProperties(properties, result);
        });
    }

    OcctStatus occt_model_shape_volume_properties(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            GProp_GProps properties;
            BRepGProp::VolumeProperties(model->requireShape(shapeId), properties);
            fillProperties(properties, result);
        });
    }

    OcctStatus occt_model_shape_distance(
        OcctModelingSessionHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId,
        OcctDistanceResult* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            BRepExtrema_DistShapeShape distance(model->requireShape(firstId), model->requireShape(secondId));
            distance.Perform();
            if (!distance.IsDone() || distance.NbSolution() < 1)
                throw std::runtime_error("Distance calculation failed.");

            const gp_Pnt first = distance.PointOnShape1(1);
            const gp_Pnt second = distance.PointOnShape2(1);
            result->distance = distance.Value();
            result->pointOnFirst = {first.X(), first.Y(), first.Z()};
            result->pointOnSecond = {second.X(), second.Y(), second.Z()};
        });
    }

    OcctStatus occt_model_shape_check_report_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        char* buffer,
        int capacity,
        int* required)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (capacity < 0 || required == nullptr) return OcctStatus_ErrorInvalidArgument;

        std::string report;
        const OcctStatus status = executeStatus(model, [&]
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
                    stream << "{\"type\":" << toOcctShapeType(static_cast<TopAbs_ShapeEnum>(type))
                           << ",\"index\":" << index << '}';
                }
            }
            stream << "]}";
            report = stream.str();
        });
        if (status != OcctStatus_Ok) return status;
        return copyUtf8(report, buffer, capacity, required);
    }

    OcctStatus occt_model_shape_location_get(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        OcctModelLocation* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = {};
        return executeStatus(model, [&]
        {
            const gp_Trsf transform = model->requireShape(shapeId).Location().Transformation();
            result->m11 = transform.Value(1, 1); result->m12 = transform.Value(1, 2); result->m13 = transform.Value(1, 3); result->m14 = transform.Value(1, 4);
            result->m21 = transform.Value(2, 1); result->m22 = transform.Value(2, 2); result->m23 = transform.Value(2, 3); result->m24 = transform.Value(2, 4);
            result->m31 = transform.Value(3, 1); result->m32 = transform.Value(3, 2); result->m33 = transform.Value(3, 3); result->m34 = transform.Value(3, 4);
            result->m41 = 0.0; result->m42 = 0.0; result->m43 = 0.0; result->m44 = 1.0;
        });
    }

    OcctStatus occt_model_shape_location_set(
        OcctModelingSessionHandle handle,
        OcctObjectId shapeId,
        const OcctModelLocation* location,
        OcctBool copyShape,
        OcctObjectId* result)
    {
        ModelSession* model = sessionOf(handle);
        if (model == nullptr) return OcctStatus_ErrorInvalidHandle;
        if (location == nullptr || result == nullptr) return OcctStatus_ErrorInvalidArgument;

        *result = 0;
        return executeStatus(model, [&]
        {
            gp_Trsf transform;
            transform.SetValues(
                location->m11, location->m12, location->m13, location->m14,
                location->m21, location->m22, location->m23, location->m24,
                location->m31, location->m32, location->m33, location->m34);

            TopoDS_Shape located = model->requireShape(shapeId).Located(TopLoc_Location(transform), Standard_False);
            if (copyShape != 0)
                *result = model->addShape(located);
            else
            {
                model->shapes[shapeId] = located;
                *result = shapeId;
            }
        });
    }
}

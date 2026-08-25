#include "geometry/OcctViewerShapeQueries.h"
#include "core/OcctInternal.hxx"

#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepBndLib.hxx>
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepExtrema_DistShapeShape.hxx>
#include <BRepGProp.hxx>
#include <BRep_Tool.hxx>
#include <BRepTools.hxx>
#include <Bnd_Box.hxx>
#include <Precision.hxx>
#include <TopExp_Explorer.hxx>
#include <TopTools_ShapeMapHasher.hxx>
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
    OcctStatus executeShapeStatus(Engine* engine, Function&& function)
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

    template<typename Function>
    OcctStatus createShapeResult(
        Engine* engine,
        OcctObjectId* result,
        Function&& function)
    {
        if (result == nullptr) return OcctStatus_ErrorInvalidArgument;
        *result = 0;
        return executeShapeStatus(engine, [&]
        {
            *result = function();
            if (*result <= 0) throw std::runtime_error("Shape operation did not create a viewer object.");
        });
    }

    TopoDS_Shape subshapeAt(const TopoDS_Shape& owner, int shapeType, int index)
    {
        if (index < 0) throw std::invalid_argument("Subshape index must not be negative.");
        int current = 0;
        for (TopExp_Explorer explorer(owner, shapeEnum(shapeType)); explorer.More(); explorer.Next(), ++current)
        {
            if (current == index) return explorer.Current();
        }
        throw std::out_of_range("Subshape index is out of range.");
    }
}

extern "C"
{
    OcctStatus occt_engine_shape_type_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        int* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Shape type output is null.");
            *result = shapeTypeValue(requiredShape(engine, shapeId).shape);
        });
    }

    OcctStatus occt_engine_shape_validity_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctBool* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Shape validity output is null.");
            *result = BRepCheck_Analyzer(requiredShape(engine, shapeId).shape, Standard_True).IsValid() ? 1 : 0;
        });
    }

    OcctStatus occt_engine_shape_bounds_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctBounds* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Shape bounds output is null.");
            Bnd_Box box;
            BRepBndLib::Add(shapeWithPresentationTransformation(requiredShape(engine, shapeId)), box);
            if (box.IsVoid()) throw std::runtime_error("Shape has no finite bounds.");
            box.Get(result->minX, result->minY, result->minZ, result->maxX, result->maxY, result->maxZ);
        });
    }

    OcctStatus occt_engine_shape_linear_properties_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Linear properties output is null.");
            GProp_GProps properties;
            BRepGProp::LinearProperties(requiredShape(engine, shapeId).shape, properties);
            fillMassProperties(properties, result);
        });
    }

    OcctStatus occt_engine_shape_surface_properties_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Surface properties output is null.");
            GProp_GProps properties;
            BRepGProp::SurfaceProperties(requiredShape(engine, shapeId).shape, properties);
            fillMassProperties(properties, result);
        });
    }

    OcctStatus occt_engine_shape_volume_properties_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctMassProperties* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Volume properties output is null.");
            GProp_GProps properties;
            BRepGProp::VolumeProperties(requiredShape(engine, shapeId).shape, properties);
            fillMassProperties(properties, result);
        });
    }

    OcctStatus occt_engine_shape_distance_get(
        OcctEngineHandle handle,
        OcctObjectId firstId,
        OcctObjectId secondId,
        OcctDistanceResult* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Distance output is null.");
            const TopoDS_Shape first = shapeWithPresentationTransformation(requiredShape(engine, firstId));
            const TopoDS_Shape second = shapeWithPresentationTransformation(requiredShape(engine, secondId));
            BRepExtrema_DistShapeShape distance(first, second);
            distance.Perform();
            if (!distance.IsDone() || distance.NbSolution() < 1)
                throw std::runtime_error("Distance calculation failed.");
            const gp_Pnt p1 = distance.PointOnShape1(1);
            const gp_Pnt p2 = distance.PointOnShape2(1);
            result->distance = distance.Value();
            result->pointOnFirst = {p1.X(), p1.Y(), p1.Z()};
            result->pointOnSecond = {p2.X(), p2.Y(), p2.Z()};
        });
    }

    OcctStatus occt_engine_shape_topology_count_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        int shapeType,
        int* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Topology count output is null.");
            int count = 0;
            const TopoDS_Shape owner = shapeWithPresentationTransformation(requiredShape(engine, shapeId));
            for (TopExp_Explorer explorer(owner, shapeEnum(shapeType)); explorer.More(); explorer.Next()) ++count;
            *result = count;
        });
    }

    OcctStatus occt_engine_shape_subshape_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        int shapeType,
        int index,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createShapeResult(engine, result, [&]
        {
            const TopoDS_Shape owner = shapeWithPresentationTransformation(requiredShape(engine, shapeId));
            return engine->addShape(subshapeAt(owner, shapeType, index), false, "Subshape");
        });
    }

    OcctStatus occt_engine_shape_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createShapeResult(engine, result, [&]
        {
            ObjectEntry& entry = requiredShape(engine, shapeId);
            BRepBuilderAPI_Copy copy(shapeWithPresentationTransformation(entry));
            if (!copy.IsDone()) throw std::runtime_error("Shape copy failed.");
            const OcctObjectId created = engine->addShape(copy.Shape(), false, "Copy");
            if (hideInput != 0) engine->hide(shapeId);
            return created;
        });
    }

    OcctStatus occt_engine_shape_translate_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctVector3d value,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createShapeResult(engine, result, [&]
        {
            gp_Trsf transform;
            transform.SetTranslation(vector(value));
            const OcctObjectId created = engine->addShape(
                transformed(shapeWithPresentationTransformation(requiredShape(engine, shapeId)), transform),
                false,
                "Translated");
            if (hideInput != 0) engine->hide(shapeId);
            return created;
        });
    }

    OcctStatus occt_engine_shape_rotate_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d axisPoint,
        OcctVector3d axisDirection,
        double angleDegrees,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createShapeResult(engine, result, [&]
        {
            if (!std::isfinite(angleDegrees)) throw std::invalid_argument("Rotation angle must be finite.");
            gp_Trsf transform;
            transform.SetRotation(
                gp_Ax1(point(axisPoint), direction(axisDirection)),
                angleDegrees * 3.14159265358979323846 / 180.0);
            const OcctObjectId created = engine->addShape(
                transformed(shapeWithPresentationTransformation(requiredShape(engine, shapeId)), transform),
                false,
                "Rotated");
            if (hideInput != 0) engine->hide(shapeId);
            return created;
        });
    }

    OcctStatus occt_engine_shape_scale_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d center,
        double factor,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createShapeResult(engine, result, [&]
        {
            requirePositive(factor, "Scale factor");
            gp_Trsf transform;
            transform.SetScale(point(center), factor);
            const OcctObjectId created = engine->addShape(
                transformed(shapeWithPresentationTransformation(requiredShape(engine, shapeId)), transform),
                false,
                "Scaled");
            if (hideInput != 0) engine->hide(shapeId);
            return created;
        });
    }

    OcctStatus occt_engine_shape_mirror_plane_copy(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        OcctPoint3d planePoint,
        OcctVector3d planeNormal,
        OcctBool hideInput,
        OcctObjectId* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return createShapeResult(engine, result, [&]
        {
            gp_Trsf transform;
            transform.SetMirror(gp_Ax2(point(planePoint), direction(planeNormal)));
            const OcctObjectId created = engine->addShape(
                transformed(shapeWithPresentationTransformation(requiredShape(engine, shapeId)), transform),
                false,
                "Mirrored");
            if (hideInput != 0) engine->hide(shapeId);
            return created;
        });
    }

    OcctStatus occt_engine_shape_hash_get(
        OcctEngineHandle handle,
        OcctObjectId shapeId,
        std::int64_t* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Shape hash output is null.");
            *result = static_cast<std::int64_t>(TopTools_ShapeMapHasher{}(requiredShape(engine, shapeId).shape));
        });
    }

    OcctStatus occt_engine_shape_vertex_point_get(
        OcctEngineHandle handle,
        OcctObjectId vertexId,
        OcctPoint3d* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Vertex point output is null.");
            ObjectEntry& entry = requiredShape(engine, vertexId);
            if (entry.shape.ShapeType() != TopAbs_VERTEX)
                throw std::invalid_argument("Input must be a vertex.");
            const gp_Pnt value = BRep_Tool::Pnt(TopoDS::Vertex(entry.shape));
            *result = {value.X(), value.Y(), value.Z()};
        });
    }

    OcctStatus occt_engine_shape_edge_endpoints_get(
        OcctEngineHandle handle,
        OcctObjectId edgeId,
        OcctPoint3d* start,
        OcctPoint3d* end)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (start == nullptr || end == nullptr) throw std::invalid_argument("Edge endpoint output is null.");
            ObjectEntry& entry = requiredShape(engine, edgeId);
            if (entry.shape.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Input must be an edge.");
            BRepAdaptor_Curve curve(TopoDS::Edge(entry.shape));
            const gp_Pnt first = curve.Value(curve.FirstParameter());
            const gp_Pnt last = curve.Value(curve.LastParameter());
            *start = {first.X(), first.Y(), first.Z()};
            *end = {last.X(), last.Y(), last.Z()};
        });
    }

    OcctStatus occt_engine_shape_edge_evaluate(
        OcctEngineHandle handle,
        OcctObjectId edgeId,
        double normalizedParameter,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultTangent)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (resultPoint == nullptr || resultTangent == nullptr)
                throw std::invalid_argument("Edge evaluation output is null.");
            if (!std::isfinite(normalizedParameter) || normalizedParameter < 0.0 || normalizedParameter > 1.0)
                throw std::invalid_argument("Normalized edge parameter must be between 0 and 1.");
            ObjectEntry& entry = requiredShape(engine, edgeId);
            if (entry.shape.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Input must be an edge.");
            BRepAdaptor_Curve curve(TopoDS::Edge(entry.shape));
            const double parameter = curve.FirstParameter()
                + (curve.LastParameter() - curve.FirstParameter()) * normalizedParameter;
            gp_Pnt value;
            gp_Vec tangent;
            curve.D1(parameter, value, tangent);
            if (tangent.SquareMagnitude() <= Precision::SquareConfusion())
                throw std::runtime_error("Edge tangent is undefined at this parameter.");
            tangent.Normalize();
            *resultPoint = {value.X(), value.Y(), value.Z()};
            *resultTangent = {tangent.X(), tangent.Y(), tangent.Z()};
        });
    }

    OcctStatus occt_engine_shape_edge_curve_type_get(
        OcctEngineHandle handle,
        OcctObjectId edgeId,
        int* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Edge curve type output is null.");
            ObjectEntry& entry = requiredShape(engine, edgeId);
            if (entry.shape.ShapeType() != TopAbs_EDGE)
                throw std::invalid_argument("Input must be an edge.");
            *result = static_cast<int>(BRepAdaptor_Curve(TopoDS::Edge(entry.shape)).GetType());
        });
    }

    OcctStatus occt_engine_shape_face_surface_type_get(
        OcctEngineHandle handle,
        OcctObjectId faceId,
        int* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Face surface type output is null.");
            ObjectEntry& entry = requiredShape(engine, faceId);
            if (entry.shape.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Input must be a face.");
            *result = static_cast<int>(BRepAdaptor_Surface(TopoDS::Face(entry.shape), Standard_True).GetType());
        });
    }

    OcctStatus occt_engine_shape_face_uv_bounds_get(
        OcctEngineHandle handle,
        OcctObjectId faceId,
        OcctUvBounds* result)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (result == nullptr) throw std::invalid_argument("Face UV bounds output is null.");
            ObjectEntry& entry = requiredShape(engine, faceId);
            if (entry.shape.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Input must be a face.");
            BRepTools::UVBounds(
                TopoDS::Face(entry.shape),
                result->uMin,
                result->uMax,
                result->vMin,
                result->vMax);
        });
    }

    OcctStatus occt_engine_shape_face_evaluate(
        OcctEngineHandle handle,
        OcctObjectId faceId,
        double u,
        double v,
        OcctPoint3d* resultPoint,
        OcctVector3d* resultNormal)
    {
        Engine* engine = reinterpret_cast<Engine*>(handle);
        return executeShapeStatus(engine, [&]
        {
            if (resultPoint == nullptr || resultNormal == nullptr)
                throw std::invalid_argument("Face evaluation output is null.");
            if (!std::isfinite(u) || !std::isfinite(v))
                throw std::invalid_argument("Face parameters must be finite.");
            ObjectEntry& entry = requiredShape(engine, faceId);
            if (entry.shape.ShapeType() != TopAbs_FACE)
                throw std::invalid_argument("Input must be a face.");
            BRepAdaptor_Surface surface(TopoDS::Face(entry.shape), Standard_True);
            gp_Pnt value;
            gp_Vec du;
            gp_Vec dv;
            surface.D1(u, v, value, du, dv);
            gp_Vec normal = du.Crossed(dv);
            if (normal.SquareMagnitude() <= Precision::SquareConfusion())
                throw std::runtime_error("Face normal is undefined at this UV position.");
            if (entry.shape.Orientation() == TopAbs_REVERSED) normal.Reverse();
            normal.Normalize();
            *resultPoint = {value.X(), value.Y(), value.Z()};
            *resultNormal = {normal.X(), normal.Y(), normal.Z()};
        });
    }
}
